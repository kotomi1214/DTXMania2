using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SharpDX;
using FDK;

namespace DTXMania.起動
{
    class 起動ステージ : ステージ
    {
        public enum フェーズ
        {
            開始,
            グローバルリソース生成中,
            システムサウンド構築中,
            曲ツリー構築中,
            開始音終了待ち,
            完了,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; } = フェーズ.完了;



        // 生成と終了


        public 起動ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public override void OnDispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                base.OnDispose();
            }
        }



        // 活性化と非活性化


        public override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._コンソールフォント = new 画像フォント( @"$(System)images\コンソールフォント20x32.png", @"$(System)images\コンソールフォント20x32.yaml", 文字幅補正dpx: -6f );

                if( !AppForm.ビュアーモードである )
                {
                    App進行描画.システムサウンド.読み込む( システムサウンド種別.認証ステージ_開始音 );     // この２つを先行読み込み。残りはシステムサウンド構築フェーズにて。 
                    App進行描画.システムサウンド.読み込む( システムサウンド種別.認証ステージ_ループBGM );  //

                    App進行描画.システムサウンド.再生する( システムサウンド種別.起動ステージ_開始音 );
                    App進行描画.システムサウンド.再生する( システムサウンド種別.起動ステージ_ループBGM, ループ再生する: true );
                }

                this._コンソール表示内容 = new List<string>() {
                    $"{AppForm.属性<AssemblyTitleAttribute>().Title} Release {AppForm.リリース番号:000} - Beats with your heart.",
                    $"{AppForm.属性<AssemblyCopyrightAttribute>().Copyright}",
                    $"",
                };

                this.現在のフェーズ = フェーズ.開始;

                base.On活性化();
            }
        }

        public override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App進行描画.システムサウンド.停止する( システムサウンド種別.起動ステージ_開始音 );
                App進行描画.システムサウンド.停止する( システムサウンド種別.起動ステージ_ループBGM );

                this._コンソールフォント?.Dispose();

                this.現在のフェーズ = フェーズ.完了;

                base.On非活性化();
            }
        }



        // 進行と描画


        public override void 進行する()
        {
            // 進行

            switch( this.現在のフェーズ )
            {
                case フェーズ.開始:
                    #region " 一度描画処理を通してから（画面を表示させてから）次のフェーズへ。"
                    //----------------
                    // 次のフェーズへ。
                    this.現在のフェーズ = フェーズ.グローバルリソース生成中;
                    //----------------
                    #endregion
                    break;

                case フェーズ.グローバルリソース生成中:
                    #region " グローバルリソースを生成する。"
                    //----------------
                    App進行描画.Instance.グローバルリソースを生成する();  // 同期処理；この間描画もブロックされる

                    // 次のフェーズへ。
                    this.現在のフェーズ = フェーズ.システムサウンド構築中;
                    //----------------
                    #endregion
                    break;

                case フェーズ.システムサウンド構築中:
                    #region " システムサウンドとドラムサウンドを構築する。"
                    //----------------
                    Task.Run( () => {   // 並列処理

                        App進行描画.システムサウンド.読み込む();
                        Log.Info( "システムサウンドの読み込みが完了しました。" );

                        App進行描画.ドラムサウンド.読み込む();
                        Log.Info( "ドラムサウンドの読み込みが完了しました。" );

                    } );

                    // 処理の完了を待たず、次のフェーズへ。
                    this.現在のフェーズ = フェーズ.曲ツリー構築中;
                    //----------------
                    #endregion
                    break;

                case フェーズ.曲ツリー構築中:
                    #region " 曲検索タスクを起動する。"
                    //----------------
                    if( AppForm.ビュアーモードである )
                    {
                        // (A) ビュアーモードでは曲ツリーを使わないので、構築しない。
                    }
                    else
                    {
                        // (B) 通常モードでは曲ツリーの更新を開始する。

                        App進行描画.曲ツリー?.Dispose();
                        App進行描画.曲ツリー = new 曲ツリー();

                        // 先頭にはランダムノード。
                        App進行描画.曲ツリー.ランダムセレクトノードを追加する( App進行描画.曲ツリー.ルートノード );

                        // システム設定に指定されたすべての曲検索フォルダについて、曲の検索を開始する。
                        foreach( var varpath in App進行描画.システム設定.曲検索フォルダ )
                            App進行描画.曲ツリー.曲の検索を開始する( varpath );  // 並列処理
                    }

                    this._コンソール表示内容.Add( "" );

                    // 次のフェーズへ。
                    this.現在のフェーズ = フェーズ.開始音終了待ち;
                    //----------------
                    #endregion
                    break;

                case フェーズ.開始音終了待ち:
                    #region " 開始音がまだ鳴っていれば、終了するまで待つ。"
                    //----------------
                    if( !App進行描画.システムサウンド.再生中( システムサウンド種別.起動ステージ_開始音 ) )
                    {
                        // 再生が終わったので次のフェーズへ。
                        this.現在のフェーズ = フェーズ.完了;
                    }
                    //----------------
                    #endregion
                    break;

                case フェーズ.完了:
                    break;
            }
        }

        public override void 描画する()
        {
            var dc = DXResources.Instance.既定のD2D1DeviceContext;
            dc.Transform = DXResources.Instance.拡大行列DPXtoPX;

            // 文字列表示
            for( int i = 0; i < this._コンソール表示内容.Count; i++ )
                this._コンソールフォント.描画する( dc, 0f, i * 32f, this._コンソール表示内容[ i ] );
        }



        // private


        private 画像フォント _コンソールフォント = null;

        private List<string> _コンソール表示内容 = null;
    }
}

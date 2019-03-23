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
            ドラムサウンド構築中,
            曲ツリー構築中,
            開始音終了待ち,
            完了,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; }



        // 生成と終了


        public 起動ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public override void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( this.活性化中 )
                    this.非活性化する();
            }
        }



        // 活性化と非活性化


        public override void 活性化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( this.活性化中 )
                    return;

                this.活性化中 = true;

                this._コンソールフォント = new 画像フォント( @"$(System)images\コンソールフォント20x32.png", @"$(System)images\コンソールフォント20x32.yaml", 文字幅補正dpx: -6f );

                if( !App.ビュアーモードである )
                {
                    App進行描画.システムサウンド.読み込む( システムサウンド種別.認証ステージ_開始音 );     // この２つを先行読み込み。残りはシステムサウンド構築フェーズにて。 
                    App進行描画.システムサウンド.読み込む( システムサウンド種別.認証ステージ_ループBGM );  //

                    App進行描画.システムサウンド.再生する( システムサウンド種別.起動ステージ_開始音 );
                    App進行描画.システムサウンド.再生する( システムサウンド種別.起動ステージ_ループBGM, ループ再生する: true );
                }

                this._コンソール表示内容 = new List<string>() {
                    $"{App.属性<AssemblyTitleAttribute>().Title} r{App.リリース番号:000}",
                    $"{App.属性<AssemblyCopyrightAttribute>().Copyright}",
                    $"",
                };

                this.現在のフェーズ = フェーズ.開始;


                base.活性化する();
            }
        }

        public override void 非活性化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( !this.活性化中 )
                    return;

                this.活性化中 = false;

                App進行描画.システムサウンド.停止する( システムサウンド種別.起動ステージ_開始音 );
                App進行描画.システムサウンド.停止する( システムサウンド種別.起動ステージ_ループBGM );

                this._コンソールフォント?.Dispose();

                this.現在のフェーズ = フェーズ.完了;


                base.非活性化する();
            }
        }



        // 進行と描画


        public override void 進行する()
        {
            // 進行

            switch( this.現在のフェーズ )
            {
                case フェーズ.開始:
                    //this._コンソール表示内容.Add( $"Loading and decoding sounds of system ..." );
                    this.現在のフェーズ = フェーズ.グローバルリソース生成中; // 一度描画処理を通してから（画面を表示させてから）次のフェーズへ。
                    break;

                case フェーズ.グローバルリソース生成中:
                    App進行描画.Instance.グローバルリソースを生成する();  // 同期処理；この間描画もブロックされる
                    this.現在のフェーズ = フェーズ.システムサウンド構築中;
                    break;

                case フェーズ.システムサウンド構築中:
                    #region " システムサウンドを構築する。"
                    //----------------
                    Task.Run( () => {
                        App進行描画.システムサウンド.読み込む();
                        Log.Info( "システムサウンドの読み込みが完了しました。" );
                    } );

                    //this._コンソール表示内容.RemoveAt( this._コンソール表示内容.Count - 1 );
                    //this._コンソール表示内容.Add( $"Loading and decoding sounds of system ... done." );
                    //this._コンソール表示内容.Add( $"Loading and decoding sounds of drums ..." );

                    this.現在のフェーズ = フェーズ.ドラムサウンド構築中;
                    //----------------
                    #endregion
                    break;

                case フェーズ.ドラムサウンド構築中:
                    #region " ドラムサウンドを構築する。"
                    //----------------
                    Task.Run( () => {
                        App進行描画.ドラムサウンド.読み込む();
                        Log.Info( "ドラムサウンドの読み込みが完了しました。" );
                    } );

                    //this._コンソール表示内容.RemoveAt( this._コンソール表示内容.Count - 1 );
                    //this._コンソール表示内容.Add( $"Loading and decoding sounds of drums ... done." );

                    this.現在のフェーズ = フェーズ.曲ツリー構築中;
                    //----------------
                    #endregion
                    break;

                case フェーズ.曲ツリー構築中:
                    #region " 曲検索タスクを起動する。"
                    //----------------
                    if( App.ビュアーモードである )
                    {
                        // ビュアーモードならスキップ。
                    }
                    else
                    {
                        App進行描画.曲ツリー?.Dispose();
                        App進行描画.曲ツリー = new 曲ツリー();

                        // 曲ツリーの構築を開始。
                        foreach( var varpath in App進行描画.システム設定.曲検索フォルダ )
                            App進行描画.曲ツリー.曲の検索を開始する( varpath );
                    }

                    this._コンソール表示内容.Add( "" );

                    this.現在のフェーズ = フェーズ.開始音終了待ち;
                    //----------------
                    #endregion
                    break;

                case フェーズ.開始音終了待ち:
                    #region " 起動ステージ_開始音 が終了するまで待つ。"
                    //----------------
                    if( !App進行描画.システムサウンド.再生中( システムサウンド種別.起動ステージ_開始音 ) )
                    {
                        this.現在のフェーズ = フェーズ.完了; // 再生が終わったのでフェーズ遷移。
                    }
                    //----------------
                    #endregion
                    break;
            }
        }

        public override void 描画する()
        {
            var dc = グラフィックデバイス.Instance.既定のD2D1DeviceContext;
            dc.Transform = グラフィックデバイス.Instance.拡大行列DPXtoPX;

            // 文字列表示
            for( int i = 0; i < this._コンソール表示内容.Count; i++ )
                this._コンソールフォント.描画する( dc, 0f, i * 32f, this._コンソール表示内容[ i ] );
        }



        // private


        private 画像フォント _コンソールフォント = null;

        private List<string> _コンソール表示内容 = null;
    }
}

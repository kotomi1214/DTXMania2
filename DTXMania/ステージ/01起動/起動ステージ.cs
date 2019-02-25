using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.起動
{
    /// <summary>
    ///     起動画面を表示し、ドラムサウンドと曲ツリーを構築する。
    /// </summary>
    class 起動ステージ : ステージ
    {
        public enum フェーズ
        {
            開始,
            曲ツリー構築中,
            ドラムサウンド構築中,
            開始音終了待ち,
            確定,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; }


        public 起動ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._コンソールフォント = new 画像フォント( @"$(System)images\コンソールフォント20x32.png", @"$(System)images\コンソールフォント20x32.yaml", 文字幅補正dpx: -6f ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( !App.ビュアーモードである )
                {
                    App.システムサウンド.再生する( 設定.システムサウンド種別.起動ステージ_開始音 );
                    App.システムサウンド.再生する( 設定.システムサウンド種別.起動ステージ_ループBGM, ループ再生する: true );
                }

                this._コンソール表示内容 = new List<string>() {
                    $"{App.属性<AssemblyTitleAttribute>().Title} r{App.リリース番号:000}",
                    $"{App.属性<AssemblyCopyrightAttribute>().Copyright}",
                    $"",
                };

                this.現在のフェーズ = フェーズ.開始;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.システムサウンド.停止する( 設定.システムサウンド種別.起動ステージ_開始音 );
                App.システムサウンド.停止する( 設定.システムサウンド種別.起動ステージ_ループBGM );
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            // 進行描画

            for( int i = 0; i < this._コンソール表示内容.Count; i++ )
                this._コンソールフォント.描画する( dc, 0f, i * 32f, this._コンソール表示内容[ i ] );

            switch( this.現在のフェーズ )
            {
                case フェーズ.開始:
                    #region " 曲検索タスクを起動し、構築中フェーズへ遷移する。"
                    //----------------
                    if( App.ビュアーモードである )
                    {
                        // ビュアーモードならスキップ。
                    }
                    else
                    {
                        App.曲ツリー.非活性化する();
                        App.曲ツリー = new 曲.曲ツリー();
                        App.曲ツリー.活性化する();
                        App.曲ツリー.非活性化する();
                        this._ファイル検出数 = 0;

                        this._構築タスク = Task.Factory.StartNew( () => {

                            // 曲ツリーを構築する。

                            foreach( var varpath in App.システム設定.曲検索フォルダ )
                            {
                                App.曲ツリー.曲を検索して親ノードに追加する( App.曲ツリー.ルートノード, varpath, ( vpath ) => {
                                    Interlocked.Increment( ref this._ファイル検出数 );
                                } );
                            }

                            // 構築終了。
                        } );
                    }

                    this._コンソール表示内容.Add( "" );

                    this.現在のフェーズ = フェーズ.曲ツリー構築中;
                    //----------------
                    #endregion
                    break;

                case フェーズ.曲ツリー構築中:
                    #region " 曲ツリー構築タスクの進捗情報を画面に表示する。"
                    //----------------
                    if( App.ビュアーモードである )
                    {
                        // ビュアーモードならスキップ。
                        this._コンソール表示内容.Add( $"Loading and decoding sounds ..." );
                        this.現在のフェーズ = フェーズ.ドラムサウンド構築中;
                    }
                    else
                    {
                        // 構築タスクの進捗表示。
                        this._コンソール表示内容.RemoveAt( this._コンソール表示内容.Count - 1 );
                        this._コンソール表示内容.Add( $"Enumerating and loading score properties from file ... {Interlocked.Read( ref this._ファイル検出数 )}" );

                        // 構築タスクが完了したら、次のフェーズへ遷移する。
                        if( this._構築タスク.IsCompleted || this._構築タスク.IsCanceled )
                        {
                            App.曲ツリー.活性化する();

                            this._コンソール表示内容.Add( $"Loading and decoding sounds ..." );
                            this.現在のフェーズ = フェーズ.ドラムサウンド構築中;
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case フェーズ.ドラムサウンド構築中:
                    #region " ドラムサウンドを構築する。"
                    //----------------
                    {
                        App.ドラムサウンド.初期化する();

                        this._コンソール表示内容.RemoveAt( this._コンソール表示内容.Count - 1 );
                        this._コンソール表示内容.Add( $"Loading and decoding sounds ... done." );

                        this.現在のフェーズ = フェーズ.開始音終了待ち;
                    }
                    //----------------
                    #endregion
                    break;

                case フェーズ.開始音終了待ち:
                    #region " 起動ステージ_開始音 が終了するまで待つ。"
                    //----------------
                    if( !App.システムサウンド.再生中( 設定.システムサウンド種別.起動ステージ_開始音 ) )
                        this.現在のフェーズ = フェーズ.確定; // 再生が終わったのでフェーズ遷移。
                    //----------------
                    #endregion
                    break;

                case フェーズ.確定:
                case フェーズ.キャンセル:
                    break;
            }


            // 入力

            App.入力管理.すべての入力デバイスをポーリングする();

            if( App.入力管理.キャンセルキーが入力された() )
            {
                this.現在のフェーズ = フェーズ.キャンセル;
            }
        }


        private 画像フォント _コンソールフォント = null;

        private List<string> _コンソール表示内容 = null;

        private Task _構築タスク = null;

        private long _ファイル検出数 = 0;
    }
}
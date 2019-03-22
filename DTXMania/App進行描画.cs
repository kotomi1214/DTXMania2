using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using SSTFormat.v4;
using FDK;

namespace DTXMania
{
    class App進行描画 : FDK.進行描画
    {

        // グローバルリソース(static)

        public static App進行描画 Instance { get; protected set; }

        public static Random 乱数 { get; protected set; }

        public static システム設定 システム設定 { get; set; }

        public static サウンドデバイス サウンドデバイス { get; protected set; }

        public static SoundTimer サウンドタイマ { get; protected set; }

        public static システムサウンド システムサウンド { get; protected set; }

        public static ユーザ管理 ユーザ管理 { get; protected set; }

        public static キャッシュデータレンタル<CSCore.ISampleSource> WAVキャッシュレンタル { get; protected set; }

        public static 入力管理 入力管理 { get; set; }

        public static ドラムサウンド ドラムサウンド { get; protected set; }

        public static 曲ツリー 曲ツリー { get; set; }   // ビュアーモード時は未使用。

        public static アイキャッチ管理 アイキャッチ管理 { get; protected set; }



        // 演奏ごとのプロパティ(static)


        public static MusicNode ビュアー用曲ノード { get; set; } // ビュアーモード時のみ使用。

        public static MusicNode 演奏曲ノード
            => App.ビュアーモードである ? App進行描画.ビュアー用曲ノード : App進行描画.曲ツリー.フォーカス曲ノード; // MusicNode 以外は null が返される

        /// <summary>
        ///     現在演奏中のスコア。
        ///     選曲画面で選曲するたびに更新される。
        /// </summary>
        public static スコア 演奏スコア { get; set; }

        /// <summary>
        ///     <see cref="演奏スコア"/> に対応して生成されたWAVサウンドインスタンスの管理。
        /// </summary>
        public static WAV管理 WAV管理 { get; set; }

        /// <summary>
        ///     <see cref="演奏スコア"/>  に対応して生成されたAVI動画インスタンスの管理。
        /// </summary>
        public static AVI管理 AVI管理 { get; set; }



        // 生成と終了


        public App進行描画()
        {
            App進行描画.Instance = this;
        }

        protected override void On開始する()
        {
            // グローバルリソースを生成。（最低限。残りは起動ステージから グローバルリソースを生成する() が呼び出されたときにおこなれる。）

            App進行描画.乱数 = new Random( DateTime.Now.Millisecond );
            //App進行描画.システム設定 = システム設定.読み込む();   --> App() で初期化する。
            App進行描画.サウンドデバイス = new サウンドデバイス( CSCore.CoreAudioAPI.AudioClientShareMode.Shared ) {
                音量 = 0.5f, // マスタ音量（小:0～1:大）... 0.5を超えるとだいたいWASAPI共有モードのリミッターに抑制されるようになる
            };
            App進行描画.システムサウンド = new システムサウンド();
            //App進行描画.システムサウンド.読み込む();  --> 起動ステージで行う。
            App進行描画.入力管理 = new 入力管理( this.AppForm.キーボード ) {
                キーバインディングを取得する = () => App進行描画.システム設定.キー割り当て,
                キーバインディングを保存する = () => App進行描画.システム設定.保存する(),
            };
            App進行描画.入力管理.初期化する();


            // ステージを生成。（起動ステージのみ。残りは起動ステージから グローバルリソースを生成する() が呼び出されたときにおこなれる。）

            this.起動ステージ = new 起動ステージ();


            // 最初のステージを設定し、活性化する。

            this.現在のステージ = this.起動ステージ;
            this.現在のステージ.活性化する();
            ;
        }

        // 起動ステージから呼び出される。
        public void グローバルリソースを生成する()
        {
            テクスチャ.全インスタンスで共有するリソースを作成する();

            App進行描画.サウンドタイマ = new SoundTimer( App進行描画.サウンドデバイス );
            App進行描画.ユーザ管理 = new ユーザ管理();
            App進行描画.ユーザ管理.ユーザリスト.SelectItem( ( user ) => ( user.ユーザID == "AutoPlayer" ) );  // ひとまずAutoPlayerを選択。
            App進行描画.WAVキャッシュレンタル = new キャッシュデータレンタル<CSCore.ISampleSource>() {
                ファイルからデータを生成する = ( path ) => SampleSourceFactory.Create( App進行描画.サウンドデバイス, path, App進行描画.ユーザ管理.ログオン中のユーザ.再生速度 ),
            };
            App進行描画.ドラムサウンド = new ドラムサウンド();
            App進行描画.アイキャッチ管理 = new アイキャッチ管理();


            // 起動ステージ以外のステージを生成。

            this.タイトルステージ = new タイトルステージ();
            this.認証ステージ = new 認証ステージ();
            this.選曲ステージ = new 選曲ステージ();
            this.オプション設定ステージ = new オプション設定ステージ();
            this.曲読み込みステージ = new 曲読み込みステージ();
            this.終了ステージ = new 終了ステージ();
        }

        protected override void On終了する()
        {
            this.現在のステージ = null;


            // ステージを解放。

            this.起動ステージ?.Dispose();
            this.タイトルステージ?.Dispose();
            this.認証ステージ?.Dispose();
            this.選曲ステージ?.Dispose();
            this.オプション設定ステージ?.Dispose();
            this.曲読み込みステージ?.Dispose();
            this.終了ステージ?.Dispose();


            // グローバルリソースを解放。

            App進行描画.アイキャッチ管理?.Dispose();
            App進行描画.ドラムサウンド?.Dispose();
            App進行描画.入力管理?.Dispose();
            App進行描画.WAVキャッシュレンタル?.Dispose();
            App進行描画.ユーザ管理?.Dispose();
            App進行描画.システムサウンド?.Dispose();
            App進行描画.サウンドタイマ?.Dispose();
            App進行描画.サウンドデバイス?.Dispose();

            テクスチャ.全インスタンスで共有するリソースを解放する();

            App進行描画.Instance = null;
        }

        private void _アプリを終了する()
        {
            this.AppForm.BeginInvoke( new Action( () => {
                this.AppForm.Close();
            } ) );
        }

        internal static void ユーザ管理を再構築する()
        {
            ユーザ管理?.Dispose();
            ユーザ管理 = new ユーザ管理();
        }



        // 進行と描画


        protected ステージ 現在のステージ;

        protected override void 進行する()
        {
            // ステージを進行する。

            this.現在のステージ?.進行する();

            
            // 進行結果により処理分岐。

            switch( this.現在のステージ )
            {
                case 起動ステージ stage:
                    #region " キャンセル → アプリ終了 "
                    //----------------
                    if( stage.現在のフェーズ == 起動ステージ.フェーズ.キャンセル )
                    {
                        this._アプリを終了する();
                    }
                    //----------------
                    #endregion
                    #region " 完了 → 通常時はタイトルステージ、ビュアーモード時は演奏ステージ_ビュアーモードへ "
                    //----------------
                    if( stage.現在のフェーズ == 起動ステージ.フェーズ.完了 )
                    {
                        stage.非活性化する();

                        if( App.ビュアーモードである )
                        {
                            // (A) ビュアーモードなら 演奏ステージ_ビュアーモード へ
                            // undone: 演奏ステージ_ビュアーモード へ
                            //this.現在のステージ = this.演奏ステージ_ビュアーモード;
                        }
                        else
                        {
                            // (B) 通常時はタイトルステージへ
                            this.現在のステージ = this.タイトルステージ;
                        }

                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    break;

                case タイトルステージ stage:
                    #region " キャンセル → 終了ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == タイトルステージ.フェーズ.キャンセル )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.終了ステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    #region " 完了 → 認証ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == タイトルステージ.フェーズ.完了 )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.認証ステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    break;

                case 認証ステージ stage:
                    #region " キャンセル → タイトルステージへ "
                    //----------------
                    if( stage.現在のフェーズ == 認証ステージ.フェーズ.キャンセル )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.タイトルステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    #region " 完了 → 選曲ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == 認証ステージ.フェーズ.完了 )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.選曲ステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    break;

                case 選曲ステージ stage:
                    #region " キャンセル → タイトルステージへ "
                    //----------------
                    if( stage.現在のフェーズ == 選曲ステージ.フェーズ.キャンセル )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.タイトルステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    #region " 確定_選曲 → 曲読み込みステージへ "
                    //----------------
                    if( stage.現在のフェーズ == 選曲ステージ.フェーズ.確定_選曲 )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.曲読み込みステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    #region " 確定_設定 → 設定ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == 選曲ステージ.フェーズ.確定_設定 )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.オプション設定ステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    break;

                case オプション設定ステージ stage:
                    #region " キャンセル/完了 → 選曲ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == オプション設定ステージ.フェーズ.キャンセル ||
                        stage.現在のフェーズ == オプション設定ステージ.フェーズ.完了 )
                    {
                        stage.非活性化する();
                        this.現在のステージ = this.選曲ステージ;
                        this.現在のステージ.活性化する();
                    }
                    //----------------
                    #endregion
                    break;

                case 曲読み込みステージ stage:
                    break;

                case 終了ステージ stage:
                    #region " 完了 → アプリ終了 "
                    //----------------
                    if( stage.現在のフェーズ == 終了ステージ.フェーズ.完了 )
                    {
                        this._アプリを終了する();
                    }
                    //----------------
                    #endregion
                    break;
            }
        }

        protected override void 描画する()
        {
            #region " 画面クリア "
            //----------------
            // 既定のD3Dレンダーターゲットビューを黒でクリアする。
            グラフィックデバイス.Instance.D3D11Device1.ImmediateContext.ClearRenderTargetView( グラフィックデバイス.Instance.既定のD3D11RenderTargetView, Color4.Black );

            // 深度バッファを 1.0f でクリアする。
            グラフィックデバイス.Instance.D3D11Device1.ImmediateContext.ClearDepthStencilView(
                グラフィックデバイス.Instance.既定のD3D11DepthStencilView,
                SharpDX.Direct3D11.DepthStencilClearFlags.Depth,
                depth: 1.0f,
                stencil: 0 );
            //----------------
            #endregion

            this.現在のステージ.描画する();
        }



        // ステージ


        protected 起動ステージ 起動ステージ;

        protected タイトルステージ タイトルステージ;

        protected 認証ステージ 認証ステージ;

        protected 選曲ステージ 選曲ステージ;

        protected オプション設定ステージ オプション設定ステージ;

        protected 曲読み込みステージ 曲読み込みステージ;

        protected 終了ステージ 終了ステージ;



        // サイズ変更


        protected override void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            this.現在のステージ?.スワップチェーンに依存するグラフィックリソースを復元する();
        }

        protected override void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            this.現在のステージ?.スワップチェーンに依存するグラフィックリソースを解放する();
        }



        // IDTXManiaService の実装


        #region " IDTXManiaService.ViewerPlay "
        //----------------
        public AutoResetEvent ViewerPlay( string path, int startPart = 0, bool drumsSound = true )
        {
            var msg = new ViewerPlayメッセージ {
                path = path,
                startPart = startPart,
                drumSound = drumsSound,
            };
            this._メッセージキュー.Enqueue( msg );
            return msg.完了通知;
        }

        private class ViewerPlayメッセージ : 通知メッセージ
        {
            public string path = "";
            public int startPart = 0;
            public bool drumSound = true;
        }

        private void _ViewrePlay( ViewerPlayメッセージ msg )
        {
            // undone: ViewerPlay の実装
            throw new NotImplementedException();
        }
        //----------------
        #endregion

        #region " IDTXManiaService.ViewerStop "
        //----------------
        public AutoResetEvent ViewerStop()
        {
            var msg = new ViewerStopメッセージ();
            this._メッセージキュー.Enqueue( msg );
            return msg.完了通知;
        }

        private class ViewerStopメッセージ : 通知メッセージ
        {
        }

        private void _ViewerStop( ViewerStopメッセージ msg )
        {
            // undone: ViewerStop の実装
            throw new NotImplementedException();
        }
        //----------------
        #endregion

        #region " IDTXManiaService.GetSoundDelay "
        //----------------
        public float GetSoundDelay()    // 常に同期
        {
            // undone: GetSoundDelay の実装
            throw new NotImplementedException();
        }
        //----------------
        #endregion
    }
}

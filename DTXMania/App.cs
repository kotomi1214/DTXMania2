using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using SharpDX;
using FDK;
using SSTFormat.v4;
using DTXMania.ステージ;
using DTXMania.曲;
using DTXMania.設定;
using DTXMania.入力;
using DTXMania.API;

namespace DTXMania
{
    [ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]   // サービスインターフェースをシングルスレッドで呼び出す。
    partial class App : ApplicationForm
    {
        public static int リリース番号
            => int.TryParse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ), out int release ) ? release : throw new Exception( "アセンブリのプロダクトバージョンに記載ミスがあります。" );

        public static T 属性<T>() where T : Attribute
            => (T) Attribute.GetCustomAttribute( Assembly.GetExecutingAssembly(), typeof( T ) );

        public static bool ビュアーモードである { get; protected set; }

        public static Random 乱数 { get; protected set; }

        public static システム設定 システム設定 { get; protected set; }

        public static システムサウンド システムサウンド { get; protected set; }

        public static 入力管理 入力管理 { get; set; }

        public static ステージ管理 ステージ管理 { get; protected set; }

        public static SoundDevice サウンドデバイス { get; protected set; }

        public static SoundTimer サウンドタイマ { get; protected set; }

        public static ドラムサウンド ドラムサウンド { get; protected set; }

        public static ユーザ管理 ユーザ管理 { get; protected set; }

        
        public static 曲ツリー 曲ツリー { get; set; }            // ビュアーモード時は未使用。

        public static MusicNode ビュアー用曲ノード { get; set; } // ビュアーモード時のみ使用。

        public static MusicNode 演奏曲ノード
            => App.ビュアーモードである ? App.ビュアー用曲ノード : App.曲ツリー.フォーカス曲ノード; // MusicNode 以外は null が返される

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

        /// <summary>
        ///     <see cref="WAV管理"/> で使用される、サウンドのサンプルストリームインスタンスをキャッシュ管理する。
        /// </summary>
        public static キャッシュデータレンタル<CSCore.ISampleSource> WAVキャッシュレンタル { get; protected set; }

        public static bool ウィンドウがアクティブである { get; set; } = false;    // DirectInput 用。

        public static bool ウィンドウがアクティブではない
        {
            get => !( App.ウィンドウがアクティブである );
            set => App.ウィンドウがアクティブである = !( value );
        }

        /// <summary>
        ///		true なら全画面モード、false ならウィンドウモード。
        /// </summary>
        /// <remarks>
        ///		ウィンドウの表示モード（全画面 or ウィンドウ）を示す。
        ///		値を set することで、モードを変更することもできる。
        ///		正確には、「全画面(fullscreen)」ではなく「最大化(maximize)」。
        /// </remarks>
        public new static bool 全画面モード
        {
            get => ( (ApplicationForm) Program.App ).全画面モード;
            set => ( (ApplicationForm) Program.App ).全画面モード = value;
        }


        public static void システム設定を初期化する()
        {
            var vpath = システム設定.システム設定ファイルパス;
            try
            {
                File.Delete( vpath.変数なしパス );  // ファイルがない場合には例外は出ない
            }
            catch( Exception e )
            {
                Log.ERROR( $"システム設定ファイルの削除に失敗しました。[{vpath.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }

            App.システム設定 = システム設定.復元する(); // ファイルがない場合、新規に作られる
        }

        public static void 曲データベースを初期化する()
        {
            App.曲ツリー.非活性化する();

            var vpath = データベース.曲.SongDB.曲DBファイルパス;
            try
            {
                File.Delete( vpath.変数なしパス );  // ファイルがない場合には例外は出ない
            }
            catch( Exception e )
            {
                Log.ERROR( $"曲データベースファイルの削除に失敗しました。[{vpath.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }
        }

        public static void ユーザデータベースを初期化する()
        {
            App.ユーザ管理.Dispose();

            var vpath = データベース.ユーザ.UserDB.ユーザDBファイルパス;
            try
            {
                File.Delete( vpath.変数なしパス );  // ファイルがない場合には例外は出ない
            }
            catch( Exception e )
            {
                Log.ERROR( $"ユーザデータベースファイルの削除に失敗しました。[{vpath.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }

            App.ユーザ管理 = new ユーザ管理();    // 再生成。
            App.ユーザ管理.ユーザリスト.SelectItem( ( user ) => ( user.ユーザID == "AutoPlayer" ) );  // ひとまずAutoPlayerを選択。
        }


        /// <summary>
        ///     コンストラクタ；アプリの初期化
        /// </summary>
        public App( bool ビュアーモードである )
            : base( 設計画面サイズ: new SizeF( 1920f, 1080f ), 物理画面サイズ: new SizeF( 1280f, 720f ), 深度ステンシルを使う: false )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
#if DEBUG
                SharpDX.Configuration.EnableReleaseOnFinalizer = true;          // ファイナライザの実行中、未解放のCOMを見つけたら解放を試みる。
                SharpDX.Configuration.EnableTrackingReleaseOnFinalizer = true;  // その際には Trace にメッセージを出力する。
#endif
                App.ビュアーモードである = ビュアーモードである;

                var exePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
                VariablePath.フォルダ変数を追加または更新する( "Exe", $@"{exePath}\" );
                VariablePath.フォルダ変数を追加または更新する( "System", Path.Combine( exePath, @"System\" ) );
                VariablePath.フォルダ変数を追加または更新する( "AppData", Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMania2\" ) );

                if( !( Directory.Exists( VariablePath.フォルダ変数の内容を返す( "AppData" ) ) ) )
                    Directory.CreateDirectory( VariablePath.フォルダ変数の内容を返す( "AppData" ) );  // なければ作成。

                App.乱数 = new Random( DateTime.Now.Millisecond );

                App.システム設定 = システム設定.復元する();

                #region " ウィンドウ初期化（システム設定復元後）"
                //----------------
                this.Text = "DTXMania2 r" + App.リリース番号.ToString( "000" ) + ( App.ビュアーモードである ? " [Viewer Mode]" : "" );

                if( App.ビュアーモードである )
                {
                    // ビュアーモードなら、前回の位置とサイズを復元する。
                    this.StartPosition = FormStartPosition.Manual;
                    this.Location = App.システム設定.ウィンドウ表示位置Viewerモード用;
                    this.ClientSize = App.システム設定.ウィンドウサイズViewerモード用;
                }
                //----------------
                #endregion

                App.入力管理 = new 入力管理( this.Handle ) {
                    // 外部アクション接続
                    キーバインディングを取得する = () => App.システム設定.キーバインディング,
                    キーバインディングを保存する = () => App.システム設定.保存する(),
                };
                App.入力管理.初期化する();

                App.ステージ管理 = new ステージ管理();

                App.サウンドデバイス = new SoundDevice( CSCore.CoreAudioAPI.AudioClientShareMode.Shared );
                App.サウンドデバイス.音量 = 0.5f; // マスタ音量（小:0～1:大）... 0.5を超えるとだいたいWASAPI共有モードのリミッターに抑制されるようになる

                App.サウンドタイマ = new SoundTimer( App.サウンドデバイス );

                App.ドラムサウンド = new ドラムサウンド();

                App.システムサウンド = new システムサウンド();

                App.ユーザ管理 = new ユーザ管理();
                App.ユーザ管理.ユーザリスト.SelectItem( ( user ) => ( user.ユーザID == "AutoPlayer" ) );  // ひとまずAutoPlayerを選択。

                App.曲ツリー = new 曲ツリー();

                App.WAVキャッシュレンタル = new キャッシュデータレンタル<CSCore.ISampleSource>() {
                    // 外部アクション接続
                    ファイルからデータを生成する = ( path ) => SampleSourceFactory.Create( App.サウンドデバイス, path, App.ユーザ管理.ログオン中のユーザ.再生速度 ),
                };

                // 以下は選曲されるまで null
                App.ビュアー用曲ノード = null;
                App.演奏スコア = null;
                App.WAV管理 = null;
                App.AVI管理 = null;

                // WCFサービス用
                App.サービスメッセージキュー = new ServiceMessageQueue();

                // 活性化
                this._グローバルリソースを活性化する();

                // ビュアーモードでは常にウィンドウモードで起動。
                base.全画面モード = (App.ビュアーモードである)  ? false : App.ユーザ管理.ログオン中のユーザ.全画面モードである;

                // 最初のステージへ遷移する。
                App.ステージ管理.ステージを遷移する( App.ステージ管理.最初のステージ名 );
            }
        }

        /// <summary>
        ///     アプリの終了処理を行う。
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( disposing && !( this._Dispose済み ) )
                {
                    this._Dispose済み = true;

                    this._グローバルリソースを非活性化する();

                    App.ユーザ管理?.Dispose();
                    App.ユーザ管理 = null;

                    App.システムサウンド?.Dispose();
                    App.システムサウンド = null;

                    App.ドラムサウンド?.Dispose();
                    App.ドラムサウンド = null;

                    App.AVI管理?.Dispose();
                    App.AVI管理 = null;

                    App.WAV管理?.Dispose();   // サウンドデバイスより先に開放すること
                    App.WAV管理 = null;

                    App.WAVキャッシュレンタル?.Dispose();
                    App.WAVキャッシュレンタル = null;

                    App.サウンドタイマ?.Dispose();
                    App.サウンドタイマ = null;

                    App.サウンドデバイス?.Dispose();
                    App.サウンドデバイス = null;

                    App.演奏スコア = null;

                    App.曲ツリー.Dispose();
                    App.曲ツリー = null;

                    App.ステージ管理.Dispose();
                    App.ステージ管理 = null;

                    App.入力管理.Dispose();
                    App.入力管理 = null;

                    App.システム設定.保存する();
                    App.システム設定 = null;
                }

                base.Dispose( disposing );
            }
        }

        /// <summary>
        ///     進行処理。
        ///     描画以外の反復処理が必要なら、ここで行う。
        /// </summary>
        public override void 進行する()
        {
            App.ステージ管理.現在のステージ.高速進行する();
        }

        /// <summary>
        ///     描画処理。
        ///     Direct2D, 3D などを用いた描画を行う。
        ///     スワップチェーンの呼び出しは行わない（呼び出しもとが行う）。
        /// </summary>
        public override void 描画する()
        {
            var gd = グラフィックデバイス.Instance;

            #region " D2D, D3Dレンダリングの前処理を行う。"
            //----------------
            gd.D2DDeviceContext.Transform = グラフィックデバイス.Instance.拡大行列DPXtoPX;
            gd.D2DDeviceContext.PrimitiveBlend = SharpDX.Direct2D1.PrimitiveBlend.SourceOver;

            // 既定のD3Dレンダーターゲットビューを黒でクリアする。
            gd.D3DDevice.ImmediateContext.ClearRenderTargetView( グラフィックデバイス.Instance.D3DRenderTargetView, Color4.Black );

            // 深度バッファを 1.0f でクリアする。
            gd.D3DDevice.ImmediateContext.ClearDepthStencilView(
                グラフィックデバイス.Instance.D3DDepthStencilView,
                SharpDX.Direct3D11.DepthStencilClearFlags.Depth,
                depth: 1.0f,
                stencil: 0 );
            //----------------
            #endregion

            // Windows Animation を進行。
            gd.Animation.進行する();    // 必ずメイン(UI)スレッドから呼び出すこと。

            // 現在のステージを進行＆描画。
            App.ステージ管理.現在のステージ.進行描画する( gd.D2DDeviceContext );

            // ステージの進行描画の結果（フェーズの状態など）を受けての後処理。
            switch( App.ステージ管理.現在のステージ )
            {
                case ステージ.起動.起動ステージ stage:
                    #region " 確定 → 通常時はタイトルステージ、ビュアーモード時は演奏ステージ_ビュアーモードへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.起動.起動ステージ.フェーズ.確定 )
                    {
                        if( App.ビュアーモードである )
                        {
                            // (A) ビュアーモードなら 演奏ステージ_ビュアーモード へ
                            App.ステージ管理.ステージを遷移する( nameof( ステージ.演奏.演奏ステージ_ビュアーモード ) );
                        }
                        else
                        {
                            // (B) 通常時はタイトルステージへ
                            App.ステージ管理.ステージを遷移する( nameof( ステージ.タイトル.タイトルステージ ) );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.タイトル.タイトルステージ stage:
                    #region " キャンセル → 終了ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.タイトル.タイトルステージ.フェーズ.キャンセル )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.終了.終了ステージ ) );
                    }
                    //----------------
                    #endregion
                    #region " 確定 → 認証ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.タイトル.タイトルステージ.フェーズ.確定 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.認証.認証ステージ ) );
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.認証.認証ステージ stage:
                    #region " キャンセル → 終了ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.認証.認証ステージ.フェーズ.キャンセル )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.終了.終了ステージ ) );
                    }
                    //----------------
                    #endregion
                    #region " 確定 → 選曲ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.認証.認証ステージ.フェーズ.確定 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.選曲.選曲ステージ stage:
                    #region " キャンセル → タイトルステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.キャンセル )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.タイトル.タイトルステージ ) );
                    }
                    //----------------
                    #endregion
                    #region " 確定_選曲 → 曲読み込みステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.確定_選曲 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.曲読み込み.曲読み込みステージ ) );
                    }
                    //----------------
                    #endregion
                    #region " 確定_設定 → 設定ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.確定_設定 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.オプション設定.オプション設定ステージ ) );
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.オプション設定.オプション設定ステージ stage:
                    #region " キャンセル, 確定 → 選曲ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.オプション設定.オプション設定ステージ.フェーズ.キャンセル ||
                        stage.現在のフェーズ == ステージ.オプション設定.オプション設定ステージ.フェーズ.確定 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                    }
                    //----------------
                    #endregion
                    #region " 再起動 "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.オプション設定.オプション設定ステージ.フェーズ.再起動 )
                    {
                        Application.Restart();
                        return;
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.曲読み込み.曲読み込みステージ stage:
                    #region " 確定 → 演奏ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.曲読み込み.曲読み込みステージ.フェーズ.完了 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.演奏.演奏ステージ ) );

                        // 曲読み込みステージ画面をキャプチャする（演奏ステージのクロスフェードで使う）
                        var 演奏ステージ = App.ステージ管理.ステージリスト[ nameof( ステージ.演奏.演奏ステージ ) ] as ステージ.演奏.演奏ステージ;
                        演奏ステージ.キャプチャ画面 = 画面キャプチャ.取得する();
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.演奏.演奏ステージ stage:
                    #region " キャンセル → 選曲ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.演奏.演奏ステージ.フェーズ.キャンセル完了 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                    }
                    //----------------
                    #endregion
                    #region " クリア → 結果ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.演奏.演奏ステージ.フェーズ.クリア )
                    {
                        if( App.ビュアーモードである )
                        {
                            // ビュアーモードならクリアフェーズを維持。（サービスメッセージ待ち。）
                        }
                        else
                        {
                            App.ステージ管理.ステージを遷移する( nameof( ステージ.結果.結果ステージ ) );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.演奏.演奏ステージ_ビュアーモード stage:
                    // このステージからどこかへ遷移することはない。
                    break;

                case ステージ.結果.結果ステージ stage:
                    #region " 確定 → 選曲ステージへ "
                    //----------------
                    if( stage.現在のフェーズ == ステージ.結果.結果ステージ.フェーズ.確定 )
                    {
                        App.ステージ管理.ステージを遷移する( nameof( ステージ.選曲.選曲ステージ ) );
                    }
                    //----------------
                    #endregion
                    break;

                case ステージ.終了.終了ステージ stage:
                    #region " 確定 → アプリを終了する。"
                    //----------------
                    if( stage.現在のフェーズ == ステージ.終了.終了ステージ.フェーズ.確定 )
                    {
                        App.ステージ管理.ステージを遷移する( null );
                        this._アプリを終了する();
                    }
                    //----------------
                    #endregion
                    break;
            }
        }


        protected override void OnKeyDown( KeyEventArgs e )
        {
            // F11 キーで、全画面／ウィンドウモードを切り替える。
            if( e.KeyCode == Keys.F11 )
            {
                App.全画面モード = !( App.全画面モード );
                App.ユーザ管理.ログオン中のユーザ.全画面モードである = App.全画面モード;
            }

            base.OnKeyDown( e );
        }

        protected override void OnActivated( EventArgs e )
		{
			App.ウィンドウがアクティブである = true;
			Log.Info( "ウィンドウがアクティブ化されました。" );

			base.OnActivated( e );
		}

        protected override void OnDeactivate( EventArgs e )
		{
			App.ウィンドウがアクティブではない = true;
			Log.Info( "ウィンドウが非アクティブ化されました。" );

			base.OnDeactivate( e );
		}

        protected override void OnResizeEnd( EventArgs e )
        {
            if( App.ビュアーモードである )
            {
                // ビュアーモードなら、新しい位置とサイズを記憶しておく。
                App.システム設定.ウィンドウ表示位置Viewerモード用 = this.Location;
                App.システム設定.ウィンドウサイズViewerモード用 = this.ClientSize;
            }

            base.OnResizeEnd( e );
        }

        protected override void OnInput( in Message msg )
        {
            App.入力管理.WM_INPUTを処理する( msg );

            base.OnInput( msg );
        }

        private bool _Dispose済み = false;


        /// <summary>
        ///		グローバルリソース（Appクラスのstaticメンバ）のうち、
        ///		グラフィックリソースを持つものについて、活性化がまだなら活性化する。
        /// </summary>
        private void _グローバルリソースを活性化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 現状、以下の２つ。
                App.ステージ管理.活性化する();
                App.曲ツリー.活性化する();
            }
        }

        /// <summary>
        ///		グローバルリソース（Appクラスのstaticメンバ）のうち、
        ///		グラフィックリソースを持つものについて、活性化中なら非活性化する。
        /// </summary>
        private void _グローバルリソースを非活性化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 現状、以下の２つ。
                App.ステージ管理.非活性化する();
                App.曲ツリー.非活性化する();
            }
        }

        /// <summary>
        ///	    ウィンドウを閉じ、アプリを終了する。
        /// </summary>
        private void _アプリを終了する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // ユーザ設定を保存する。念のため。
                App.ユーザ管理.ログオン中のユーザ?.保存する();

                this.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using SharpDX;
using FDK;
using DTXMania.WCF;

namespace DTXMania
{
    [ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]   // WCFサービスインターフェースをシングルスレッドで呼び出す。
    partial class App : AppForm, IDTXManiaService
    {

        // statc 


        public static int リリース番号
            => int.TryParse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ), out int release ) ? release : throw new Exception( "アセンブリのプロダクトバージョンに記載ミスがあります。" );

        public static T 属性<T>() where T : Attribute
            => (T) Attribute.GetCustomAttribute( Assembly.GetExecutingAssembly(), typeof( T ) );

        public static bool ビュアーモードである { get; protected set; }



        // 生成と終了


        public App()
            : base( new App進行描画() )
        {
            InitializeComponent();

            this.Text = "DTXMania2 release" + App.リリース番号.ToString( "000" ) + ( App.ビュアーモードである ? " [Viewer Mode]" : "" );

            App進行描画.システム設定 = システム設定.読み込む();

            if( App.ビュアーモードである )
            {
                // 前回の位置とサイズを復元する。
                this.StartPosition = FormStartPosition.Manual;
                this.Location = App進行描画.システム設定.ウィンドウ表示位置Viewerモード用;
                this.ClientSize = App進行描画.システム設定.ウィンドウサイズViewerモード用;
            }
            else
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new System.Drawing.Point( 20, 20 );
                this.ClientSize = new Size( 1024, 576 );
            }

            this.キーボード = new キーボードデバイス();
        }

        private new App進行描画 進行描画 => (App進行描画) base.進行描画;

        protected override void OnKeyDown( KeyEventArgs e )
        {
            // F11 キーで、全画面／ウィンドウモードを切り替える。
            if( e.KeyCode == Keys.F11 )
            {
                this.画面モード = ( this.画面モード == 画面モード.ウィンドウ ) ? 画面モード.全画面 : 画面モード.ウィンドウ;
                //App.ユーザ管理.ログオン中のユーザ.全画面モードである = App.全画面モード;
            }

            base.OnKeyDown( e );
        }



        // IDTXManiaService


        /// <summary>
        ///		曲を読み込み、演奏を開始する。
        ///		ビュアーモードのときのみ有効。
        /// </summary>
        /// <param name="path">曲ファイルパス</param>
        /// <param name="startPart">演奏開始小節番号(0～)</param>
        /// <param name="drumsSound">ドラムチップ音を発声させるなら true。</param>
        public void ViewerPlay( string path, int startPart = 0, bool drumsSound = true )
            => this.進行描画.ViewerPlay( path, startPart, drumsSound );

        /// <summary>
        ///		現在の演奏を停止する。
        ///		ビュアーモードのときのみ有効。
        /// </summary>
        public void ViewerStop()
            => this.進行描画.ViewerStop();

        /// <summary>
        ///		サウンドデバイスの発声遅延[ms]を返す。
        /// </summary>
        /// <returns>遅延量[ms]</returns>
        public float GetSoundDelay()
            => this.進行描画.GetSoundDelay();



        // WCF サービス


        public static readonly string serviceUri = "net.pipe://localhost/DTXMania";
        public static readonly string endPointName = "Viewer";
        public static readonly string endPointUri = $"{serviceUri}/{endPointName}";

        private ServiceHost _wcfServiceHost;

        public static ServiceMessageQueue サービスメッセージキュー { get; protected set; }

        /// <summary>
        ///     WCFサービスの存在チェックと起動。
        /// </summary>
        /// <param name="options">コマンドラインオプション。</param>
        /// <returns>true ならアプリを起動可能。false なら起動せず終了する。</returns>
        public bool WCFサービスをチェックする( CommandLineOptions options )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.ビュアーモードである = ( options.再生開始 || options.再生停止 );
                App.サービスメッセージキュー = new ServiceMessageQueue();   // WCFサービス用

                // WCFサービスホストを起動する。
                this._wcfServiceHost = null;
                try
                {
                    this._WCFサービスホストを起動する( out this._wcfServiceHost );

                    Log.Info( $"WCF サービスの受付を開始しました。[{endPointUri}]" );
                }
                catch( AddressAlreadyInUseException )   // 既に起動されている。
                {
                    if( ビュアーモードである )
                    {
                        // ビュアーモードなら OK。既に別のWCFサービスが立ち上がっているので、そのサービスでオプションを処理して、終了する。
                        this._WCFサービスを取得する( out var factory, out var service, out var serviceChannel );
                        this._WCFサービスでオプションを処理する( service, options );
                        this._WCFサービスを解放する( factory, service, serviceChannel );
                        return false;
                    }
                    else
                    {
                        // 通常モードなら二重起動で NG。
                        MessageBox.Show( "DTXMania はすでに起動しています。多重起動はできません。", "DTXMania Runtime Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                        return false;
                    }
                }

                // ビュアーモードなら、オプションを自分で処理する。
                if( ビュアーモードである )
                    _WCFサービスでオプションを処理する( this, options );

                return true;
            }
        }

        private void _WCFサービスホストを起動する( out ServiceHost serviceHost )
        {
            // アプリのWCFサービスホストを生成する。
            serviceHost = new ServiceHost( this, new Uri( serviceUri ) );

            // 名前付きパイプにバインドしたエンドポイントをサービスホストへ追加する。
            serviceHost.AddServiceEndpoint(
                typeof( WCF.IDTXManiaService ),                             // 公開するインターフェース
                new NetNamedPipeBinding( NetNamedPipeSecurityMode.None ),   // 名前付きパイプ
                endPointName );                                             // 公開するエンドポイント

            // WCFサービスの受付を開始する。
            serviceHost.Open();
        }

        private void _WCFサービスホストを終了する( ServiceHost serviceHost )
        {
            serviceHost.Close( new TimeSpan( 0, 0, 2 ) );   // 最大2sec待つ
        }

        private bool _WCFサービスを取得する( out ChannelFactory<IDTXManiaService> factory, out IDTXManiaService service, out IClientChannel serviceChannel )
        {
            const int 最大リトライ回数 = 1;

            for( int retry = 1; retry <= 最大リトライ回数; retry++ )
            {
                try
                {
                    var binding = new NetNamedPipeBinding( NetNamedPipeSecurityMode.None );
                    factory = new ChannelFactory<IDTXManiaService>( binding );
                    service = factory.CreateChannel( new EndpointAddress( endPointUri ) );
                    serviceChannel = service as IClientChannel; // サービスとチャンネルは同じインスタンス。
                    serviceChannel.Open();

                    return true;    // 取得成功。
                }
                catch
                {
                    // 取得失敗。少し待ってからリトライする。
                    if( 最大リトライ回数 != retry )
                        System.Threading.Thread.Sleep( 500 );
                    continue;
                }
            }

            serviceChannel = null;
            service = null;
            factory = null;
            return false;   // 取得失敗。
        }

        private void _WCFサービスを解放する( ChannelFactory<IDTXManiaService> factory, IDTXManiaService service, IClientChannel serviceChannel )
        {
            serviceChannel?.Close();
            factory?.Close();
        }

        private void _WCFサービスでオプションを処理する( IDTXManiaService service, CommandLineOptions options )
        {
            if( options.再生開始 )
            {
                service.ViewerPlay( options.Filename, options.再生開始小節番号, options.ドラム音を発声する );
            }
            else if( options.再生停止 )
            {
                service.ViewerStop();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using FDK;
using DTXMania.API;

namespace DTXMania
{
    static class Program
    {
        public static App App { get; private set; }

        public const int ログファイルの最大保存日数 = 30;
        public static string ログファイル名 = "";

        // WCFサービスのエンドポイントとURI。
        public static readonly string serviceUri = "net.pipe://localhost/DTXMania";
        public static readonly string endPointName = "Viewer";
        public static readonly string endPointUri = $"{serviceUri}/{endPointName}";


        // メインエントリ。
        [MTAThread] // STAThread にすると60秒で ContextSwitchDeadlock 例外が発生するっぽい
        static void Main( string[] args )
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            try
            {
                Trace.AutoFlush = true;

                #region " %USERPROFILE%/AppData/DTXMania2 フォルダがなければ作成する。"
                //----------------
                var AppDataフォルダ名 = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMania2\" );

                // なければ作成。
                if( !( Directory.Exists( AppDataフォルダ名 ) ) )
                    Directory.CreateDirectory( AppDataフォルダ名 );
                //----------------
                #endregion

                #region " ログファイルへのログの複製出力開始。"
                //----------------
                Program.ログファイル名 = Log.ログファイル名を生成する( Path.Combine( AppDataフォルダ名, "Logs" ), "Log.", TimeSpan.FromDays( ログファイルの最大保存日数 ) );

                // ログファイルをTraceリスナとして追加。
                // 以降、Trace（ならびにFDK.Logクラス）による出力は、このリスナ（＝ログファイル）にも出力される。
                Trace.Listeners.Add( new TraceLogListener( new StreamWriter( Program.ログファイル名, false, Encoding.GetEncoding( "utf-8" ) ) ) );
                //----------------
                #endregion

                Log.現在のスレッドに名前をつける( "描画" );
                Log.WriteLine( Application.ProductName + " " + App.リリース番号.ToString( "000" ) );  // アプリ名とバージョン
                Log.システム情報をログ出力する();
                Log.WriteLine( "" );


                var options = new CommandLineOptions();

                #region " コマンドライン引数を解析する。"
                //----------------
                if( !options.解析する( args ) ) // 解析に失敗すればfalse
                {
                    // 利用法を表示して終了。
                    Log.WriteLine( options.Usage );               // ログと
                    using( var console = new FDK.Console() )
                        console.Out?.WriteLine( options.Usage );  // 標準出力の両方へ
                    return;
                }
                //----------------
                #endregion

                bool ビュアーモードである = ( options.再生開始 || options.再生停止 );


                if( ビュアーモードである )
                {
                    // (A) ビュアーモードの場合、DTXManiaのWCFサービスがすでに存在しているか否かで処理分岐。

                    if( _WCFサービスを取得する( 1, out var factory, out var service, out var serviceChannel ) )
                    {
                        // (A-a) 取得できた　→　すでに起動しているアプリへ処理を委託し、自分は終了。
                        _起動済みのアプリケーションに処理を委託する( service, options );
                        _WCFサービスを解放する( factory, service, serviceChannel );
                    }
                    else
                    {
                        // (A-b) 取得失敗　→　自分がビュアーとして起動し、処理を自分へ委託。
                        _アプリケーションをビュアーモードで起動する( options );
                    }
                }
                else
                {
                    // (B) 通常起動する。
                    _アプリケーションを通常起動する();
                }
            }

            // Release 時には、未処理の例外をキャッチしたらダイアログを表示する。
#if !DEBUG
            catch( Exception e )
            {
                using( var dlg = new 未処理例外検出ダイアログ() )
                {
                    Trace.WriteLine( "" );
                    Trace.WriteLine( "====> 未処理の例外が検出されました。" );
                    Trace.WriteLine( "" );
                    Trace.WriteLine( e.ToString() );

                    dlg.ShowDialog();
                }
            }
#else
            finally
            {
                // DEBUG 時には、未処理の例外が発出されても無視。（デバッガでキャッチすることを想定。）
            }
#endif
        }


        static bool _WCFサービスを取得する( int 最大リトライ回数, out ChannelFactory<IDTXManiaService> factory, out IDTXManiaService service, out IClientChannel serviceChannel )
        {
            for( int retry = 1; retry <= 最大リトライ回数; retry++ )
            {
                try
                {
                    factory = new ChannelFactory<IDTXManiaService>( new NetNamedPipeBinding( NetNamedPipeSecurityMode.None ) );
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

        static void _WCFサービスを解放する( ChannelFactory<IDTXManiaService> factory, IDTXManiaService service, IClientChannel serviceChannel )
        {
            serviceChannel?.Close();
            factory?.Close();
        }

        static bool _WCFサービスホストを起動する( out ServiceHost serviceHost )
        {
            // アプリのWCFサービスホストを生成する。
            serviceHost = new ServiceHost( Program.App, new Uri( serviceUri ) );

            // 名前付きパイプにバインドしたエンドポイントをサービスホストへ追加する。
            serviceHost.AddServiceEndpoint(
                typeof( IDTXManiaService ),                                 // 公開するインターフェース
                new NetNamedPipeBinding( NetNamedPipeSecurityMode.None ),   // 名前付きパイプ
                endPointName );                                             // 公開するエンドポイント

            // WCFサービスの受付を開始する。
            try
            {
                serviceHost.Open();
                Log.Info( $"WCF サービスの受付を開始しました。[{endPointUri}]" );
            }
            catch( AddressAlreadyInUseException )
            {
                // エンドポイントが使用中なら、アプリの二重起動とみなして終了。
                MessageBox.Show( "DTXMania はすでに起動しています。多重起動はできません。", "DTXMania Runtime Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                return false;
            }

            return true;
        }

        static void _WCFサービスホストを終了する( ServiceHost serviceHost )
        {
            serviceHost.Close( new TimeSpan( 0, 0, 2 ) );   // 最大2sec待つ

            Log.Info( $"WCF サービス {endPointUri} の受付を終了しました。" );
        }

        static void _アプリケーションを通常起動する()
        {
            using( Program.App = new App( ビュアーモードである: false ) )
            {
                var WCFサービスホスト = (ServiceHost) null;

                if( _WCFサービスホストを起動する( out WCFサービスホスト ) )    // 起動に成功した
                {
                    try
                    {
                        // アプリを実行する。
                        Program.App.Run();
                    }
                    finally
                    {
                        _WCFサービスホストを終了する( WCFサービスホスト );
                    }
                }
            }
            Log.Header( "アプリケーションを終了しました。" );

            Log.WriteLine( "" );
            Log.WriteLine( "遊んでくれてありがとう！" );
        }

        static void _アプリケーションをビュアーモードで起動する( CommandLineOptions options )
        {
            using( Program.App = new App( ビュアーモードである: true ) )
            {
                var WCFサービスホスト = (ServiceHost) null;

                if( _WCFサービスホストを起動する( out WCFサービスホスト ) )    // 起動に成功した
                {
                    try
                    {
                        // 自分へ処理委託。
                        _起動済みのアプリケーションに処理を委託する( Program.App, options );

                        // アプリを実行する。
                        Program.App.Run();
                    }
                    finally
                    {
                        _WCFサービスホストを終了する( WCFサービスホスト );
                    }
                }
            }
            Log.Header( "アプリケーションを終了しました。" );
        }

        static void _起動済みのアプリケーションに処理を委託する( IDTXManiaService service, CommandLineOptions options )
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

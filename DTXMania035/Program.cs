using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using FDK32;
using DTXMania.API;

namespace DTXMania
{
    static class Program
    {
        public static App App { get; private set; }

        public const int ログファイルの最大保存日数 = 30;
        public static string ログファイル名 = "";

        // メインエントリ。
        [STAThread]
        static void Main( string[] args )
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            #region " %USERPROFILE%/AppData/DTXMania2 フォルダがなければ作成する。"
            //----------------
            var AppDataフォルダ名 = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMania2\" );

            if( !( Directory.Exists( AppDataフォルダ名 ) ) )
                Directory.CreateDirectory( AppDataフォルダ名 );
            //----------------
            #endregion

            #region " ログファイルへのログの複製出力開始。"
            //----------------
            Trace.AutoFlush = true;

            Program.ログファイル名 = Log.ログファイル名を生成する( Path.Combine( AppDataフォルダ名, "Logs" ), "Log.", TimeSpan.FromDays( ログファイルの最大保存日数 ) );

            // ログファイルをTraceリスナとして追加。
            // 以降、Trace（ならびにFDK.Logクラス）による出力は、このリスナ（＝ログファイル）にも出力される。
            Trace.Listeners.Add( new TraceLogListener( new StreamWriter( Program.ログファイル名, false, Encoding.GetEncoding( "utf-8" ) ) ) );

            // 最初の出力。
            Log.現在のスレッドに名前をつける( "UI" );
            Log.WriteLine( Application.ProductName + " " + App.リリース番号.ToString( "000" ) );  // アプリ名とバージョン
            Log.システム情報をログ出力する();
            Log.WriteLine( "" );
            //----------------
            #endregion

            try
            {
                #region " コマンドライン引数を解析する。"
                //----------------
                var options = new CommandLineOptions();

                if( !options.解析する( args ) ) // 解析に失敗すればfalse
                {
                    // 利用法を表示して終了。
                    Log.WriteLine( options.Usage );               // ログと
                    using( var console = new FDK32.Console() )
                        console.Out?.WriteLine( options.Usage );  // 標準出力の両方へ
                    return;
                }
                //----------------
                #endregion

                #region " アプリ起動。"
                //----------------
                Program.App = new App();

                if( Program.App.WCFサービスをチェックする( options ) )
                {
                    Application.Run( Program.App );
                }
                else
                {
                    // false ならアプリを起動しない。
                }
                //----------------
                #endregion
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

            Log.WriteLine( "" );
            Log.WriteLine( "遊んでくれてありがとう！" );
        }
    }
}

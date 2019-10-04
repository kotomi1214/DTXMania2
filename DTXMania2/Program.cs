using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DTXMania2
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // 初期化

                timeBeginPeriod( 1 );

                #region " AppData/DTXMania2 フォルダがなければ作成する。"
                //----------------
                //var AppDataフォルダ名 = Application.UserAppDataPath;  // %USERPROFILE%/AppData/<会社名>/DTXMania2/
                var AppDataフォルダ名 = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), "DTXMania2" ); // %USERPROFILE%/AppData/DTXMania2/

                if( !( Directory.Exists( AppDataフォルダ名 ) ) )
                    Directory.CreateDirectory( AppDataフォルダ名 );
                //----------------
                #endregion

                #region " ログファイルへのログの複製出力開始。"
                //----------------
                {
                    const int ログファイルの最大保存日数 = 30;
                    Trace.AutoFlush = true;

                    var ログファイル名 = Log.ログファイル名を生成する( Path.Combine( AppDataフォルダ名, "Logs" ), "Log.", TimeSpan.FromDays( ログファイルの最大保存日数 ) );

                    // ログファイルをTraceリスナとして追加。
                    // 以降、Trace（ならびにLogクラス）による出力は、このリスナ（＝ログファイル）にも出力される。
                    Trace.Listeners.Add( new TraceLogListener( new StreamWriter( ログファイル名, false, Encoding.GetEncoding( "utf-8" ) ) ) );

                    Log.現在のスレッドに名前をつける( "Form" );
                }
                //----------------
                #endregion

                #region " タイトル、著作権、システム情報をログ出力する。"
                //----------------
                Log.WriteLine( $"{Application.ProductName} Release {int.Parse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ) )}" );

                var copyrights = (AssemblyCopyrightAttribute[]) Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyCopyrightAttribute ), false );
                Log.WriteLine( $"{copyrights[ 0 ].Copyright}" );
                Log.WriteLine( "" );

                Log.システム情報をログ出力する();
                Log.WriteLine( "" );
                //----------------
                #endregion

                #region " フォルダ変数を設定する。"
                //----------------
                {
                    var exePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) ?? "";

                    Folder.フォルダ変数を追加または更新する( "Exe", exePath );
                    Folder.フォルダ変数を追加または更新する( "System", Path.Combine( exePath, "System" ) );
                    Folder.フォルダ変数を追加または更新する( "AppData", AppDataフォルダ名 );
                    Folder.フォルダ変数を追加または更新する( "UserProfile", Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) );
                }
                //----------------
                #endregion


                // アプリ起動

                Application.SetHighDpiMode( HighDpiMode.SystemAware );
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                Application.Run( new AppForm() );


                // 終了

                timeEndPeriod( 1 );

                Log.WriteLine( "" );
                Log.WriteLine( "遊んでくれてありがとう！" );
            }
#if !DEBUG
            // Release 時には、未処理の例外をキャッチしたらダイアログを表示する。
            catch( Exception e )
            {
                MessageBox.Show(
                    $"未処理の例外が発生しました。\n\n" +
                    $"{e.Message}\n" +
                    $"{e.StackTrace}",
                    "Exception" );
            }
#else
            // Debug 時には、未処理の例外が発出されても無視。（デバッガでキャッチすることを想定。）
            finally
            {
            }
#endif
        }

        #region " Win32 "
        //----------------
        [System.Runtime.InteropServices.DllImport( "winmm.dll" )]
        static extern void timeBeginPeriod( uint x );

        [System.Runtime.InteropServices.DllImport( "winmm.dll" )]
        static extern void timeEndPeriod( uint x );
        //----------------
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FDK32
{
    public class Log
    {
        private static readonly string tagINFO = "[INFORMATION]";
        private static readonly string tagWARNING = "[  WARNING  ]";
        private static readonly string tagERROR = "[   ERROR   ]";
        private static readonly string tagINTERVAL = "[ INTERVAL  ]";

        /// <summary>
        ///		これを設定しておくと、スレッドID の横に (名前) と出力されるようになる。
        /// </summary>
        public static void 現在のスレッドに名前をつける( string 名前 )
        {
            lock( Log._スレッド間同期 )
            {
                // (1) Log用に名前を設定
                var ID = Log.GetCurrentThreadId();
                Log._IDto名前[ ID ] = 名前;

                // (2) デバッグ用に名前を設定
                System.Threading.Thread.CurrentThread.Name = 名前;
            }
        }

        public static void Info( string 出力 )
        {
            lock( Log._スレッド間同期 )
            {
                Log._一定時間が経過していたら区切り線を表示する();

                Trace.WriteLine( $"{tagINFO} {Log._日時とスレッドID} {Log._インデックスを返す( Log._深さ )}{出力}" );
            }
        }

        public static void WARNING( string 出力 )
        {
            lock( Log._スレッド間同期 )
            {
                Log._一定時間が経過していたら区切り線を表示する();

                Trace.WriteLine( $"{tagWARNING} {Log._日時とスレッドID} {出力}" );
            }
        }

        public static void ERROR( string 出力 )
        {
            lock( Log._スレッド間同期 )
            {
                Log._一定時間が経過していたら区切り線を表示する();

                Trace.WriteLine( $"{tagERROR} {Log._日時とスレッドID} {出力}" );
            }
        }

        public static void WriteLine( string 出力 )
        {
            lock( Log._スレッド間同期 )
            {
                Trace.WriteLine( 出力 );
            }
        }

        public static void Header( string ヘッダ出力 )
        {
            lock( Log._スレッド間同期 )
            {
                Log._一定時間が経過していたら区切り線を表示する();

                Log.Info( "" );
                Log.Info( $"======== {ヘッダ出力} ========" );
            }
        }

        public static void システム情報をログ出力する()
        {
            // OS 関連

            string _WinEdition = null;
            string _WinVersion = null;
            string _Win10Version = null;
            string _TotalMemory = null;
            string _FreeMemory = null;
            string _DotnetRelease = null;

            // from レジストリ
            using( var hklmKey = Microsoft.Win32.Registry.LocalMachine )
            {
                using( var subKey = hklmKey.OpenSubKey( @"SOFTWARE\Microsoft\Windows NT\CurrentVersion" ) ) // キーがなかったら null が返される
                {
                    if( null != subKey )
                    {
                        _Win10Version = subKey.GetValue( "ReleaseId" ) as string;

                        switch( _Win10Version )
                        {
                            // https://ja.wikipedia.org/wiki/Microsoft_Windows_10#%E5%8D%8A%E6%9C%9F%E3%83%81%E3%83%A3%E3%83%8D%E3%83%AB%EF%BC%88%E3%82%BF%E3%83%BC%E3%82%B2%E3%83%83%E3%83%88%E6%8C%87%E5%AE%9A%EF%BC%89%EF%BC%8F_Current_Branch(CB)
                            case "1507": _Win10Version += "(Released in July 2015)"; break;
                            case "1511": _Win10Version += "(November Update)"; break;
                            case "1607": _Win10Version += "(Anniversary Update)"; break;
                            case "1703": _Win10Version += "(Creators Update)"; break;
                            case "1709": _Win10Version += "(Fall Creators Update)"; break;
                            case "1803": _Win10Version += "(April 2018 Update)"; break;
                            case "1809": _Win10Version += "(October 2018 Update)"; break;
                        }
                    }
                }

                using( var ndpKey = hklmKey.OpenSubKey( @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" ) ) // キーがなかったら null が返される
                {
                    if( null != ndpKey )
                    {
                        var release = ndpKey.GetValue( "Release" ) as int?;

                        if( null != release )
                        {
                            int r = release.Value;

                            // https://docs.microsoft.com/ja-jp/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
                            _DotnetRelease =
                                ( r < 378389 ) ? "4.5未満" :
                                ( r < 378675 ) ? "4.5" :
                                ( r < 379893 ) ? "4.5.1" :
                                ( r < 393295 ) ? "4.5.2" :
                                ( r < 394254 ) ? "4.6" :
                                ( r < 394802 ) ? "4.6.1" :
                                ( r < 460798 ) ? "4.6.2" :
                                ( r < 461308 ) ? "4.7" :
                                ( r < 461808 ) ? "4.7.1" :
                                "4.7.2以降"
                                + $" ({r})";
                        }
                    }
                }
            }

            // from WMI

            using( var mc = new System.Management.ManagementClass( "Win32_OperatingSystem" ) )
            using( var moc = mc.GetInstances() )
            {
                foreach( var mo in moc )
                {
                    using( mo )
                    {
                        _WinEdition = mo[ "Caption" ] as string;
                        _WinVersion = mo[ "Version" ] as string;
                        _TotalMemory = ( int.Parse( mo[ "TotalVisibleMemorySize" ].ToString() ) / ( 1024.0 * 1024.0 ) ).ToString( "F2" );
                        _FreeMemory = ( int.Parse( mo[ "FreePhysicalMemory" ].ToString() ) / ( 1024.0 * 1024.0 ) ).ToString( "F2" );
                    }
                }
            }

            // 出力

            Log.WriteLine( $"{_WinEdition} " + ( ( null != _Win10Version ) ? $"{_Win10Version}, {_WinVersion}" : $"{_WinVersion}" ) );
            Log.WriteLine( $"RAM: {_TotalMemory:F2} GB 中 {_FreeMemory:F2} GB 利用可能" );
            Log.WriteLine( $".NET Framework: {_DotnetRelease}" );
        }

        public static LogBlock Block( string ブロック名 )
        {
            return new LogBlock( ブロック名 );
        }

        /// <summary>
        ///		連続して呼び出しても、前回の同一識別キーでの表示から一定時間が経たないと表示しないInfoメソッド。
        /// </summary>
        /// <remarks>
        ///		毎秒60回の進行描画の進捗など、連続して呼び出すと膨大な数のログが出力されてしまう場合に使用する。
        /// </remarks>
        /// <param name="識別キー"></param>
        /// <param name="出力"></param>
        public static void 定間隔Info( string 識別キー, string 出力, double 間隔sec = 0.25 )
        {
            lock( Log._スレッド間同期 )
            {
                if( Log._識別キー別最終表示時刻.ContainsKey( 識別キー ) )
                {
                    if( ( DateTime.Now - Log._識別キー別最終表示時刻[ 識別キー ] ).TotalSeconds >= 間隔sec )
                    {
                        Log._識別キー別最終表示時刻[ 識別キー ] = DateTime.Now;
                        Trace.WriteLine( $"{tagINFO} {Log._日時とスレッドID} {出力}" );
                    }
                }
                else
                {
                    Log._識別キー別最終表示時刻.Add( 識別キー, DateTime.Now );
                    Trace.WriteLine( $"{tagINFO} {Log._日時とスレッドID} {出力}" );
                }
            }
        }

        /// <summary>
        ///		指定されたフォルダ内に配置可能な、新しいログファイル名を生成して返す。
        /// </summary>
        /// <param name="ログフォルダパス">ログファイルを配置するフォルダのパス。</param>
        /// <param name="ログファイルの接頭辞">ログファイル名に付与する接頭辞。</param>
        /// <param name="最大保存期間">フォルダ内に保存しておく最大の期間。</param>
        /// <returns>生成されたログファイル名。パス付き。</returns>
        /// <remarks>
        ///		ログファイル名は、現在時刻をベースに名付けられる。
        ///		同時に、フォルダ内に存在するすべてのファイルの更新時刻をチェックし、最大保存期間を超える古いファイルは、自動的に削除する。
        /// </remarks>
        public static string ログファイル名を生成する( VariablePath ログフォルダパス, string ログファイルの接頭辞, TimeSpan 最大保存期間 )
        {
            var 現在の日時 = DateTime.Now;

            if( Directory.Exists( ログフォルダパス.変数なしパス ) )
            {
                // (A) フォルダがある場合 → 最大保存期間を超える古いファイルを削除する。
                var 削除対象ファイルs = Directory.GetFiles( ログフォルダパス.変数なしパス ).Where(
                    ( file ) => ( File.GetLastWriteTime( file ) < ( 現在の日時 - 最大保存期間 ) ) );

                foreach( var path in 削除対象ファイルs )
                    File.Delete( path );
            }
            else
            {
                // (B) フォルダがない場合 → 作成する。
                Directory.CreateDirectory( ログフォルダパス.変数なしパス );
            }

            // 現在の時刻をもとに、新しいログファイル名を生成して返す。
            return Path.Combine( ログフォルダパス.変数なしパス, $"{ログファイルの接頭辞}{現在の日時.ToString( "yyyyMMdd-HHmmssffff" )}.txt" );
        }


        internal static void BeginInfo( string 開始ブロック名 )
        {
            lock( Log._スレッド間同期 )
            {
                Log._一定時間が経過していたら区切り線を表示する();
                Trace.WriteLine( $"{tagINFO} {Log._日時とスレッドID} {Log._インデックスを返す( Log._深さ )}{開始ブロック名} --> 開始" );

                Log._深さ++;
            }
        }

        internal static void EndInfo( string 終了ブロック名 )
        {
            lock( Log._スレッド間同期 )
            {
                Log._深さ = Math.Max( ( Log._深さ - 1 ), 0 );

                Log._一定時間が経過していたら区切り線を表示する();
                Trace.WriteLine( $"{tagINFO} {Log._日時とスレッドID} {Log._インデックスを返す( Log._深さ )}{終了ブロック名} <-- 終了" );
            }
        }


        private const double _最小区切り時間 = 2.0; // 区切り線を入れる最小の間隔[秒]。

        private static string _日時とスレッドID
        {
            get
            {
                var NETスレッドID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                var Win32スレッドID = Log.GetCurrentThreadId();
                var スレッド識別文字列 = ( Log._IDto名前.ContainsKey( Win32スレッドID ) ) ? $"({Log._IDto名前[ Win32スレッドID ]})" : "";
                return $"{DateTime.Now.ToLongTimeString()} [{NETスレッドID:00},0x{Win32スレッドID:x}{スレッド識別文字列}]";
            }
        }

        private static readonly Dictionary<uint, string> _IDto名前 = new Dictionary<uint, string>();

        private static Dictionary<string, DateTime> _識別キー別最終表示時刻 = new Dictionary<string, DateTime>();

        private static TimeSpan _経過時間
        {
            get
            {
                var 現在時刻 = DateTime.Now;
                var 経過時間 = 現在時刻 - Log._最終表示時刻;
                Log._最終表示時刻 = 現在時刻;  // 更新
                return 経過時間;
            }
        }

        private static DateTime _最終表示時刻 = DateTime.Now;

        private static int _深さ = 0;

        private static readonly object _スレッド間同期 = new object();


        private static void _一定時間が経過していたら区切り線を表示する()
        {
            var span = Log._経過時間.TotalSeconds;

            if( Log._最小区切り時間 < span )
            {
                #region " faces "
                //----------------
                var faces = new[] {
                    @" ^^) _旦~~",
                    @"( ..)φ",
                    @"( ｀ー´)ノ",
                    @"(#^^#)",
                    @"('ω')",
                    @"(´・ω・｀)",
                    @"(*´ω｀*)",
                    @"( ・_・)ノΞ●~*",
                    @"∠(　˙-˙ )／",
                    @"(*/▽＼*)",
                    @"(ɔ ˘⌣˘)˘⌣˘ c)",
                    @"((*◕ω◕)ﾉ",
                    @"ㆆ﹏ㆆ",
                    @"(ﾟДﾟ;≡;ﾟдﾟ)",
                    @"(ﾟдﾉ[壁]",
                    @"ʅ(´-ω-`)ʃ",
                    @"(*´﹃｀*)",
                    @"(>ω<)",
                    @"٩(๑❛ᴗ❛๑)۶ ",
                    @"(｡･ω･｡)",
                    @"_(┐「ε:)_",
                    @"(ノ-_-)ノ~┻━┻",
                    @"_(ˇωˇ」∠)_ ",
                };
                int faceNow = ( (int) ( span * 1000.0 ) ) % faces.Length;
                //----------------
                #endregion

                Trace.WriteLine( $"{tagINTERVAL} ......　{faces[ faceNow ]}　......" );
            }
        }

        private static string _インデックスを返す( int 長さ )
        {
            var sb = new StringBuilder();

            for( int i = 0; i < 長さ; i++ )
                sb.Append( "| " );

            return sb.ToString();
        }


        #region " Win32 "
        //-----------------
        [System.Runtime.InteropServices.DllImport( "kernel32.dll" )]
        private static extern uint GetCurrentThreadId();
        //-----------------
        #endregion
    }
}

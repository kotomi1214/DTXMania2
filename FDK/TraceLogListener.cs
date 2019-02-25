using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FDK
{
    /// <summary>
    ///		指定されたストリームに Trace の内容を複製出力するリスナー。
    /// </summary>
    public class TraceLogListener : TraceListener
    {
        public TraceLogListener( StreamWriter stream )
        {
            this._streamWriter = stream;
        }

        public override void Flush()
            => this._streamWriter?.Flush();

        public override void Write( string message )
            => this._streamWriter?.Write( this.SanitizeUsername( message ) );

        public override void WriteLine( string message )
            => this._streamWriter?.WriteLine( this.SanitizeUsername( message ) );

        protected override void Dispose( bool disposing )
        {
            this._streamWriter?.Close();
            this._streamWriter = null;

            base.Dispose( disposing );
        }


        private StreamWriter _streamWriter;

        /// <summary>
        ///		もしユーザー名の情報が出力に存在する場合は、伏字にする。
        /// </summary>
        private string SanitizeUsername( string message )
        {
            string userprofile = System.Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );

            if( message.Contains( userprofile ) )
            {
                char delimiter = System.IO.Path.DirectorySeparatorChar;
                string[] u = userprofile.Split( delimiter );
                int c = u[ u.Length - 1 ].Length;     // ユーザー名の文字数
                u[ u.Length - 1 ] = "*".PadRight( c, '*' );
                string sanitizedusername = string.Join( delimiter.ToString(), u );
                message = message.Replace( userprofile, sanitizedusername );
            }

            return message;
        }
    }
}

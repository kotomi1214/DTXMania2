using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.WCF
{
    /// <summary>
    ///		DTXMania へのサービスメッセージ（<see cref="DTXManiaServiceMessage"/>）を管理するキュー。
    /// </summary>
    /// <remarks>
    ///		<see cref="IDTXManiaService"/> のメソッドが呼び出されると、対応するサービスメッセージが生成され、
    ///		このキューに格納される。
    ///		サーバーでは、定期的にサービスメッセージをこのキューから受け取り、対応する処理を行う。
    /// </remarks>
    class DTXManiaServiceMessageQueue
    {
        public DTXManiaServiceMessageQueue()
        {
            this._メッセージキュー = new ConcurrentQueue<DTXManiaServiceMessage>();
        }

        public void 格納する( DTXManiaServiceMessage msg )
        {
            this._メッセージキュー.Enqueue( msg );
        }

        /// <summary>
        ///		メッセージキューから<see cref="DTXManiaServiceMessage">メッセージ</see>を取り出す。
        ///		メッセージがなかった場合は null 。
        /// </summary>
        public DTXManiaServiceMessage 取得する()
        {
            if( this._メッセージキュー.TryDequeue( out DTXManiaServiceMessage msg ) )
            {
                Log.Info( $"サービスメッセージを取得しました。[{msg.種別}]" );
                return msg;
            }
            else
            {
                return null;    // キューは空。
            }
        }


        private ConcurrentQueue<DTXManiaServiceMessage> _メッセージキュー;
    }
}

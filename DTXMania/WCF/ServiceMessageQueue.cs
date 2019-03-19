using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.WCF
{
    /// <summary>
    ///		DTXMania への制御メッセージ（<see cref="ServiceMessage"/>）を管理するキュー。
    /// </summary>
    class ServiceMessageQueue
    {
        public ServiceMessageQueue()
        {
            this._メッセージキュー = new ConcurrentQueue<ServiceMessage>();
        }

        public void 格納する( ServiceMessage msg )
        {
            this._メッセージキュー.Enqueue( msg );
        }

        /// <summary>
        ///		メッセージキューから<see cref="ServiceMessage">メッセージ</see>を取り出す。
        ///		メッセージがなかった場合は null 。
        /// </summary>
        public ServiceMessage 取得する()
        {
            if( this._メッセージキュー.TryDequeue( out ServiceMessage msg ) )
            {
                Log.Info( $"サービスメッセージを取得しました。[{msg.種別}]" );
                return msg;
            }
            else
            {
                return null;    // キューは空。
            }
        }


        private ConcurrentQueue<ServiceMessage> _メッセージキュー;
    }
}

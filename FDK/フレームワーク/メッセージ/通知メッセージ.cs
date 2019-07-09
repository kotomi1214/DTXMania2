using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FDK
{
    /// <summary>
    ///     メッセージの基本クラス。
    /// </summary>
    public abstract class 通知メッセージ
    {
        /// <summary>
        ///     このメッセージの処理が完了した場合にセットされるイベント。
        /// </summary>
        /// <remarks>
        ///     メッセージに対応する処理を行った者がセットすること。
        /// </remarks>
        public AutoResetEvent 完了通知 = new AutoResetEvent( false );
    }
}

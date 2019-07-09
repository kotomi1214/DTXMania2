using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FDK
{
    /// <summary>
    ///     通知の基本クラス。
    /// </summary>
    public abstract class 通知
    {
        /// <summary>
        ///     この通知に対する処理が完了した場合にセットされるイベント。
        /// </summary>
        public AutoResetEvent 完了通知 = new AutoResetEvent( false );
    }
}

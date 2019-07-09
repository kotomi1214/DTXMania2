using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
    /// <summary>
    ///     <see cref="通知"/> を扱うスレッドセーフなキュー。
    /// </summary>
    public class 通知キュー : ConcurrentQueue<通知>
    {
    }
}

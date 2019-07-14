using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Animation;

namespace FDK
{
    /// <summary>
    ///     Windows Animation API の wrapper。
    /// </summary>
    public class Animation : IDisposable
    {
        public Manager Manager { get; private set; } = null;

        public Timer Timer { get; private set; } = null;

        public TransitionLibrary TrasitionLibrary { get; private set; } = null;


        public Animation()
        {
            this.Manager = new Manager();
            this.Timer = new Timer();
            this.TrasitionLibrary = new TransitionLibrary();
        }

        public void Dispose()
        {
            this.TrasitionLibrary?.Dispose();
            this.Timer?.Dispose();
            this.Manager?.Dispose();
        }

        // 生成スレッドと同じスレッドで呼び出すこと！
        public void 進行する()
        {
            this.Manager.Update( this.Timer.Time );
        }
    }
}
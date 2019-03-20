using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania
{
    abstract class ステージ : IDisposable
    {
        public virtual void Dispose()
        {
        }

        public abstract void 進行する();

        public abstract void 描画する();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania
{
    abstract class ステージ : IDisposable
    {

        // 生成と終了


        public abstract void Dispose();



        // 活性化と非活性化

        public abstract void 活性化する();

        public abstract void 非活性化する();

        public bool 活性化中 { get; protected set; } = false;

        public virtual void スワップチェーンに依存するグラフィックリソースを復元する() { }

        public virtual void スワップチェーンに依存するグラフィックリソースを解放する() { }

        
        
        // 進行と描画


        public abstract void 進行する();

        public abstract void 描画する();
    }
}

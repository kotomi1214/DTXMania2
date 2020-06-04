using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DTXMania2_
{
    interface IStage : IDisposable
    {
        void 進行する();

        void 描画する();
    }
}

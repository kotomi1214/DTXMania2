using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;

namespace FDK32
{
    public class DCEventArgs
    {
        public DeviceContext1 dc { get; protected set; } = null;

        public DCEventArgs( DeviceContext1 dc )
        {
            this.dc = dc;
        }
    }
}

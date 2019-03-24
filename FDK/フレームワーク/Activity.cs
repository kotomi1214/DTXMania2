using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
    public class Activity
    {
        public bool 活性化中 { get; protected set; } = false;


        public virtual void 活性化する()
        {
            if( this.活性化中 )
                return;

            this.活性化中 = true;
        }

        public virtual void 非活性化する()
        {
            if( !this.活性化中 )
                return;

            this.活性化中 = false;
        }
    }
}

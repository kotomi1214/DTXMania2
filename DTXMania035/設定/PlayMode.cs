using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXMania.設定
{
    /// <summary>
    ///     演奏モード。
    ///     数値は DB に保存される値なので変更しないこと。
    /// </summary>
    enum PlayMode : int
    {
        BASIC = 0,
        EXPERT = 1,
    }
}

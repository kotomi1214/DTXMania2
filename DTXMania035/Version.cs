using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DTXMania
{
    /// <summary>
    ///     バージョン情報。外部アセンブラ向け。
    /// </summary>
    public static class Version
    {
        public static int Major => Assembly.GetExecutingAssembly().GetName().Version.Major;

        public static int Minor => Assembly.GetExecutingAssembly().GetName().Version.Minor;

        public static int Revision => Assembly.GetExecutingAssembly().GetName().Version.Revision;

        public static int Build => Assembly.GetExecutingAssembly().GetName().Version.Build;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace DTXMania
{
    partial class Node
    {
        /// <summary>
        ///		難易度それぞれのカラー。
        ///		具体的には難易度ラベルの背景の色。
        /// </summary>
        public static IReadOnlyDictionary<int, Color4> LevelColor { get; protected set; } = new Dictionary<int, Color4>() {
            [ 0 ] = new Color4( 0xfffe9551 ),   // BASIC 相当
            [ 1 ] = new Color4( 0xff00aaeb ),   // ADVANCED 相当
            [ 2 ] = new Color4( 0xff7d5cfe ),   // EXTREME 相当
            [ 3 ] = new Color4( 0xfffe55c6 ),   // MASTER 相当
            [ 4 ] = new Color4( 0xff2b28ff ),   // ULTIMATE 相当
        };
    }
}

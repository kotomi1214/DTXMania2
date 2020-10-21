using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation;

namespace FDK
{
    /// <summary>
    ///     ビデオフレームの定義。
    ///     ビデオフレームとは、<see cref="IVideoSource.Read"/> で返されるオブジェクトのこと。
    /// </summary>
    public class VideoFrame : IDisposable
    {
        /// <summary>
        ///     このフレームの表示時刻。100ナノ秒単位。
        /// </summary>
        public long 表示時刻100ns { get; set; }

        /// <summary>
        ///     <see cref="Bitmap"/> の変換元となるサンプル。
        /// </summary>
        public Sample Sample { get; set; } = null!;

        /// <summary>
        ///     <see cref="Sample"/> からの変換後ビットマップ。
        ///     <see cref="Sample"/> とビデオメモリを共有しているので注意。
        /// </summary>
        public Bitmap Bitmap { get; set; } = null!;


        public virtual void Dispose()
        {
            this.Bitmap?.Dispose();
            this.Sample?.Dispose();
        }
    }
}

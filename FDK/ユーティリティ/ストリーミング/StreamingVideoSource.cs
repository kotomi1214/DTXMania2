using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation;

namespace FDK
{
    /// <summary>
    ///     Read と排他的に Write ができる VideoSource 。
    /// </summary>
    public class StreamingVideoSource : IVideoSource
    {
        public Size2F フレームサイズ { get; protected set; }


        public StreamingVideoSource( Size2F フレームサイズ )
        {
            this.フレームサイズ = フレームサイズ;
            this._FrameQueue = new BlockingQueue<VideoFrame>( 3 );
        }

        public void Dispose()
        {
            this.Cancel();

            this._FrameQueue?.Dispose();
            this._FrameQueue = null;
        }

        public void Start( double 再生開始位置sec )
        {
            // todo: 実装する
            throw new NotImplementedException();
        }

        public void Stop()
        {
            // todo: 実装する
            throw new NotImplementedException();
        }

        public void Pause()
        {
            // todo: 実装する
            throw new NotImplementedException();
        }

        public void Resume()
        {
            // todo: 実装する
            throw new NotImplementedException();
        }

        /// <summary>
        ///     次に読みだされるフレームがあれば、その表示予定時刻[100ns単位]を返す。
        ///     フレームがなければ、ブロックせずにすぐ 負数 を返す。
        /// </summary>
        public long Peek()
        {
            if( this._キャンセル済み )
                return -1;

            this._FrameQueue.Peek( out VideoFrame frame );
            return frame?.表示時刻100ns ?? -1;
        }

        /// <summary>
        ///     フレームを１つ読みだす。
        ///     フレームが取得できるまでブロックする。
        ///     キャンセル済みの場合は、ブロックせずに null を返す。
        ///     取得したフレームは、使用が終わったら、呼び出し元で Dispose すること。
        /// </summary>
        public VideoFrame Read()
        {
            if( this._キャンセル済み )
                return null;

            return this._FrameQueue.Take();
        }

        /// <summary>
        ///     フレームを１つ書き込む。
        ///     フレームキューがいっぱいの場合には、キューが空くまでブロックする。
        /// </summary>
        public void Write( VideoFrame frame )
        {
            if( this._キャンセル済み )
                return;

            this._FrameQueue.Add( frame );
        }

        public void Cancel()
        {
            if( !( this._キャンセル済み ) )
            {
                this._キャンセル済み = true;
                this._FrameQueue.Cancel();
            }
        }

        public unsafe Bitmap サンプルからビットマップを取得する( Sample Sample )
        {
            // 1. Sample → MediaBuffer
            using( var mediaBuffer = Sample.ConvertToContiguousBuffer() )
            {
                // 2. MediaBuffer → DXGIBuffer
                using( var dxgiBuffer = mediaBuffer.QueryInterface<DXGIBuffer>() )
                {
                    // 3. DXGIBuffer → DXGISurface(IntPtr)
                    dxgiBuffer.GetResource( typeof( SharpDX.DXGI.Surface ).GUID, out IntPtr vDxgiSurface );

                    // 4.  DXGISurface(IntPtr) → DXGISurface
                    using( var dxgiSurface = new SharpDX.DXGI.Surface( vDxgiSurface ) )
                    {
                        // 5. DXGISurface → Bitmap
                        return new Bitmap(
                            DXResources.Instance.既定のD2D1DeviceContext,
                            dxgiSurface,
                            new BitmapProperties( new PixelFormat( dxgiSurface.Description.Format, AlphaMode.Ignore ) ) );
                    }
                }
            }
        }


        private BlockingQueue<VideoFrame> _FrameQueue = null;

        private bool _キャンセル済み = false;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSCore;

namespace FDK
{
    /// <summary>
    ///     Read と排他的に Write ができる WaveSource 。
    /// </summary>
    public class StreamingWaveSource :IWaveSource
    {
        public bool CanSeek => true;

        public WaveFormat WaveFormat { get; protected set; }

        public long Position { get; set; }

        public long Length => this._FrameQueue.Length;


        public StreamingWaveSource( WaveFormat waveFormat )
        {
            this.WaveFormat = waveFormat;
            this._FrameQueue = new QueueMemoryStream();
        }

        public void Dispose()
        {
            this.Cancel();

            this._FrameQueue?.Dispose();
            this._FrameQueue = null;
        }
        
        /// <summary>
        ///		連続したバイトデータを読み込む。
        ///		データが不足していれば、その分は 0 で埋めて返す。
        /// </summary>
        /// <param name="buffer">読み込んだデータを格納するための配列。</param>
        /// <param name="offset"><paramref name="buffer"/> に格納を始める位置。</param>
        /// <param name="count">読み込む最大のデータ数。</param>
        /// <returns><paramref name="buffer"/> に読み込んだデータの総数（常に <paramref name="count"/> に一致）。</returns>
        public int Read( byte[] buffer, int offset, int count )
        {
            // 前提条件チェック。音がめちゃくちゃになるとうざいので、このメソッド内では例外を出さないこと。
            if( ( null == this._FrameQueue ) || ( null == buffer ) )
                return 0;

            // ストリームから読み出す。データが不足していてもブロックせずすぐ戻る。
            int readCount = this._FrameQueue.Read( buffer, offset, count, データが足りないならブロックする: false );

            // データが不足しているなら、不足分を 0 で埋める。
            if( readCount < count )
                Array.Clear( buffer, ( offset + readCount ), ( count - readCount ) );

            return count;
        }

        /// <summary>
        ///     連続したバイトデータを書き込む。
        ///     バッファサイズは理論上無限なので、空き不足でブロックすることはない。
        /// </summary>
        public void Write( byte[] buffer, int offset, int count )
        {
            this._FrameQueue.Write( buffer, offset, count );
        }

        public void Cancel()
        {
            if( !( this._キャンセル済み ) )
            {
                this._キャンセル済み = true;
                this._FrameQueue.Cancel();
            }
        }


        private QueueMemoryStream _FrameQueue = null;

        private bool _キャンセル済み = false;
    }
}

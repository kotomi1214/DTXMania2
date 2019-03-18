using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using CSCore.DSP;

namespace FDK
{
    /// <summary>
    ///		指定されたメディアファイルをデコードし、リサンプルして、
    ///		<see cref="CSCore.IWaveSource"/> オブジェクトを生成する。
    /// </summary>
    public class ResampledOnMemoryWaveSource : IWaveSource
    {
        public bool CanSeek => true;    // オンメモリなので常にサポートできる。

        /// <summary>
        /// 	デコード＆リサンプル後のオーディオデータのフォーマット。
        /// </summary>
        public WaveFormat WaveFormat { get; protected set; } = null;

        /// <summary>
        ///		現在の再生位置[byte]。
        /// </summary>
        public long Position
        {
            get
                => this._Position;
            set
                => this._Position = FDKUtilities.位置をブロック境界単位にそろえて返す( value, this.WaveFormat.BlockAlign );
        }

        /// <summary>
        ///		デコード後のオーディオデータのすべての長さ[byte]。
        /// </summary>
        public long Length => this._DecodedWaveData.Length;


        /// <summary>
        ///     コンストラクタ。
        ///     指定された <see cref="IWaveSource"/> をリサンプルする。
        /// </summary>
        public ResampledOnMemoryWaveSource( IWaveSource waveSource, WaveFormat deviceFormat, double 再生速度 = 1.0 )
        {
            // サウンドデバイスには、それに合わせたサンプルレートで報告する。
            this.WaveFormat = new WaveFormat(
                deviceFormat.SampleRate,
                32,
                deviceFormat.Channels,
                AudioEncoding.IeeeFloat );

            // しかしサウンドデータは、指定された再生速度を乗したサンプルレートで生成する。
            var waveFormtForResampling = new WaveFormat(
                (int) ( this.WaveFormat.SampleRate / 再生速度 ),
                this.WaveFormat.BitsPerSample,
                this.WaveFormat.Channels,
                AudioEncoding.IeeeFloat );

            using( var resampler = new DmoResampler( waveSource, waveFormtForResampling ) )
            {
                var サイズbyte = resampler.Length;

                this._DecodedWaveData = new byte[ サイズbyte ];
                resampler.Read( this._DecodedWaveData, 0, (int) サイズbyte );
            }
        }

        /// <summary>
        ///		解放する。
        /// </summary>
        public void Dispose()
        {
            this._DecodedWaveData = null;
        }

        /// <summary>
        ///		連続したデータを読み込み、<see cref="Position"/> を読み込んだ数だけ進める。
        /// </summary>
        /// <param name="buffer">読み込んだデータを格納するための配列。</param>
        /// <param name="offset"><paramref name="buffer"/> に格納を始める位置。</param>
        /// <param name="count">読み込む最大のデータ数。</param>
        /// <returns><paramref name="buffer"/> に読み込んだデータの総数。</returns>
        public int Read( byte[] buffer, int offset, int count )
        {
            // ※ 音がめちゃくちゃになるとうざいので、このメソッド内では例外を出さないこと。
            if( ( null == this._DecodedWaveData ) || ( null == buffer ) )
                return 0;

            long 読み込み可能な最大count = ( this.Length - this._Position );

            if( count > 読み込み可能な最大count )
                count = (int) 読み込み可能な最大count;

            if( 0 < count )
            {
                Buffer.BlockCopy(
                    src: this._DecodedWaveData,
                    srcOffset: (int) this._Position,
                    dst: buffer,
                    dstOffset: offset,
                    count: count );

                this._Position += count;
            }

            return count;
        }


        private long _Position = 0;

        private byte[] _DecodedWaveData = null;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;

namespace FDK
{
    /// <summary>
    ///     <see cref="ISampleSource"/> オブジェクトの生成を行う。
    /// </summary>
    public static class SampleSourceFactory
    {
        /// <summary>
        ///		指定されたファイルの音声をデコードし、<see cref="ISampleSource"/> オブジェクトを返す。
        ///		失敗すれば null 。
        /// </summary>
        public static ISampleSource Create( SoundDevice device, VariablePath ファイルパス, double 再生速度 = 1.0 )
        {
            if( !( File.Exists( ファイルパス.変数なしパス ) ) )
            {
                Log.ERROR( $"ファイルが存在しません。[{ファイルパス.変数付きパス}]" );
                return null;
            }

            var 拡張子 = Path.GetExtension( ファイルパス.変数なしパス ).ToLower();

            if( ".ogg" == 拡張子 )
            {
                #region " OggVorvis "
                //----------------
                try
                {
                    using( var audioStream = new FileStream( ファイルパス.変数なしパス, FileMode.Open, FileAccess.Read ) )
                    {
                        // ファイルを読み込んで IWaveSource を生成。
                        using( var waveSource = new NVorbisOnStreamingSampleSource( audioStream, device.WaveFormat ).ToWaveSource() )
                        {
                            // IWaveSource をリサンプルして ISampleSource を生成。
                            return new ResampledOnMemoryWaveSource( waveSource, device.WaveFormat, 再生速度 ).ToSampleSource();
                        }
                    }
                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }
            if( ".wav" == 拡張子 )
            {
                #region " WAV "
                //----------------
                try
                {
                    // ファイルを読み込んで IWaveSource を生成。
                    using( var waveSource = new WavOnMemoryWaveSource( ファイルパス, device.WaveFormat ) )
                    {
                        // IWaveSource をリサンプルして ISampleSource を生成。
                        return new ResampledOnMemoryWaveSource( waveSource, device.WaveFormat, 再生速度 ).ToSampleSource();
                    }
                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }
            if( ".xa" == 拡張子 )
            {
                #region " XA "
                //----------------
                try
                {
                    // ファイルを読み込んで IWaveSource を生成。
                    using( var waveSource = new XAOnMemoryWaveSource( ファイルパス, device.WaveFormat ) )
                    {
                        // IWaveSource をリサンプルして ISampleSource を生成。
                        return new ResampledOnMemoryWaveSource( waveSource, device.WaveFormat, 再生速度 ).ToSampleSource();
                    }
                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }
            else
            {
                #region " MediaFoundation "
                //----------------
                try
                {
                    // ファイルを読み込んで IWaveSource を生成。
                    using( var waveSource = new MediaFoundationOnMemoryWaveSource( ファイルパス, device.WaveFormat ) )
                    {
                        // IWaveSource をリサンプルして ISampleSource を生成。
                        return new ResampledOnMemoryWaveSource( waveSource, device.WaveFormat, 再生速度 ).ToSampleSource();
                    }
                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }

            Log.ERROR( $"未対応のオーディオファイルです。{ファイルパス.変数付きパス}" );
            return null;
        }
    }
}


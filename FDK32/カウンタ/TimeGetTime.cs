using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace FDK32
{
    /// <summary>
    ///		::timeGetTime のラッパ 兼 PerformanceCounter との相互変換機能。
    /// </summary>
    public class TimeGetTime : IDisposable
    {
        public TimeGetTime()
        {
            timeBeginPeriod( 1 );

            this._カウンタ周波数 = QPCTimer.周波数;
            this._開始時のカウンタ = QPCTimer.生カウント;
            this._開始時のタイマ = timeGetTime();
        }
        public void Dispose()
        {
            timeEndPeriod( 1 );
        }

        /// <summary>
        ///		パフォーマンスカウンタの生カウンタを、対応するタイマ値に変換して返す。
        /// </summary>
        public uint カウンタtoタイマ( long 生カウンタ値 )
        {
            var 経過時間ms = ( 生カウンタ値 - this._開始時のカウンタ ) * 1000 / this._カウンタ周波数;

            return this._開始時のタイマ + (uint) 経過時間ms;
        }

        /// <summary>
        ///		タイマ値を、対応するパフォーマンスカウンタの生カウンタ値に変換して返す。
        /// </summary>
        public long タイマtoカウンタ( uint タイマ値 )
        {
            var 経過カウンタ = ( タイマ値 - this._開始時のタイマ ) * this._カウンタ周波数 / 1000;

            return this._開始時のカウンタ + 経過カウンタ;
        }

        private uint _開始時のタイマ = 0;
        private long _開始時のカウンタ = 0;
        private long _カウンタ周波数 = 0;

        #region " Win32 "
        //----------------
        [StructLayout( LayoutKind.Sequential )]
        protected struct TimeCaps
        {
            public uint wPeriodMin;
            public uint wPeriodMax;
        }
        [DllImport( "winmm.dll" )]
        protected static extern void timeBeginPeriod( uint x );

        [DllImport( "winmm.dll" )]
        protected static extern void timeEndPeriod( uint x );

        [DllImport( "winmm.dll" )]
        protected static extern uint timeGetDevCaps( out TimeCaps timeCaps, uint size );

        [DllImport( "winmm.dll" )]
        protected static extern uint timeGetTime();
        //----------------
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using FDK32;

namespace DTXMania.ステージ
{
    class システム情報 : Activity
    {
        public int 現在のFPS => this._FPS.現在のFPS;

        public int 現在のVPS => this._FPS.現在のVPS;


        public システム情報()
        {
            this.子Activityを追加する( this._FPS = new FPS() );
            this.子Activityを追加する( this._文字列画像 = new 文字列画像() );
        }

        protected override void On活性化()
        {
        }

        protected override void On非活性化()
        {
        }

        public bool FPSをカウントしプロパティを更新する()
            => this._FPS.FPSをカウントしプロパティを更新する();

        public void VPSをカウントする()
            => this._FPS.VPSをカウントする();

        public void 描画する( DeviceContext1 dc, string 追加文字列 = "" )
        {
            double FPSの周期ms = ( 0 < this._FPS.現在のFPS ) ? ( 1000.0 / this._FPS.現在のFPS ) : -1.0;

            this._文字列画像.表示文字列 =
                $"VPS: {this._FPS.現在のVPS.ToString()} / FPS: {this._FPS.現在のFPS.ToString()} (" + FPSの周期ms.ToString( "0.000" ) + "ms)" + Environment.NewLine +
                $"GameMode: {ONOFF[ FDKUtilities.ゲームモードである ]}" + Environment.NewLine +
                追加文字列;

            this._文字列画像.描画する( dc, 0f, 0f );
        }


        private FPS _FPS = null;

        private 文字列画像 _文字列画像 = null;

        private readonly Dictionary<bool, string> ONOFF = new Dictionary<bool, string> { { false, "OFF" }, { true, "ON" } };
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using FDK32;

namespace DTXMania.ステージ.演奏.BASIC
{
    /// <summary>
    ///     ヒットバー ... チップがこの位置に来たら叩け！という線。
    /// </summary>
    class ヒットバー : Activity
    {
        public const float ヒット判定バーの中央Y座標dpx = 847f;


        public ヒットバー()
        {
            this.子Activityを追加する( this._ヒットバー画像 = new テクスチャ( @"$(System)images\演奏\ヒットバー.png" ) );
        }

        protected override void On活性化()
        {
        }

        protected override void On非活性化()
        {
        }

        public void 描画する()
        {
            const float バーの左端Xdpx = 441f;
            const float バーの中央Ydpx = 演奏ステージ.ヒット判定位置Ydpx;
            const float バーの厚さdpx = 8f;

            this._ヒットバー画像.描画する( バーの左端Xdpx, バーの中央Ydpx - バーの厚さdpx / 2f );
        }


        private テクスチャ _ヒットバー画像 = null;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.演奏.BASIC
{
    /// <summary>
    ///     ヒットバー ... チップがこの位置に来たら叩け！という線。
    /// </summary>
    class ヒットバー : IDisposable
    {
        public const float ヒット判定バーの中央Y座標dpx = 847f;



        // 生成と終了


        public ヒットバー()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._ヒットバー画像 = new テクスチャ( @"$(System)images\演奏\ヒットバー.png" );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._ヒットバー画像?.Dispose();
            }
        }



        // 進行と描画


        public void 描画する()
        {
            const float バーの左端Xdpx = 441f;
            const float バーの中央Ydpx = 演奏ステージ.ヒット判定位置Ydpx;
            const float バーの厚さdpx = 8f;

            this._ヒットバー画像.描画する( バーの左端Xdpx, バーの中央Ydpx - バーの厚さdpx / 2f );
        }



        // private


        private テクスチャ _ヒットバー画像 = null;
    }
}

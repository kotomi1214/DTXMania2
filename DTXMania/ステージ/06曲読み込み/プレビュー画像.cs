using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.曲読み込み
{
    class プレビュー画像 : IDisposable
    {

        // 生成と終了


        public プレビュー画像()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }



        // 進行と描画


        public void 描画する( DeviceContext dc )
        {
            var 選択曲 = App進行描画.曲ツリー.フォーカス曲ノード;
            var preimage = 選択曲.ノード画像 ?? Node.既定のノード画像;

            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 変換行列 =
                Matrix.Scaling(
                    this._プレビュー画像表示サイズdpx.X / preimage.サイズ.Width,
                    this._プレビュー画像表示サイズdpx.Y / preimage.サイズ.Height,
                    0f ) *
                Matrix.Translation(
                    グラフィックデバイス.Instance.画面左上dpx.X + this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X / 2f,
                    グラフィックデバイス.Instance.画面左上dpx.Y - this._プレビュー画像表示位置dpx.Y - this._プレビュー画像表示サイズdpx.Y / 2f,
                    0f );

            preimage.描画する( 変換行列 );
        }



        // private


        private readonly Vector3 _プレビュー画像表示位置dpx = new Vector3( 150f, 117f, 0f );

        private readonly Vector3 _プレビュー画像表示サイズdpx = new Vector3( 576f, 576f, 0f );
    }
}

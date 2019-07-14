using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.演奏
{
    class 左サイドクリアパネル : IDisposable
    {
        public 描画可能テクスチャ クリアパネル { get; protected set; } = null;



        // 生成と終了


        public 左サイドクリアパネル()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._背景 = new 画像( @"$(System)images\演奏\左サイドクリアパネル.png" );
                this.クリアパネル = new 描画可能テクスチャ( this._背景.サイズ );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._背景?.Dispose();
                this.クリアパネル?.Dispose();
            }
        }



        // クリア


        public void クリアする()
        {
            this.クリアパネル.テクスチャへ描画する( ( dcp ) => {
                dcp.Clear( new Color4( Color3.Black, 0f ) );
                dcp.DrawBitmap( this._背景.Bitmap, opacity: 1f, interpolationMode: InterpolationMode.Linear );
            } );
        }



        // 進行と描画


        public void 描画する()
        {
            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 変換行列 =
                Matrix.RotationY( MathUtil.DegreesToRadians( -48f ) ) *
                Matrix.Translation( 
                    DXResources.Instance.画面左上dpx.X + 230f,
                    DXResources.Instance.画面左上dpx.Y - 530f,
                    0f );

            this.クリアパネル.描画する( 変換行列 );
        }



        // private


        private 画像 _背景 = null;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;

namespace FDK32
{
    /// <summary>
    ///		D3Dテクスチャとメモリを共有するD2Dビットマップを持つテクスチャ。
    ///		D2Dビットマップに対して描画を行えば、それをD3Dテクスチャとして表示することができる。
    /// </summary>
    public class 描画可能テクスチャ : テクスチャ
    {
        public 描画可能テクスチャ( VariablePath 画像ファイルパス )
            : base( 画像ファイルパス, BindFlags.RenderTarget | BindFlags.ShaderResource )
        {
        }

        public 描画可能テクスチャ( Size2F サイズ )
            : base( サイズ, BindFlags.RenderTarget | BindFlags.ShaderResource )
        {
        }

        protected override void On活性化()
        {
            // テクスチャを作成する。
            base.On活性化();

            // 作成したテクスチャとデータを共有するビットマップターゲットを作成する。
            using( var dxgiSurface = this.Texture.QueryInterfaceOrNull<SharpDX.DXGI.Surface1>() )
            {
                var bmpProp = new BitmapProperties1() {
                    PixelFormat = new PixelFormat( dxgiSurface.Description.Format, AlphaMode.Premultiplied ),
                    BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw,
                };
                this._Bitmap = new Bitmap1( グラフィックデバイス.Instance.D2DDeviceContext, dxgiSurface, bmpProp );
            }
        }

        protected override void On非活性化()
        {
            this._Bitmap?.Dispose();
            this._Bitmap = null;

            // テクスチャを解放する。
            base.On非活性化();
        }

        public void テクスチャへ描画する( Action<SharpDX.Direct2D1.DeviceContext1> 描画アクション )
        {
            var dc = グラフィックデバイス.Instance.D2DDeviceContext;

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                using( var originalTarget = dc.Target )
                {
                    try
                    {
                        dc.Target = this._Bitmap;
                        dc.Transform = Matrix3x2.Identity;  // 等倍描画（dpx to dpx）

                        描画アクション( dc );
                    }
                    finally
                    {
                        dc.Target = originalTarget;
                    }
                }

            } );
        }


        /// <summary>
        ///     テクスチャとメモリを共有するビットマップ。
        /// </summary>
        private Bitmap1 _Bitmap = null;
    }
}

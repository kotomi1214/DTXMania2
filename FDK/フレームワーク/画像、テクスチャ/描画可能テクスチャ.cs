using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;

namespace FDK
{
    /// <summary>
    ///		D3Dテクスチャとメモリを共有するD2Dビットマップを持つテクスチャ。
    ///		D2Dビットマップに対して描画を行えば、それをD3Dテクスチャとして表示することができる。
    /// </summary>
    public class 描画可能テクスチャ : テクスチャ
    {

        // 生成と終了


        /// <summary>
        ///     指定した画像ファイルからテクスチャを作成する。
        /// </summary>
        public 描画可能テクスチャ( VariablePath 画像ファイルパス )
            : base( 画像ファイルパス, BindFlags.RenderTarget | BindFlags.ShaderResource )
        {
            this._Bitmap = this._作成したテクスチャとデータを共有するビットマップターゲットを作成する();
        }

        /// <summary>
        ///     指定したサイズの、空のテクスチャを作成する。
        /// </summary>
        public 描画可能テクスチャ( Size2F サイズ )
            : base( サイズ, BindFlags.RenderTarget | BindFlags.ShaderResource )
        {
            this._Bitmap = this._作成したテクスチャとデータを共有するビットマップターゲットを作成する();
        }

        public override void Dispose()
        {
            this._Bitmap?.Dispose();
            this._Bitmap = null;

            base.Dispose();
        }


        private Bitmap1 _Bitmap = null;

        private Bitmap1 _作成したテクスチャとデータを共有するビットマップターゲットを作成する()
        {
            using( var dxgiSurface = this.Texture.QueryInterfaceOrNull<SharpDX.DXGI.Surface>() )
            {
                var bmpProp = new BitmapProperties1() {
                    PixelFormat = new PixelFormat( dxgiSurface.Description.Format, AlphaMode.Premultiplied ),
                    BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw,
                };

                return new Bitmap1( グラフィックデバイス.Instance.既定のD2D1DeviceContext, dxgiSurface, bmpProp );
            }
        }



        // 描画


        public void テクスチャへ描画する( Action<SharpDX.Direct2D1.DeviceContext> 描画アクション )
        {
            var dc = グラフィックデバイス.Instance.既定のD2D1DeviceContext;

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                dc.Target = this._Bitmap;           // 描画先
                dc.Transform = Matrix3x2.Identity;  // 等倍描画（dpx to dpx）
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

                描画アクション( dc );

                dc.Target = グラフィックデバイス.Instance.既定のD2D1RenderBitmap1;

            } );
        }
    }
}

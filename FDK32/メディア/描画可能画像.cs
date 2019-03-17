using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK32
{
    /// <summary>
    ///		レンダーターゲットとしても描画可能なビットマップを扱うクラス。
    /// </summary>
    public class 描画可能画像 : 画像
    {
        public 描画可能画像( VariablePath 画像ファイルパス )
            : base( 画像ファイルパス )
        {
        }

        public 描画可能画像( Size2F サイズ )
            : base( (VariablePath) null )
        {
            this._サイズ = サイズ;
        }

        public 描画可能画像( float width, float height )
            : base( (VariablePath) null )
        {
            this._サイズ = new Size2F( width, height );
        }

        protected override void On活性化()
        {
            // ビットマップを生成。
            if( this._画像ファイルパス?.変数なしパス.Nullでも空でもない() ?? false )
            {
                // (A) ファイルから生成する。
                this._Bitmapを生成する( new BitmapProperties1() { BitmapOptions = BitmapOptions.Target } );
            }
            else
            {
                // (B) 空のビットマップを生成する。
                this._Bitmap?.Dispose();
                this._Bitmap = new Bitmap1(
                    グラフィックデバイス.Instance.D2DDeviceContext, 
                    new Size2( (int) this._サイズ.Width, (int) this._サイズ.Height ),
                    new BitmapProperties1() {
                        PixelFormat = new PixelFormat( グラフィックデバイス.Instance.D2DDeviceContext.PixelFormat.Format, AlphaMode.Premultiplied ),
                        BitmapOptions = BitmapOptions.Target,
                    } );
            }
        }

        protected override void On非活性化()
        {
            this._Bitmap?.Dispose();
            this._Bitmap = null;
        }

        /// <summary>
        ///		生成済み画像（ビットマップ）に対するユーザアクションによる描画を行う。
        /// </summary>
        /// <remarks>
        ///		活性化状態であれば、進行描画() 中でなくても、任意のタイミングで呼び出して良い。
        ///		ユーザアクション内では BeginDraw(), EndDraw() の呼び出しは（呼び出しもとでやるので）不要。
        /// </remarks>
        /// <param name="gd">グラフィックデバイス。</param>
        /// <param name="描画アクション">Bitmap に対して行いたい操作。</param>
        public void 画像へ描画する( Action<DeviceContext1> 描画アクション )
        {
            var dc = グラフィックデバイス.Instance.D2DDeviceContext;

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                using( var originalTarget = dc.Target )
                {
                    try
                    {
                        dc.Target = this._Bitmap;
                        dc.Transform = Matrix3x2.Identity;  // 等倍（dpx to dpx）

                        描画アクション( dc );
                    }
                    finally
                    {
                        dc.Target = originalTarget;
                    }
                }

            } );
        }


        private Size2F _サイズ;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK
{
    /// <summary>
    ///		レンダーターゲットとしても描画可能なビットマップを扱うクラス。
    /// </summary>
    public class 描画可能画像 : 画像
    {

        // 生成と終了


        public 描画可能画像( VariablePath 画像ファイルパス )
            : base( 画像ファイルパス )
        {
            // 画像ファイルから生成する。

            if( 画像ファイルパス?.変数なしパス.Nullでも空でもない() ?? false )
            {
                this._Bitmapを生成する( 
                    画像ファイルパス, 
                    new BitmapProperties1 {
                        BitmapOptions = BitmapOptions.Target,
                    } );
            }
        }

        public 描画可能画像( Size2F サイズ )
            : base( null )
        {
            this._サイズ = サイズ;

            // 空のビットマップを生成する。

            this.Bitmap?.Dispose();
            this.Bitmap = new Bitmap1(
                グラフィックデバイス.Instance.既定のD2D1DeviceContext,
                new Size2( (int) this._サイズ.Width, (int) this._サイズ.Height ),
                new BitmapProperties1() {
                    PixelFormat = new PixelFormat(
                        グラフィックデバイス.Instance.既定のD2D1DeviceContext.PixelFormat.Format,
                        AlphaMode.Premultiplied ),
                    BitmapOptions = BitmapOptions.Target,
                } );
        }

        public 描画可能画像( float width, float height )
            : this( new Size2F( width, height ) )
        {
        }



        // 描画


        /// <summary>
        ///		生成済み画像（ビットマップ）に対するユーザアクションによる描画を行う。
        /// </summary>
        /// <remarks>
        ///		活性化状態であれば、進行描画() 中でなくても、任意のタイミングで呼び出して良い。
        ///		ユーザアクション内では BeginDraw(), EndDraw() の呼び出しは（呼び出しもとでやるので）不要。
        /// </remarks>
        /// <param name="gd">グラフィックデバイス。</param>
        /// <param name="描画アクション">Bitmap に対して行いたい操作。</param>
        public void 画像へ描画する( Action<DeviceContext> 描画アクション )
        {
            var dc = グラフィックデバイス.Instance.既定のD2D1DeviceContext;

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                dc.AntialiasMode = AntialiasMode.Aliased;
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                dc.TextAntialiasMode = TextAntialiasMode.Grayscale;
                dc.UnitMode = UnitMode.Pixels;
                dc.Target = this.Bitmap;            // 描画先
                dc.Transform = Matrix3x2.Identity;  // 等倍（dpx to dpx）

                描画アクション( dc );

            } );
        }


        private Size2F _サイズ;
    }
}

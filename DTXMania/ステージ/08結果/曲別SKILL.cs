using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.結果
{
    class 曲別SKILL : IDisposable
    {

        // 生成と終了


        public 曲別SKILL()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._曲別SKILLアイコン = new テクスチャ( @"$(System)Images\結果\曲別SKILLアイコン.png" );
                this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大.png", @"$(System)images\パラメータ文字_大.yaml", 文字幅補正dpx: 0f );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._数字画像?.Dispose();
                this._曲別SKILLアイコン?.Dispose();
            }
        }



        // 進行と描画


        public void 進行描画する( DeviceContext dc, float left, float top, double スキル値 )
        {
            // アイコン

            this._曲別SKILLアイコン.描画する( left, top );


            // 数値

            string スキル値文字列 = スキル値.ToString( "0.00" ).PadLeft( 6 );    // 左余白は ' '。例:" 19.00", "199.99"

            // 小数部を描画する
            this._数字画像.描画する( dc, left + 360f, top + 3f + 17f, スキル値文字列.Substring( 4 ), new Size2F( 1.0f, 1.0f ) );

            // 整数部を描画する（'.'含む）
            this._数字画像.描画する( dc, left + 180f, top + 3f, スキル値文字列.Substring( 0, 4 ), new Size2F( 1.0f, 1.2f ) );


            // アンダーライン

            DXResources.Instance.D2DBatchDraw( dc, () => {
                using( var brush = new SolidColorBrush( dc, Color4.White ) )
                    dc.FillRectangle( new RectangleF( left + 33f, top + 113f, 513f, 3f ), brush );
            } );
        }



        // ローカル


        private テクスチャ _曲別SKILLアイコン;

        private 画像フォント _数字画像;
    }
}

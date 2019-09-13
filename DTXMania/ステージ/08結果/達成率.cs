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
    class 達成率 : IDisposable
    {

        // 生成と終了


        public 達成率()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._達成率アイコン = new テクスチャ( @"$(System)Images\結果\達成率アイコン.png" );
                this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大太斜.png", @"$(System)images\パラメータ文字_大太斜.yaml", 文字幅補正dpx: -2f );
                this._MAX = new テクスチャ( @"$(System)Images\結果\MAX.png" );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._MAX?.Dispose();
                this._数字画像?.Dispose();
                this._達成率アイコン?.Dispose();
            }
        }



        // 進行と描画


        public void 進行描画する( DeviceContext dc, float left, float top, double 達成率0to100 )
        {
            // アイコン

            this._達成率アイコン.描画する( left, top );


            // 数値

            if( 達成率0to100 < 100.0 )
            {
                string 難易度文字列 = 達成率0to100.ToString( "0.00" ).PadLeft( 6 ) + '%';    // 左余白は ' '。例:" 19.00%", "100.00%"

                // 小数部を描画する（'%'含む）
                this._数字画像.描画する( dc, left + 396f, top + 48f + 50f, 難易度文字列.Substring( 4 ), new Size2F( 1.0f, 1.0f ) );

                // 整数部を描画する（'.'含む）
                this._数字画像.描画する( dc, left + 150f, top + 48f, 難易度文字列.Substring( 0, 4 ), new Size2F( 1.4f, 1.6f ) );
            }
            else
            {
                // MAX
                this._MAX.描画する( left + 274f, top + 114f );
            }

            // アンダーライン

            DXResources.Instance.D2DBatchDraw( dc, () => {
                using( var brush = new SolidColorBrush( dc, Color4.White ) )
                    dc.FillRectangle( new RectangleF( left + 33f, top + 198f, 553f, 3f ), brush );
            } );
        }



        // ローカル


        private テクスチャ _達成率アイコン;

        private 画像フォント _数字画像;

        private テクスチャ _MAX;
    }
}

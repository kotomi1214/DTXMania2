using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.演奏
{
    class 曲別SKILL : IDisposable
    {

        // 生成と終了


        public 曲別SKILL()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大太斜.png", @"$(System)images\パラメータ文字_大太斜.yaml", 文字幅補正dpx: -6f );
                this._ロゴ画像 = new 画像( @"$(System)images\曲別SKILLアイコン.png" );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._数字画像?.Dispose();
                this._ロゴ画像?.Dispose();
            }
        }



        // 進行と描画


        public void 進行描画する( DeviceContext dc, double? スキル値 )
        {
            if( null == スキル値 )
                return;

            var skill = (double) スキル値;
            var 描画領域 = new RectangleF( 108f, 780f, 275f, 98f );
            string スキル値文字列 = skill.ToString( "0.00" ).PadLeft( 6 ).Replace( ' ', 'o' );  // 右詰め、余白は'o'。

            // 曲別SKILLアイコンを描画する
            var 変換行列2D =
                Matrix3x2.Scaling( 0.375f, 0.5f ) *
                Matrix3x2.Translation( 描画領域.X, 描画領域.Y );

            this._ロゴ画像.描画する( dc, 変換行列2D );

            // 小数部を描画する
            this._数字画像.描画する( dc, 描画領域.X + 90f + 105f, 描画領域.Y + ( 描画領域.Height * 0.2f ), スキル値文字列.Substring( 4 ), new Size2F( 0.65f, 0.8f ) );

            // 整数部を描画する（'.'含む）
            this._数字画像.描画する( dc, 描画領域.X + 90f, 描画領域.Y, スキル値文字列.Substring( 0, 4 ), new Size2F( 0.65f, 1.0f ) );
        }



        // private


        private 画像フォント _数字画像 = null;

        private 画像 _ロゴ画像 = null;
    }
}

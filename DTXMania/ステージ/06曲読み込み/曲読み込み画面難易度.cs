using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;

namespace DTXMania
{
    class 曲読み込み画面難易度 : IDisposable
    {

        // 生成と終了


        public 曲読み込み画面難易度()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大.png", @"$(System)images\パラメータ文字_大.yaml", 文字幅補正dpx: 0f );
                this._見出し用TextFormat = new TextFormat( グラフィックデバイス.Instance.DWriteFactory, "Century Gothic", 50f );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._見出し用TextFormat?.Dispose();
                this._数字画像?.Dispose();
            }
        }



        // 進行と描画


        public void 描画する( DeviceContext dc )
        {
            var 見出し描画領域 = new RectangleF( 783f, 117f, 414f, 63f );
            var 数値描画領域 = new RectangleF( 783f, 180f, 414f, 213f );


            // 現在のフォーカスノードとアンカー値。

            var node = App進行描画.曲ツリー.フォーカス曲ノード;
            var anker = App進行描画.曲ツリー.フォーカス難易度;


            // 難易度のラベルと値を描画する。

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                dc.Transform = グラフィックデバイス.Instance.拡大行列DPXtoPX;
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

                using( var 見出し背景ブラシ = new SolidColorBrush( dc, Node.LevelColor[ anker ] ) )
                using( var 黒ブラシ = new SolidColorBrush( dc, Color4.Black ) )
                using( var 黒透過ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) ) )
                using( var 白ブラシ = new SolidColorBrush( dc, Color4.White ) )
                {
                    // 背景領域を塗りつぶす。
                    dc.FillRectangle( 見出し描画領域, 見出し背景ブラシ );
                    dc.FillRectangle( 数値描画領域, 黒ブラシ );

                    // 見出し文字列を描画する。
                    this._見出し用TextFormat.TextAlignment = TextAlignment.Trailing;
                    var 見出し文字領域 = 見出し描画領域;
                    見出し文字領域.Width -= 8f;    // 右マージン
                    dc.DrawText( node.難易度ラベル, this._見出し用TextFormat, 見出し文字領域, 白ブラシ );

                    // 小数部を描画する。
                    var 数値文字列 = node.難易度.ToString( "0.00" ).PadLeft( 1 );
                    this._数字画像.描画する( dc, 数値描画領域.X + 175f, 数値描画領域.Y, 数値文字列.Substring( 2 ), new Size2F( 2.2f, 2.2f ) );

                    // 整数部と小数点を描画する。
                    this._数字画像.描画する( dc, 数値描画領域.X + 15f, 数値描画領域.Y, 数値文字列.Substring( 0, 2 ), new Size2F( 2.2f, 2.2f ) );
                }

            } );
        }



        // private


        private 画像フォント _数字画像 = null;

        private TextFormat _見出し用TextFormat = null;
    }
}

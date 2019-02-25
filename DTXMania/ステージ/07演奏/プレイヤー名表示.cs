using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;

namespace DTXMania.ステージ.演奏
{
    /// <summary>
    ///		プレイヤー名の表示。
    /// </summary>
    class プレイヤー名表示 : Activity
    {
        public string 名前 { get; set; }


        public プレイヤー名表示()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                //this.名前 = "(no name)";	更新しない。親Activityで設定済みのため。
                this._前回表示した名前 = "";
                this._TextFormat = new TextFormat( グラフィックデバイス.Instance.DWriteFactory, "メイリオ", FontWeight.Regular, FontStyle.Normal, 22f );
                this._TextLayout = null;
                this._文字色 = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, Color4.White );
                this._拡大率X = 1.0f;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._文字色?.Dispose();
                this._文字色 = null;

                this._TextLayout?.Dispose();
                this._TextLayout = null;

                this._TextFormat?.Dispose();
                this._TextFormat = null;
            }
        }

        public void 進行描画する( DeviceContext1 dc )
        {
            var 描画矩形 = new RectangleF( 122f, 313f, 240f, 30f );

            // 初回または名前が変更された場合に TextLayout を再構築する。
            if( ( null == this._TextLayout ) || ( this._前回表示した名前 != this.名前 ) )
            {
                this._TextLayout = new TextLayout( グラフィックデバイス.Instance.DWriteFactory, this.名前, this._TextFormat, 1000f, 30f ) { // 最大1000dpxまで
                    TextAlignment = TextAlignment.Leading,
                    WordWrapping = WordWrapping.NoWrap, // 1000dpxを超えても改行しない（はみ出し分は切り捨て）
                };

                float 文字列幅dpx = this._TextLayout.Metrics.WidthIncludingTrailingWhitespace;
                this._拡大率X = ( 文字列幅dpx <= 描画矩形.Width ) ? 1.0f : ( 描画矩形.Width / 文字列幅dpx );
            }

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                var pretrans = dc.Transform;

                dc.Transform =
                    Matrix3x2.Scaling( this._拡大率X, 1.0f ) *          // 拡大縮小
                    Matrix3x2.Translation( 描画矩形.X, 描画矩形.Y ) *    // 平行移動
                    pretrans;

                dc.DrawTextLayout( Vector2.Zero, this._TextLayout, this._文字色 ); // 座標（描画矩形）は拡大率の影響をうけるので、このメソッドではなく、Matrix3x2.Translation() で設定するほうが楽。

            } );

            this._前回表示した名前 = this.名前;
        }


        private string _前回表示した名前;

        private TextFormat _TextFormat;

        private TextLayout _TextLayout;

        private SolidColorBrush _文字色;

        private float _拡大率X = 1.0f;
    }
}

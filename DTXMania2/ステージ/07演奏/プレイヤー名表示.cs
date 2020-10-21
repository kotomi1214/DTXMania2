using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;

namespace DTXMania2.演奏
{
    /// <summary>
    ///		プレイヤー名の表示。
    /// </summary>
    class プレイヤー名表示 : IDisposable
    {

        // プロパティ


        public string 名前 { get; set; } = "(no nmae)";



        // 生成と終了


        public プレイヤー名表示()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._前回表示した名前 = "";
            this._TextFormat = new TextFormat( Global.GraphicResources.DWriteFactory, "Meiryo", FontWeight.Regular, FontStyle.Normal, 22f );
            this._TextLayout = null;
            this._文字色 = new SolidColorBrush( Global.GraphicResources.既定のD2D1DeviceContext, Color4.White );
            this._拡大率X = 1.0f;
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._文字色.Dispose();
            this._TextLayout?.Dispose();
            this._TextFormat.Dispose();
        }



        // 進行と描画


        public void 進行描画する( DeviceContext d2ddc )
        {
            var 描画矩形 = new RectangleF( 122f, 313f, 240f, 30f );

            // 初回または名前が変更された場合に TextLayout を再構築する。
            if( ( this._TextLayout is null ) || ( this._前回表示した名前 != this.名前 ) )
            {
                const float 最大幅 = 1000f;
                this._TextLayout = new TextLayout( Global.GraphicResources.DWriteFactory, this.名前, this._TextFormat, 最大幅, 30f ) {
                    TextAlignment = TextAlignment.Leading,
                    WordWrapping = WordWrapping.NoWrap, // 最大幅を超えても改行しない（はみ出し分は切り捨て）
                };

                float 文字列幅dpx = this._TextLayout.Metrics.WidthIncludingTrailingWhitespace;
                this._拡大率X = ( 文字列幅dpx <= 描画矩形.Width ) ? 1.0f : ( 描画矩形.Width / 文字列幅dpx );
            }

            var preTrans = d2ddc.Transform;

            d2ddc.Transform =
                Matrix3x2.Scaling( this._拡大率X, 1.0f ) *          // 拡大縮小
                Matrix3x2.Translation( 描画矩形.X, 描画矩形.Y ) *    // 平行移動
                preTrans;

            // 座標（描画矩形）は拡大率の影響をうけるので、このメソッドではなく、Matrix3x2.Translation() で設定するほうが楽。
            d2ddc.DrawTextLayout( Vector2.Zero, this._TextLayout, this._文字色 );

            d2ddc.Transform = preTrans;


            // 更新

            this._前回表示した名前 = this.名前;
        }



        // ローカル


        private string _前回表示した名前;

        private readonly TextFormat _TextFormat;

        private TextLayout? _TextLayout;

        private readonly SolidColorBrush _文字色;

        private float _拡大率X = 1.0f;
    }
}

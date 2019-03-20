using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace FDK
{
    /// <summary>
    ///		DirectWrite を使った Direct2D1ビットマップ。
    /// </summary>
    /// <remarks>
    ///		「表示文字列」メンバを設定/更新すれば、次回の描画時にビットマップが生成される。
    /// </remarks>
    public class 文字列画像 : IDisposable
    {
        /// <summary>
        ///		このメンバを set すれば、次回の進行描画時に画像が更新される。
        /// </summary>
        public string 表示文字列
        {
            get
                => this._表示文字列;
            set
            {
                if( value != this._表示文字列 )
                {
                    this._表示文字列 = value;
                    this._TextLayoutを更新せよ = true;
                }
            }
        }

        public string フォント名
        {
            get
                => this._フォント名;
            set
            {
                if( value != this._フォント名 )
                {
                    this._フォント名 = value;
                    this._TextFormatを更新せよ = true;
                }
            }
        }

        public float フォントサイズpt
        {
            get
                => this._フォントサイズpt;
            set
            {
                if( value != this._フォントサイズpt )
                {
                    this._フォントサイズpt = value;
                    this._TextFormatを更新せよ = true;
                }
            }
        }

        public FontWeight フォント幅
        {
            get
                => this._フォント幅;
            set
            {
                if( value != this._フォント幅 )
                {
                    this._フォント幅 = value;
                    this._TextFormatを更新せよ = true;
                }
            }
        }

        public FontStyle フォントスタイル
        {
            get
                => this._フォントスタイル;
            set
            {
                if( value != this._フォントスタイル )
                {
                    this._フォントスタイル = value;
                    this._TextFormatを更新せよ = true;
                }
            }
        }

        public InterpolationMode 補正モード { get; set; } = InterpolationMode.Linear;

        public RectangleF? 転送元矩形 { get; set; } = null;

        public bool 加算合成 { get; set; } = false;

        /// <summary>
        ///     TextLayout を作る際に使われるレイアウトサイズ[dpx]。
        ///     右揃えや下揃えなどを使う場合には、このサイズを基準にして、文字列の折り返しなどのレイアウトが行われる。
        /// </summary>
        public Size2F レイアウトサイズdpx { get; set; } = Size2F.Zero;

        public Color4 前景色
        {
            get
                => this._前景色;
            set
            {
                if( value != this._前景色 )
                {
                    this._前景色 = value;
                    this._ビットマップを更新せよ = true;
                }
            }
        }

        public Color4 背景色
        {
            get
                => this._背景色;
            set
            {
                if( value != this._背景色 )
                {
                    this._背景色 = value;
                    this._ビットマップを更新せよ = true;
                }
            }
        }

        public enum 効果
        {
            /// <summary>
            ///		前景色で描画。
            /// </summary>
            通常,

            /// <summary>
            ///		文字は前景色で、影は背景色で描画する。
            /// </summary>
            ドロップシャドウ,
            
            /// <summary>
            ///		文字は前景色で、縁は背景色で描画する。
            /// </summary>
            縁取り,
        }

        public 効果 描画効果
        {
            get
                => this._描画効果;
            set
            {
                if( value != this._描画効果 )
                {
                    this._描画効果 = value;
                    this._ビットマップを更新せよ = true;
                }
            }
        }
        
        /// <summary>
        ///		効果が縁取りのときのみ有効。
        /// </summary>
        public float 縁のサイズdpx
        {
            get
                => this._縁のサイズdpx;
            set
            {
                if( value != this._縁のサイズdpx )
                {
                    this._縁のサイズdpx = value;
                    this._ビットマップを更新せよ = true;
                }
            }
        }

        public float LineSpacing { get; protected set; }

        public float Baseline { get; protected set; }

        public WordWrapping WordWrapping
        {
            get
                => this._WordWrapping;
            set
            {
                if( value != this._WordWrapping )
                {
                    this._WordWrapping = value;
                    this._TextFormatを更新せよ = true;
                }
            }
        }

        public ParagraphAlignment ParagraphAlignment
        {
            get
                => this._ParagraphAlignment;
            set
            {
                if( value != this._ParagraphAlignment )
                {
                    this._ParagraphAlignment = value;
                    this._TextFormatを更新せよ = true;
                }
            }
        }

        public TextAlignment TextAlignment
        {
            get
                => this._TextAlignment;
            set
            {
                if( value != this._TextAlignment )
                {
                    this._TextAlignment = value;
                    this._TextFormatを更新せよ = true;
                }
            }
        }

        public TextFormat TextFormat { get; protected set; }

        public TextLayout TextLayout { get; protected set; }

        /// <summary>
        ///     実際に作成されたビットマップ画像のサイズ[dpx]。
        ///     右揃えや下揃えなどを指定している場合は、このサイズは <see cref="レイアウトサイズdpx"/> に等しい。
        ///     指定していない場合は、このサイズは <see cref="_表示文字列のサイズdpx"/> に等しい。
        /// </summary>
        public Size2F 画像サイズdpx { get; protected set; } = Size2F.Zero;



        // 生成と終了


        public 文字列画像()
        {
            // 必要なプロパティは呼び出し元で設定すること。

            this._TextRenderer = new カスタムTextRenderer( グラフィックデバイス.Instance.D2D1Factory1, Color.White, Color.Transparent );    // ビットマップの生成前に。

            this._ビットマップを更新せよ = true;
            this._TextFormatを更新せよ = true;
            this._TextLayoutを更新せよ = true;

            if( this.レイアウトサイズdpx == Size2F.Zero )
                this.レイアウトサイズdpx = グラフィックデバイス.Instance.設計画面サイズ; // 初期サイズとして設計画面サイズを設定。

            // 画像を生成する。

            if( this.表示文字列.Nullでも空でもない() )
                this.ビットマップを生成または更新する();
        }

        public virtual void Dispose()
        {
            this._TextRenderer?.Dispose();
            this._TextRenderer = null;

            this._Bitmap?.Dispose();
            this._Bitmap = null;

            this.TextLayout?.Dispose();
            this.TextLayout = null;

            this.TextFormat?.Dispose();
            this.TextFormat = null;
        }

        protected SharpDX.Direct2D1.BitmapRenderTarget _Bitmap;

        public void ビットマップを生成または更新する()
        {
            if( this._TextFormatを更新せよ )
            {
                #region " テキストフォーマットを更新する。"
                //----------------
                this.TextFormat?.Dispose();
                this.TextFormat = new TextFormat(
                    グラフィックデバイス.Instance.DWriteFactory,
                    this.フォント名,
                    this.フォント幅,
                    this.フォントスタイル,
                    this.フォントサイズpt ) {
                    TextAlignment = this.TextAlignment,
                    ParagraphAlignment = this.ParagraphAlignment,
                    WordWrapping = this.WordWrapping,
                };
                this.TextFormat.SetLineSpacing( LineSpacingMethod.Uniform, this.LineSpacing, this.Baseline );

                // 行間は、プロパティではなくメソッドで設定する。
                this.LineSpacing = FDKUtilities.変換_pt単位からpx単位へ( グラフィックデバイス.Instance.既定のD2D1DeviceContext.DotsPerInch.Width, this.フォントサイズpt );

                // baseline の適切な比率は、lineSpacing の 80 %。（MSDNより）
                this.Baseline = this.LineSpacing * 0.8f;
                //----------------
                #endregion
            }

            if( this._TextLayoutを更新せよ )
            {
                #region " テキストレイアウトを更新する。"
                //----------------
                this.TextLayout?.Dispose();
                this.TextLayout = new TextLayout(
                    グラフィックデバイス.Instance.DWriteFactory,
                    this.表示文字列,
                    this.TextFormat,
                    this.レイアウトサイズdpx.Width,
                    this.レイアウトサイズdpx.Height );

                // レイアウトが変わったのでサイズも更新すｒ。

                this._表示文字列のサイズdpx = new Size2F(
                    this.TextLayout.Metrics.WidthIncludingTrailingWhitespace,
                    this.TextLayout.Metrics.Height );
                //----------------
                #endregion
            }

            if( this._ビットマップを更新せよ ||
                this._TextFormatを更新せよ ||
                this._TextLayoutを更新せよ )
            {
                #region " 古いビットマップレンダーターゲットを解放し、新しく生成する。"
                //----------------
                // D2DContext1.Target が設定済みでない場合、例外も出さずに落ちてしまうので、明示的に弾く。
                using( var target = グラフィックデバイス.Instance.既定のD2D1DeviceContext.Target )    // Target を get すると COM参照カウンタが増えるので注意。
                    Debug.Assert( null != target );

                if( this.ParagraphAlignment != ParagraphAlignment.Near ||
                    this.TextAlignment != TextAlignment.Leading )
                {
                    this.画像サイズdpx = this.レイアウトサイズdpx;       // レイアウトにレイアウトサイズが必要
                }
                else
                {
                    this.画像サイズdpx = this._表示文字列のサイズdpx;    // レイアウトサイズは不要
                }

                if( this.描画効果 == 効果.縁取り )
                {
                    // 縁取りを使うなら少し画像サイズを (+16, +16) 増やす。
                    this.画像サイズdpx = new Size2F(
                        this.画像サイズdpx.Width + 16f, 
                        this.画像サイズdpx.Height + 16f );

                    this._表示文字列のサイズdpx = new Size2F( 
                        this._表示文字列のサイズdpx.Width + 16f, 
                        this._表示文字列のサイズdpx.Height + 16f );
                }

                this._Bitmap?.Dispose();
                this._Bitmap = new SharpDX.Direct2D1.BitmapRenderTarget(
                    グラフィックデバイス.Instance.既定のD2D1DeviceContext, 
                    CompatibleRenderTargetOptions.None,
                    this.画像サイズdpx );
                //----------------
                #endregion

                #region " ビットマップレンダーターゲットをクリアし、テキストを描画する。"
                //----------------
                var rt = this._Bitmap;

                グラフィックデバイス.Instance.D2DBatchDraw( rt, () => {

                    using( var 前景色ブラシ = new SolidColorBrush( this._Bitmap, this.前景色 ) )
                    using( var 背景色ブラシ = new SolidColorBrush( this._Bitmap, this.背景色 ) )
                    {
                        rt.AntialiasMode = AntialiasMode.Aliased;
                        rt.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
                        rt.Transform = Matrix3x2.Identity;  // 等倍描画。(dpx to dpx)

                        rt.Clear( Color.Transparent );

                        switch( this.描画効果 )
                        {
                            case 効果.通常:
                                using( var de = new カスタムTextRenderer.DrawingEffect( rt ) { 文字の色 = this.前景色 } )
                                {
                                    this.TextLayout.Draw( de, this._TextRenderer, 0f, 0f );
                                }
                                break;

                            case 効果.ドロップシャドウ:
                                using( var de = new カスタムTextRenderer.ドロップシャドウDrawingEffect( rt ) { 文字の色 = this.前景色, 影の色 = this.背景色, 影の距離 = 3.0f } )
                                {
                                    this.TextLayout.Draw( de, this._TextRenderer, 0f, 0f );
                                }
                                break;

                            case 効果.縁取り:
                                using( var de = new カスタムTextRenderer.縁取りDrawingEffect( rt ) { 文字の色 = this.前景色, 縁の色 = this.背景色, 縁の太さ = this.縁のサイズdpx } )
                                {
                                    this.TextLayout.Draw( de, this._TextRenderer, 8f, 8f ); // 描画位置をずらす(+8,+8)
                                }
                                break;
                        }
                    }

                } );
                //----------------
                #endregion

                this._ビットマップを更新せよ = false;
                this._TextFormatを更新せよ = false;
                this._TextLayoutを更新せよ = false;
            }
        }


        private bool _ビットマップを更新せよ = true;

        private bool _TextFormatを更新せよ = true;

        private bool _TextLayoutを更新せよ = true;


        // 描画


        public void 描画する( DeviceContext dc, float 左位置, float 上位置, float 不透明度0to1 = 1.0f, float X方向拡大率 = 1.0f, float Y方向拡大率 = 1.0f, Matrix? 変換行列3D = null )
        {
            var 変換行列2D =
                Matrix3x2.Scaling( X方向拡大率, Y方向拡大率 ) *   // 拡大縮小
                Matrix3x2.Translation( 左位置, 上位置 );          // 平行移動

            this.描画する( dc, 変換行列2D, 変換行列3D, 不透明度0to1 );
        }

        public void 描画する( DeviceContext dc, Matrix3x2? 変換行列2D = null, Matrix? 変換行列3D = null, float 不透明度0to1 = 1.0f )
        {
            if( this.表示文字列.Nullまたは空である() )
                return;

            if( this._ビットマップを更新せよ ||
                this._TextFormatを更新せよ ||
                this._TextLayoutを更新せよ )
            {
                this.ビットマップを生成または更新する();

                this._ビットマップを更新せよ = false;
                this._TextFormatを更新せよ = false;
                this._TextLayoutを更新せよ = false;
            }

            if( null == this._Bitmap )
                return;

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                dc.AntialiasMode = AntialiasMode.Aliased;
                dc.PrimitiveBlend = ( this.加算合成 ) ? PrimitiveBlend.Add : PrimitiveBlend.SourceOver;
                dc.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
                dc.UnitMode = UnitMode.Pixels;
                dc.Transform = ( 変換行列2D ?? Matrix3x2.Identity ) * グラフィックデバイス.Instance.拡大行列DPXtoPX;

                using( var bmp = this._Bitmap.Bitmap )
                {
                    dc.DrawBitmap(
                        bitmap: bmp,
                        destinationRectangle: null,
                        opacity: 不透明度0to1,
                        interpolationMode: this.補正モード,
                        sourceRectangle: this.転送元矩形,
                        erspectiveTransformRef: 変換行列3D );
                }

            } );
        }


        
        // private 


        private string _表示文字列 = null;

        private string _フォント名 = "メイリオ";

        private float _フォントサイズpt = 20f;

        private FontWeight _フォント幅 = FontWeight.Normal;

        private FontStyle _フォントスタイル = FontStyle.Normal;

        private Color4 _前景色 = Color4.White;

        private Color4 _背景色 = Color4.Black;

        private 効果 _描画効果 = 効果.通常;

        private float _縁のサイズdpx = 6f;

        private WordWrapping _WordWrapping = WordWrapping.Wrap;

        private ParagraphAlignment _ParagraphAlignment = ParagraphAlignment.Near;

        private TextAlignment _TextAlignment = TextAlignment.Leading;

        private カスタムTextRenderer _TextRenderer;

        /// <summary>
        ///     TextLayout で作成された文字列のサイズ[dpx]。
        ///     <see cref="画像サイズ"/> と同じか、それより小さい。
        /// </summary>
        private Size2F _表示文字列のサイズdpx;
    }
}

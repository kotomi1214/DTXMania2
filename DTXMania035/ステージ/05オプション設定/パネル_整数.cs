using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK32;

namespace DTXMania.ステージ.オプション設定
{
    /// <summary>
    ///		数値ボックス。整数のみ、単位表示は任意。
    /// </summary>
    class パネル_整数 : パネル
    {
        public int 最小値 { get; }

        public int 最大値 { get; }

        public int 現在の値 { get; set; }

        public int 増加減単位値 { get; set; }

        public string 単位 { get; set; }


        public パネル_整数( string パネル名, int 最小値, int 最大値, int 初期値, int 増加減単位値 = 1, string 単位 = "", Action<パネル> 値の変更処理 = null, Color4? ヘッダ色 = null )
            : base( パネル名, 値の変更処理, ヘッダ色 )
        {
            //using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.最小値 = 最小値;
                this.最大値 = 最大値;
                this.現在の値 = 初期値;
                this.増加減単位値 = 増加減単位値;
                this.単位 = 単位;

                this.子Activityを追加する( this._項目画像 = new 文字列画像() { 表示文字列 = "", フォントサイズpt = 34f, 前景色 = Color4.White } );
                Log.Info( $"整数パネルを生成しました。[{this}]" );
            }
        }

        protected override void On活性化()
        {
            base.On活性化();   //忘れないこと
        }

        protected override void On非活性化()
        {
            base.On非活性化();   //忘れないこと
        }

        public override void 左移動キーが入力された()
        {
            // 値を減らす。
            this.現在の値 = Math.Max( this.最小値, this.現在の値 - this.増加減単位値 );

            base.左移動キーが入力された(); // 忘れないこと
        }

        public override void 右移動キーが入力された()
        {
            // 値を増やす。
            this.現在の値 = Math.Min( this.最大値, this.現在の値 + this.増加減単位値 );

            base.右移動キーが入力された(); // 忘れないこと
        }

        public override void 確定キーが入力された()
        {
            // 値を増やす。
            this.現在の値 = ( this.現在の値 + this.増加減単位値 );

            // 最大値を超えたら最小値へループ。
            if( this.現在の値 > this.最大値 )
                this.現在の値 = this.最小値;

            base.確定キーが入力された();  // 忘れないこと
        }

        public override void 進行描画する( DeviceContext1 dc, float left, float top, bool 選択中 )
        {
            // (1) パネルの下地と名前を描画。

            base.進行描画する( dc, left, top, 選択中 );


            // (2) 値を描画。

            this._項目画像.表示文字列 = $"{this.現在の値.ToString()} {this.単位}";
            this._項目画像.ビットマップを生成または更新する();      // このあと画像のサイズが必要になるので、先に生成/更新する。

            float 拡大率Y = (float) this._パネルの高さ割合.Value;
            float 項目の上下マージン = this.項目領域.Height * ( 1f - 拡大率Y ) / 2f;

            var 項目矩形 = new RectangleF(
                x: this.項目領域.X + left,
                y: this.項目領域.Y + top + 項目の上下マージン,
                width: this.項目領域.Width,
                height: this.項目領域.Height * 拡大率Y );

            float 拡大率X = Math.Min( 1f, ( 項目矩形.Width - 20f ) / this._項目画像.画像サイズdpx.Width );    // -20 は左右マージンの最低値[dpx]

            this._項目画像.描画する(
                dc,
                項目矩形.Left + ( 項目矩形.Width - this._項目画像.画像サイズdpx.Width * 拡大率X ) / 2f,
                項目矩形.Top + ( 項目矩形.Height - this._項目画像.画像サイズdpx.Height * 拡大率Y ) / 2f,
                X方向拡大率: 拡大率X,
                Y方向拡大率: 拡大率Y );
        }

        public override string ToString()
            => $"{this.パネル名}, 最小値:{this.最小値}, 最大値:{this.最大値}, 増加減単位値:{this.増加減単位値}, 現在の値:{this.現在の値}";


        private 文字列画像 _項目画像 = null;
    }
}

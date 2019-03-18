using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK32;
using DTXMania.曲;

namespace DTXMania.ステージ.選曲
{
    class 難易度と成績 : Activity
    {
        // 外部接続アクション
        public Func<青い線> 青い線を取得する = null;


        public 難易度と成績()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大.png", @"$(System)images\パラメータ文字_大.yaml", 文字幅補正dpx: 0f ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._現在表示しているノード = null;
                this._見出し用TextFormat = new TextFormat( グラフィックデバイス.Instance.DWriteFactory, "Century Gothic", 16f );
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._見出し用TextFormat?.Dispose();
                this._見出し用TextFormat = null;
            }
        }

        /// <param name="選択している難易度">
        ///		0:BASIC～4:ULTIMATE
        ///	</param>
        public void 描画する( DeviceContext1 dc, int 選択している難易度 )
        {
            if( App.曲ツリー.フォーカスノード != this._現在表示しているノード )
            {
                #region " フォーカスノードが変更されたので情報を更新する。"
                //----------------
                // フォーカス曲ノードではない → このクラスではMusicNode以外も表示できる　というかSetNodeな。
                this._現在表示しているノード = App.曲ツリー.フォーカスノード;
                //----------------
                #endregion
            }


            var node = this._現在表示しているノード;

            bool 表示可能ノードである = ( node is MusicNode ) || ( node is SetNode );


            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                var pretrans = dc.Transform;

                #region " 難易度パネルを描画する。"
                //----------------
                var 領域dpx = new RectangleF( 642f, 529f, 338f, 508f );

                using( var 黒ブラシ = new SolidColorBrush( dc, Color4.Black ) )
                using( var 黒透過ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) ) )
                using( var 白ブラシ = new SolidColorBrush( dc, Color4.White ) )
                using( var ULTIMATE色ブラシ = new SolidColorBrush( dc, Node.LevelColor[ 4 ] ) )
                using( var MASTER色ブラシ = new SolidColorBrush( dc, Node.LevelColor[ 3 ] ) )
                using( var EXTREME色ブラシ = new SolidColorBrush( dc, Node.LevelColor[ 2 ] ) )
                using( var ADVANCED色ブラシ = new SolidColorBrush( dc, Node.LevelColor[ 1 ] ) )
                using( var BASIC色ブラシ = new SolidColorBrush( dc, Node.LevelColor[ 0 ] ) )
                {
                    // 背景
                    dc.FillRectangle( 領域dpx, 黒透過ブラシ );

                    if( 表示可能ノードである )
                    {
                        var 難易度リスト = new (string ラベル, float 値)[ 5 ];

                        if( node is SetNode setNode )
                        {
                            for( int i = 0; i < 5; i++ )
                                難易度リスト[ i ] = (setNode.MusicNodes[ i ]?.難易度ラベル ?? "", setNode.MusicNodes[ i ]?.難易度 ?? 0f);
                        }
                        else
                        {
                            for( int i = 0; i < 5; i++ )
                                難易度リスト[ i ] = ("", 0f);

                            難易度リスト[ 3 ] = (node.難易度ラベル, node.難易度);
                        }

                        // ULTIMATE 相当
                        this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X + 156f, 領域dpx.Y + 13f, 難易度リスト[ 4 ].ラベル, 難易度リスト[ 4 ].値, 白ブラシ, ULTIMATE色ブラシ, 黒ブラシ );

                        // MASTER 相当
                        this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X + 156f, 領域dpx.Y + 114f, 難易度リスト[ 3 ].ラベル, 難易度リスト[ 3 ].値, 白ブラシ, MASTER色ブラシ, 黒ブラシ );

                        // EXTREME 相当
                        this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X + 156f, 領域dpx.Y + 215f, 難易度リスト[ 2 ].ラベル, 難易度リスト[ 2 ].値, 白ブラシ, EXTREME色ブラシ, 黒ブラシ );

                        // ADVANCED 相当
                        this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X + 156f, 領域dpx.Y + 316f, 難易度リスト[ 1 ].ラベル, 難易度リスト[ 1 ].値, 白ブラシ, ADVANCED色ブラシ, 黒ブラシ );

                        // BASIC 相当
                        this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X + 156f, 領域dpx.Y + 417f, 難易度リスト[ 0 ].ラベル, 難易度リスト[ 0 ].値, 白ブラシ, BASIC色ブラシ, 黒ブラシ );
                    }
                }
                //----------------
                #endregion

            } );


            if( 表示可能ノードである )
            {
                #region " 選択枠を描画する。"
                //----------------
                var 青い線 = this.青い線を取得する();

                if( null != 青い線 )
                {
                    var 領域dpx = new RectangleF( 642f + 10f, 529f + 5f + ( 4 - 選択している難易度 ) * 101f, 338f - 20f, 100f );
                    var 太さdpx = 青い線.太さdpx;

                    青い線.描画する( dc, new Vector2( 領域dpx.Left - 太さdpx / 4f, 領域dpx.Top ), 幅dpx: 領域dpx.Width + 太さdpx / 2f );      // 上辺
                    青い線.描画する( dc, new Vector2( 領域dpx.Left, 領域dpx.Top - 太さdpx / 4f ), 高さdpx: 領域dpx.Height + 太さdpx / 2f );   // 左辺
                    青い線.描画する( dc, new Vector2( 領域dpx.Left - 太さdpx / 4f, 領域dpx.Bottom ), 幅dpx: 領域dpx.Width + 太さdpx / 2f );   // 下辺
                    青い線.描画する( dc, new Vector2( 領域dpx.Right, 領域dpx.Top - 太さdpx / 4f ), 高さdpx: 領域dpx.Height + 太さdpx / 2f );  // 右辺
                }
                //----------------
                #endregion
            }
        }


        private 画像フォント _数字画像 = null;

        private Node _現在表示しているノード = null;

        private TextFormat _見出し用TextFormat = null;


        private void _難易度パネルを１つ描画する( DeviceContext1 dc, Matrix3x2 trans, float 基点X, float 基点Y, string 難易度ラベル, float 難易度値, Brush 文字ブラシ, Brush 見出し背景ブラシ, Brush 数値背景ブラシ )
        {
            dc.Transform = trans;

            dc.FillRectangle( new RectangleF( 基点X, 基点Y, 157f, 20f ), 見出し背景ブラシ );
            dc.FillRectangle( new RectangleF( 基点X, 基点Y + 20f, 157f, 66f ), 数値背景ブラシ );


            this._見出し用TextFormat.TextAlignment = TextAlignment.Trailing;

            dc.DrawText( 難易度ラベル, this._見出し用TextFormat, new RectangleF( 基点X + 4f, 基点Y, 157f - 8f, 18f ), 文字ブラシ );


            if( 難易度ラベル.Nullでも空でもない() && 0.00 != 難易度値 )
            {
                var 難易度値文字列 = 難易度値.ToString( "0.00" ).PadLeft( 1 ); // 整数部は２桁を保証（１桁なら十の位は空白文字）

                // 小数部を描画する
                dc.Transform =
                    Matrix3x2.Scaling( 0.5f, 0.5f ) *
                    Matrix3x2.Translation( 基点X + 84f, 基点Y + 36f ) *
                    trans;

                this._数字画像.描画する( dc, 0f, 0f, 難易度値文字列.Substring( 2 ) );

                // 整数部を描画する（'.'含む）
                dc.Transform =
                    Matrix3x2.Scaling( 0.7f, 0.7f ) *
                    Matrix3x2.Translation( 基点X + 20f, 基点Y + 20f ) *
                    trans;

                this._数字画像.描画する( dc, 0f, 0f, 難易度値文字列.Substring( 0, 2 ) );
            }
        }
    }
}

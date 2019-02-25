using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using DTXMania.曲;
using DTXMania.設定;
using DTXMania.データベース.ユーザ;

namespace DTXMania.ステージ.選曲
{
    class 曲別SKILL : Activity
    {
        public 曲別SKILL()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大太斜.png", @"$(System)images\パラメータ文字_大太斜.yaml", 文字幅補正dpx: 0f ) );
                this.子Activityを追加する( this._ロゴ画像 = new テクスチャ( @"$(System)images\曲別SKILLアイコン2.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._現在表示しているノード = null;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public void 進行描画する( DeviceContext1 dc )
        {
            var 描画領域 = new RectangleF( 10f, 340f, 275f, 98f );


            if( App.曲ツリー.フォーカス曲ノード != this._現在表示しているノード )
            {
                #region " フォーカスノードが変更されたので情報を更新する。"
                //----------------
                this._現在表示しているノード = App.曲ツリー.フォーカス曲ノード; // MusicNode 以外は null が返される

                this._スキル値文字列 = null;

                if( null != this._現在表示しているノード )
                {
                    using( var userdb = new UserDB() )
                    {
                        var record = userdb.Records.Where( ( r ) => ( r.UserId == App.ユーザ管理.ログオン中のユーザ.ユーザID && r.SongHashId == this._現在表示しているノード.曲ファイルハッシュ ) ).SingleOrDefault();

                        if( null != record )
                            this._スキル値文字列 = record.Skill.ToString( "0.00" ).PadLeft( 6 );  // 右詰め、余白は' '。
                    }
                }
                //----------------
                #endregion
            }


            if( this._スキル値文字列.Nullまたは空である() )
                return;


            bool 表示可能ノードである = ( this._現在表示しているノード is MusicNode );

            if( 表示可能ノードである )
            {
                // 曲別SKILLアイコンを描画する。

                this._ロゴ画像.描画する( 描画領域.X, 描画領域.Y + 10f, X方向拡大率: 0.5f, Y方向拡大率: 0.4f );


                グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                    var pretrans = dc.Transform;

                    // 小数部を描画する。

                    dc.Transform =
                        Matrix3x2.Scaling( 0.8f, 0.8f ) *
                        Matrix3x2.Translation( 描画領域.X + 130f + 175f, 描画領域.Y + ( 描画領域.Height * 0.2f ) ) *
                        pretrans;

                    this._数字画像.描画する( dc, 0f, 0f, _スキル値文字列.Substring( 4 ) );


                    // 整数部を描画する（'.'含む）。

                    dc.Transform =
                        Matrix3x2.Scaling( 1f, 1.0f ) *
                        Matrix3x2.Translation( 描画領域.X + 130f, 描画領域.Y ) *
                        pretrans;

                    this._数字画像.描画する( dc, 0f, 0f, _スキル値文字列.Substring( 0, 4 ) );

                } );
            }
        }


        private 画像フォント _数字画像 = null;

        private テクスチャ _ロゴ画像 = null;

        private MusicNode _現在表示しているノード = null;

        private string _スキル値文字列 = null;
    }
}

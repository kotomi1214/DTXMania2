using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania
{
    class 曲別SKILL : IDisposable
    {

        // 生成と終了


        public 曲別SKILL()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大太斜.png", @"$(System)images\パラメータ文字_大太斜.yaml", 文字幅補正dpx: 0f );
                this._ロゴ画像 = new テクスチャ( @"$(System)images\曲別SKILLアイコン2.png" );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._ロゴ画像?.Dispose();
                this._数字画像?.Dispose();
            }
        }



        // 進行と描画


        public void 進行描画する( DeviceContext dc )
        {
            var 描画領域 = new RectangleF( 10f, 340f, 275f, 98f );


            if( App進行描画.曲ツリー.フォーカス曲ノード != this._現在表示しているノード )
            {
                #region " フォーカスノードが変更されたので情報を更新する。"
                //----------------
                this._現在表示しているノード = App進行描画.曲ツリー.フォーカス曲ノード; // MusicNode 以外は null が返される

                this._スキル値文字列 = null;

                if( null != this._現在表示しているノード )
                {
                    using( var userdb = new UserDB() )
                    {
                        var record = userdb.Records.Where( ( r ) => ( r.UserId == App進行描画.ユーザ管理.ログオン中のユーザ.ユーザID && r.SongHashId == this._現在表示しているノード.曲ファイルハッシュ ) ).SingleOrDefault();

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


                // 小数部を描画する。
                var 拡大率 = new Size2F( 0.8f, 0.8f );
                this._数字画像.描画する( dc, 描画領域.X + 130f + 175f, 描画領域.Y + ( 描画領域.Height * ( 1.0f - 拡大率.Height) ), _スキル値文字列.Substring( 4 ), 拡大率 );

                // 整数部を描画する（'.'含む）。
                拡大率 = new Size2F( 1.0f, 1.0f );
                this._数字画像.描画する( dc, 描画領域.X + 130f, 描画領域.Y, _スキル値文字列.Substring( 0, 4 ), 拡大率 );
            }
        }



        // private


        private 画像フォント _数字画像 = null;

        private テクスチャ _ロゴ画像 = null;

        private MusicNode _現在表示しているノード = null;

        private string _スキル値文字列 = null;
    }
}

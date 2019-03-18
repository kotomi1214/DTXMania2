using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK32;
using DTXMania.設定;

namespace DTXMania.ステージ.認証
{
    /// <summary>
    ///     ユーザリストパネルの表示とユーザの選択。
    ///     ユーザリストは <see cref="App.ユーザ管理"/> が保持している。
    /// </summary>
    class ユーザリスト : Activity
    {
        /// <summary>
        ///		現在選択中のユーザ。
        ///		0 ～ <see cref="App.ユーザ管理.ユーザリスト.Count"/>-1。
        /// </summary>
        public int 選択中のユーザ { get; protected set; } = 0;


        public ユーザリスト()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var image = (画像) null;

                this._ユーザパネル = new Dictionary<PlayMode, 画像>();

                this.子Activityを追加する( image = new 画像( @"$(System)images\認証\パネル_0.png" ) );
                this._ユーザパネル.Add( PlayMode.BASIC, image );
                this.子Activityを追加する( image = new 画像( @"$(System)images\認証\パネル_1.png" ) );
                this._ユーザパネル.Add( PlayMode.EXPERT, image );

                this._ユーザパネル光彩付き = new Dictionary<PlayMode, 画像>();

                this.子Activityを追加する( image = new 画像( @"$(System)images\認証\パネル_0_光彩あり.png" ) );
                this._ユーザパネル光彩付き.Add( PlayMode.BASIC, image );
                this.子Activityを追加する( image = new 画像( @"$(System)images\認証\パネル_1_光彩あり.png" ) );
                this._ユーザパネル光彩付き.Add( PlayMode.EXPERT, image );

                this._ユーザ肩書きパネル = new Dictionary<PlayMode, 画像>();

                this.子Activityを追加する( image = new 画像( @"$(System)images\認証\肩書きパネル_0.png" ) );
                this._ユーザ肩書きパネル.Add( PlayMode.BASIC, image );
                this.子Activityを追加する( image = new 画像( @"$(System)images\認証\肩書きパネル_1.png" ) );
                this._ユーザ肩書きパネル.Add( PlayMode.EXPERT, image );

                this.子Activityを追加する( this._ユーザ名 = new 文字列画像() {
                    表示文字列 = "",
                    フォントサイズpt = 46f,
                    描画効果 = 文字列画像.効果.縁取り,
                    縁のサイズdpx = 6f,
                    前景色 = Color4.Black,
                    背景色 = Color4.White,
                } );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._光彩アニメカウンタ = new LoopCounter( 0, 200, 5 );
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        /// <summary>
        ///     ユーザリスト上で、選択されているユーザのひとつ前のユーザを選択する。
        /// </summary>
        public void 前のユーザを選択する()
        {
            this.選択中のユーザ = ( this.選択中のユーザ - 1 + App.ユーザ管理.ユーザリスト.Count ) % App.ユーザ管理.ユーザリスト.Count;  // 前がないなら末尾へ

            // アニメーションリセット
            this._光彩アニメカウンタ = new LoopCounter( 0, 200, 5 );
        }

        /// <summary>
        ///     ユーザリスト上で、選択されているユーザのひとつ前のユーザを選択する。
        /// </summary>
        public void 次のユーザを選択する()
        {
            this.選択中のユーザ = ( this.選択中のユーザ + 1 ) % App.ユーザ管理.ユーザリスト.Count;   // 次がないなら先頭へ

            // アニメーションリセット
            this._光彩アニメカウンタ = new LoopCounter( 0, 200, 5 );
        }

        public void 進行描画する( DeviceContext1 dc )
        {
            var 描画位置 = new Vector2( 569f, 188f );
            const float リストの改行幅 = 160f;


            // 選択中のパネルの光彩アニメーションの進行。

            float 不透明度 = 0f;

            var 割合 = this._光彩アニメカウンタ.現在値の割合;

            if( 0.5f > 割合 )
            {
                不透明度 = 1.0f - ( 割合 * 2.0f );     // 1→0
            }
            else
            {
                不透明度 = ( 割合 - 0.5f ) * 2.0f;     // 0→1
            }


            // ユーザリストを描画する。
            
            int 表示人数 = Math.Min( 5, App.ユーザ管理.ユーザリスト.Count );   // HACK: 現状は最大５人までとする。

            for( int i = 0; i < 表示人数; i++ )
            {
                var user = App.ユーザ管理.ユーザリスト[ i ];
                var playMode = user.演奏モード;

                if( i == this.選択中のユーザ )
                    this._ユーザパネル光彩付き[ playMode ].描画する( dc, 描画位置.X, 描画位置.Y + リストの改行幅 * i, 不透明度0to1: 不透明度 );

                this._ユーザパネル[ playMode ].描画する( dc, 描画位置.X, 描画位置.Y + リストの改行幅 * i );

                this._ユーザ名.表示文字列 = user.ユーザ名;
                this._ユーザ名.描画する( dc, 描画位置.X + 32f, 描画位置.Y + 40f + リストの改行幅 * i );

                this._ユーザ肩書きパネル[ playMode ].描画する( dc, 描画位置.X, 描画位置.Y + リストの改行幅 * i );
            }
        }


        private Dictionary<PlayMode, 画像> _ユーザパネル = null;

        private Dictionary<PlayMode, 画像> _ユーザパネル光彩付き = null;

        private Dictionary<PlayMode, 画像> _ユーザ肩書きパネル = null;

        private LoopCounter _光彩アニメカウンタ = null;

        private 文字列画像 _ユーザ名 = null;
    }
}

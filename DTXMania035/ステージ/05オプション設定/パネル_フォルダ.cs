using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using FDK32;

namespace DTXMania.ステージ.オプション設定
{
    /// <summary>
    ///		子パネルリストを持つフォルダ。
    ///		子パネルリストの活性化・非活性化はこのクラスで行う。
    /// </summary>
    class パネル_フォルダ : パネル
    {
        /// <summary>
        ///		null ならルート階層。
        /// </summary>
        public パネル_フォルダ 親パネル { get; protected set; } = null;

        public SelectableList<パネル> 子パネルリスト
        {
            get
                => this._子パネルリスト;
            set
            {
                this._子パネルリスト = value;
                this._子パネルリスト.SelectFirst();
            }
        }


        public パネル_フォルダ( string パネル名, パネル_フォルダ 親パネル, IEnumerable<パネル> 初期子パネルリスト = null, Action<パネル> 値の変更処理 = null )
            : base( パネル名, 値の変更処理, ヘッダ色種別.赤 )
        {
            //using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.親パネル = 親パネル;
                this.子パネルリスト = new SelectableList<パネル>();

                if( null != 初期子パネルリスト )
                {
                    foreach( var panel in 初期子パネルリスト )
                        this.子パネルリスト.Add( panel );

                    this._子パネルリスト.SelectFirst();
                }

                Log.Info( $"フォルダパネルを生成しました。[{this}]" );
            }
        }

        protected override void On活性化()
        {
            base.On活性化();   // 忘れないこと

            foreach( var panel in this.子パネルリスト )
                panel.活性化する();
        }

        protected override void On非活性化()
        {
            foreach( var panel in this.子パネルリスト )
                panel.非活性化する();

            base.On非活性化();  // 忘れないこと
        }

        public override void 進行描画する( DeviceContext1 dc, float left, float top, bool 選択中 )
        {
            // パネルの下地と名前を描画。
            base.進行描画する( dc, left, top, 選択中 );

            // その他の描画があれば、ここに記述する。
        }


        protected SelectableList<パネル> _子パネルリスト = null;
    }
}

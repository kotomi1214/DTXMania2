using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.オプション設定
{
    /// <summary>
    ///     <see cref="パネル_フォルダ"/> の選択と表示。
    /// </summary>
    class パネルリスト : Activity
    {
        public パネル 現在選択中のパネル
            => this._現在のパネルフォルダ.子パネルリスト.SelectedItem;

        public パネル_フォルダ 現在のパネルフォルダ
            => this._現在のパネルフォルダ;


        public パネルリスト()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._青い線 = new 青い線() );
                this.子Activityを追加する( this._パッド矢印 = new パッド矢印() );

                this._ルートパネルフォルダ = null;
                this._現在のパネルフォルダ = null;
            }
        }

        public void パネルリストを登録する( パネル_フォルダ root )
        {
            this._ルートパネルフォルダ = root;
            this._現在のパネルフォルダ = root;
        }

        public void フェードインを開始する( double 速度倍率 = 1.0 )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                for( int i = 0; i < this._現在のパネルフォルダ.子パネルリスト.Count; i++ )
                {
                    this._現在のパネルフォルダ.子パネルリスト[ i ].フェードインを開始する( 0.02, 速度倍率 );
                }
            }
        }

        public void フェードアウトを開始する( double 速度倍率 = 1.0 )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                for( int i = 0; i < this._現在のパネルフォルダ.子パネルリスト.Count; i++ )
                {
                    this._現在のパネルフォルダ.子パネルリスト[ i ].フェードアウトを開始する( 0.02, 速度倍率 );
                }
            }
        }

        public void 前のパネルを選択する()
        {
            Debug.Assert( null != this._現在のパネルフォルダ?.子パネルリスト );

            this._現在のパネルフォルダ.子パネルリスト.SelectPrev( Loop: true );
        }

        public void 次のパネルを選択する()
        {
            Debug.Assert( null != this._現在のパネルフォルダ?.子パネルリスト );

            this._現在のパネルフォルダ.子パネルリスト.SelectNext( Loop: true );
        }

        public void 親のパネルを選択する()
        {
            Debug.Assert( null != this._現在のパネルフォルダ?.親パネル );

            this._現在のパネルフォルダ = this._現在のパネルフォルダ.親パネル;
        }

        public void 子のパネルを選択する()
        {
            Debug.Assert( null != this._現在のパネルフォルダ?.子パネルリスト?.SelectedItem );

            this._現在のパネルフォルダ = this._現在のパネルフォルダ.子パネルリスト.SelectedItem as パネル_フォルダ;
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                Debug.Assert( this._ルートパネルフォルダ != null, "フォルダツリーが登録されていません。" );

                this._現在のパネルフォルダ = this._ルートパネルフォルダ;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._ルートパネルフォルダ = null;    // 実体は外で管理されるので、ここでは Dispose 不要。
                this._現在のパネルフォルダ = null;    //
            }
        }

        public void 進行描画する( DeviceContext1 dc, float left, float top )
        {
            const float パネルの下マージン = 4f;
            float パネルの高さ = パネル.サイズ.Height + パネルの下マージン;

            // (1) フレーム１（たて線）を描画。

            this._青い線.描画する( dc, new Vector2( left, 0f ), 高さdpx: グラフィックデバイス.Instance.設計画面サイズ.Height );


            // (2) パネルを描画。（選択中のパネルの3つ上から7つ下までの計11枚。）

            var panels = this._現在のパネルフォルダ.子パネルリスト;

            for( int i = 0; i < 11; i++ )
            {
                int 描画パネル番号 = ( ( panels.SelectedIndex - 3 + i ) + panels.Count * 3 ) % panels.Count;       // panels の末尾に達したら先頭に戻る。
                var 描画パネル = panels[ 描画パネル番号 ];

                描画パネル.進行描画する(
                    dc,
                    left + 22f,
                    top + i * パネルの高さ,
                    選択中: ( i == 3 ) );
            }


            // (3) フレーム２（選択パネル周囲）を描画。

            float 幅 = パネル.サイズ.Width + 22f * 2f;

            this._青い線.描画する( dc, new Vector2( left, パネルの高さ * 3f ), 幅dpx: 幅 );
            this._青い線.描画する( dc, new Vector2( left, パネルの高さ * 4f ), 幅dpx: 幅 );
            this._青い線.描画する( dc, new Vector2( left + 幅, パネルの高さ * 3f ), 高さdpx: パネルの高さ );


            // (4) パッド矢印（上＆下）を描画。

            this._パッド矢印.描画する( dc, パッド矢印.種類.上_Tom1, new Vector2( left, パネルの高さ * 3f ) );
            this._パッド矢印.描画する( dc, パッド矢印.種類.下_Tom2, new Vector2( left, パネルの高さ * 4f ) );
        }


        private パネル_フォルダ _ルートパネルフォルダ = null;

        private パネル_フォルダ _現在のパネルフォルダ = null;

        private 青い線 _青い線 = null;

        private パッド矢印 _パッド矢印 = null;
    }
}

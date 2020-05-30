using System;
using System.Collections.Generic;
using System.Text;
using FDK;

namespace DTXMania2.曲
{
    class 曲ツリー_評価順 : 曲ツリー
    {

        // 生成と終了


        /// <summary>
        ///     <see cref="App.全曲リスト"/> を元に、評価順曲ツリーを構築する。
        /// </summary>
        public 曲ツリー_評価順()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this.初期化する();
        }

        public override void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            base.Dispose();
        }


        public void 初期化する()
        {
            // 評価順ツリーだけが使用するノードを解放する。
            if( 0 < this.ルートノード.子ノードリスト.Count )
            {
                this.ルートノード.子ノードリスト[ 0 ].Dispose(); // Back
                this.ルートノード.子ノードリスト[ 1 ].Dispose(); // RandomSelect
                for( int i = 0; i < 6; i++ )
                {
                    var box = (BoxNode) this.ルートノード.子ノードリスト[ 2 + i ];
                    box.子ノードリスト[ 0 ].Dispose(); // Back
                    box.子ノードリスト[ 1 ].Dispose(); // RandomSelect
                    box.Dispose();
                }

                this.ルートノード.子ノードリスト.Clear();
            }

            // RANDOM SELECT を追加する。
            this.ルートノード.子ノードリスト.Add( new RandomSelectNode() { 親ノード = this.ルートノード } );

            // 評価BOX を追加する。
            var ratingBoxLabel = new[] {
                "評価 ★★★★★",
                "評価 ★★★★",
                "評価 ★★★",
                "評価 ★★",
                "評価 ★",
                "評価 なし",
            };
            this._評価BOX = new BoxNode[ 6 ];   // 0～5
            for( int i = 0; i < this._評価BOX.Length; i++ )
            {
                this._評価BOX[ i ] = new BoxNode( ratingBoxLabel[ i ] ) { 親ノード = this.ルートノード };
                this._評価BOX[ i ].子ノードリスト.Add( new BackNode() { 親ノード = this._評価BOX[ i ] } );
                this._評価BOX[ i ].子ノードリスト.Add( new RandomSelectNode() { 親ノード = this._評価BOX[ i ] } );
            }
            this.ルートノード.子ノードリスト.AddRange( this._評価BOX );

            // 最初のノードを選択する。
            this.フォーカスリスト.SelectFirst();
        }

        public void 再構築する()
        {
        }


        protected BoxNode[] _評価BOX = null!; // [0～5]
    }
}

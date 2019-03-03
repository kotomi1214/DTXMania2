using System;	
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FDK;

namespace DTXMania.曲
{
    /// <summary>
    ///		選曲画面で使用される、曲ツリーを管理する。
    ///		曲ツリーは、<see cref="ユーザ"/>ごとに１つずつ持つことができる。
    /// </summary>
    class 曲ツリー : Activity, IDisposable
    {
        // プロパティ

        /// <summary>
        ///		曲ツリーのルートを表すノード。
        ///		フォーカスリストやフォーカスノードも、このツリーの中に実態がある。
        /// </summary>
        public RootNode ルートノード { get; } = new RootNode();

        /// <summary>
        ///		現在選択されているノード。
        /// </summary>
        /// <remarks>
        ///		未選択またはフォーカスリストが空の場合は null 。
        ///		<see cref="フォーカスリスト"/>の<see cref="SelectableList{T}.SelectedIndex"/>で変更できる。
        ///	</remarks>
        public Node フォーカスノード
        {
            get
            {
                if( null == this.フォーカスリスト )
                    return null;    // 未設定。

                if( 0 > this.フォーカスリスト.SelectedIndex )
                    return null;    // リストが空。

                return this.フォーカスリスト[ this.フォーカスリスト.SelectedIndex ];
            }
        }

        /// <summary>
        ///	    <see cref="フォーカスノード"/> が存在するノードリスト。
        ///	    変更するには、変更先のリスト内の任意のノードを選択すること。
        /// </summary>
        public SelectableList<Node> フォーカスリスト { get; protected set; } = null;

        /// <summary>
        ///		現在選択されているノードから曲ノードを取得して返す。
        /// </summary>
        /// <remarks>
        ///		<see cref="フォーカスノード"/>が<see cref="SetNode"/>型である場合は、それが保有する難易度の中で、
        ///		現在の <see cref="ユーザ希望難易度"/> に一番近い難易度の <see cref="MusicNode"/> が返される。
        ///		それ以外の場合は常に null が返される。
        /// </remarks>
        public MusicNode フォーカス曲ノード
        {
            get
            {
                if( this.フォーカスノード is MusicNode musicnode )
                {
                    return musicnode;
                }
                if( this.フォーカスノード is SetNode setnode )
                {
                    return setnode.MusicNodes[ this.フォーカス難易度 ];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///		現在選択されているノードが対応している、現在の <see cref="ユーザ希望難易度"/> に一番近い難易度（0:BASIC～4:ULTIMATE）を返す。
        /// </summary>
        public int フォーカス難易度
        {
            get
            {
                if( this.フォーカスノード is MusicNode musicnode )
                {
                    return 3;   // MASTER 相当で固定
                }
                else if( this.フォーカスノード is SetNode setnode )
                {
                    return setnode.ユーザ希望難易度に最も近い難易度レベルを返す( this.ユーザ希望難易度 );
                }
                else
                {
                    return 0;   // BoxNode, BackNode など
                }
            }
        }

        /// <summary>
        ///     ユーザが希望している難易度。
        /// </summary>
        public int ユーザ希望難易度 { get; protected set; } = 3;


        // イベント

        /// <summary>
        ///     フォーカスノードが変更された場合に発生するイベント。
        /// </summary>
        /// <remarks>
        ///     選択されたノードと、選択が解除されたノードは、必ずしも同じNodeリストに存在するとは限らない。
        ///     例えば、BOXを移動する場合、選択されるNodeは移動後のNodeリストに、選択が解除されたNodeは移動前のNodeリストに、それぞれ存在する。
        ///     この場合、後者はすでに非活性化されているので注意すること。
        /// </remarks>
        public event EventHandler<(Node 選択されたNode, Node 選択が解除されたNode)> フォーカスノードが変更された;


        // 構築・破棄

        public 曲ツリー()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._曲ツリーの構築 = new 曲ツリーの構築();
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                Debug.Assert( this.活性化していない );

                // フォーカスリストを活性化する。
                if( null != this.フォーカスリスト )
                {
                    foreach( var node in this.フォーカスリスト )
                        node.活性化する();
                }

                //this.ユーザ希望難易度 = 3;	-> 初期化せず、前回の値を継承する。
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                Debug.Assert( this.活性化している );

                // フォーカスリストを非活性化する。
                if( null != this.フォーカスリスト )
                {
                    foreach( var node in this.フォーカスリスト )
                        node.非活性化する();
                }
            }
        }

        public void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.すべてのノードを削除する();
            }
        }

        public void 曲の検索を開始する( VariablePath フォルダパス )
        {
            this._曲ツリーの構築.検索フォルダを追加する( フォルダパス, this.ルートノード );
        }

        public void すべてのノードを削除する()
        {
            Debug.Assert( this.活性化していない );  // 活性化状態のノードが存在していないこと。

            this.フォーカスリスト = null;
            this.ルートノード.子ノードリスト.Clear();
        }


        // 難易度

        public void 難易度アンカをひとつ増やす()
        {
            for( int i = 0; i < 5; i++ )   // 最大でも5回まで
            {
                this.ユーザ希望難易度 = ( this.ユーザ希望難易度 + 1 ) % 5;

                if( this.フォーカスノード is SetNode setnode )
                {
                    if( null != setnode.MusicNodes[ this.ユーザ希望難易度 ] )
                        return; // その難易度に対応する曲ノードがあればOK。
                }

                // なければ次のアンカへ。
            }
        }

        
        // フォーカス

        /// <summary>
        ///		指定されたノードをフォーカスする。
        ///		<see cref="フォーカスリスト"/>もそのノードのあるリストへ変更される。
        ///		現在活性化中である場合、移動前のフォーカスリストは非活性化され、新しいフォーカスリストが活性化される。
        /// </summary>
        public void フォーカスする( Node ノード )
        {
            //Debug.Assert( this.活性化している );	--> どちらの状態で呼び出してもよい。

            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 親ノード = ノード?.親ノード ?? this.ルートノード;
                Trace.Assert( null != 親ノード?.子ノードリスト );


                // 必要あればフォーカスリストを変更。

                var 旧フォーカスリスト = this.フォーカスリスト;    // 初回は null 。
                this.フォーカスリスト = 親ノード.子ノードリスト;   // 常に非 null。（先のAssertで保証されている。）

                lock( 親ノード.子ノードリスト排他 )
                {
                    if( 旧フォーカスリスト == this.フォーカスリスト )
                    {
                        // (A) フォーカスリストが変わらない場合 → 必要あればフォーカスノードを変更する。

                        if( null != ノード )
                        {
                            // (A-a) ノードの指定がある（非null）なら、それを選択する。
                            this.フォーカスリスト.SelectItem( ノード );
                        }
                        else
                        {
                            // (A-b) ノードの指定がない（null）なら、フォーカスノードは現状のまま維持する。
                        }
                    }
                    else
                    {
                        // (B) フォーカスリストが変更される場合

                        Log.Info( "フォーカスリストが変更されました。" );

                        if( this.活性化している )
                        {
                            if( null != 旧フォーカスリスト ) // 初回は null 。
                            {
                                旧フォーカスリスト.SelectionChanged -= this.フォーカスリスト_SelectionChanged;   // ハンドラ削除
                                foreach( var node in 旧フォーカスリスト )
                                    node.非活性化する();
                            }

                            foreach( var node in this.フォーカスリスト )
                                node.活性化する();

                            if( null != ノード )
                                this.フォーカスリスト.SelectItem( ノード );    // イベントハンドラ登録前

                            this.フォーカスリスト.SelectionChanged += this.フォーカスリスト_SelectionChanged;   // ハンドラ登録

                            // 手動でイベントを発火。
                            this.フォーカスノードが変更された?.Invoke( this.フォーカスリスト, (this.フォーカスリスト?.SelectedItem, 旧フォーカスリスト?.SelectedItem) );
                        }
                    }
                }
            }
        }

        /// <remarks>
        ///		末尾なら先頭に戻る。
        /// </remarks>
        public void 次のノードをフォーカスする()
        {
            var index = this.フォーカスリスト.SelectedIndex;

            if( 0 > index )
                return; // 現在フォーカスされているノードがない。

            index = ( index + 1 ) % this.フォーカスリスト.Count;

            this.フォーカスリスト.SelectItem( index );
        }

        /// <remarks>
        ///		先頭なら末尾に戻る。
        /// </remarks>
        public void 前のノードをフォーカスする()
        {
            var index = this.フォーカスリスト.SelectedIndex;

            if( 0 > index )
                return; // 現在フォーカスされているノードがない。

            index = ( index - 1 + this.フォーカスリスト.Count ) % this.フォーカスリスト.Count;

            this.フォーカスリスト.SelectItem( index );
        }


        private 曲ツリーの構築 _曲ツリーの構築 = null;

        private void フォーカスリスト_SelectionChanged( object sender, (Node 選択されたItem, Node 選択が解除されたItem) e )
        {
            // 間接呼び出し；
            // フォーカスリストの SelectedChanged イベントハンドラ　→　このクラス内で変更されうる
            // 外部に対するイベントハンドラ　→　このクラス内では変更されない
            this.フォーカスノードが変更された?.Invoke( sender, e );
        }
    }
}

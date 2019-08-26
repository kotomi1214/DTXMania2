using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania
{
    /// <summary>
    ///		曲ツリー階層において「RANDOM SELECT」を表すノード。
    /// </summary>
    class RandomSelectNode : Node
    {
        /// <summary>
        ///     選択された曲(<see cref="MusicNode"/>) または <see cref="SetNode"/>。
        /// </summary>
        public Node 選択曲 { get; protected set; }


        public RandomSelectNode( Node 親ノード = null )
        {
            this.タイトル = "< RANDOM SELECT >";
            this.親ノード = 親ノード;
        }

        public void 選択曲を変更する()
        {
            int 対象曲数 = this._MusicNode数を返す( this.親ノード );

            int 対象曲Index = App進行描画.乱数.Next( 対象曲数 ); // 乱数で対象曲を選択

            this.選択曲 = this._指定されたインデックス番目の曲を返す( ref 対象曲Index, this.親ノード );
        }


        private int _MusicNode数を返す( Node parent )
        {
            int 曲数 = 0;

            foreach( var node in parent.子ノードリスト )
            {
                switch( node )
                {
                    case MusicNode musicNode:
                        曲数++;
                        break;

                    case SetNode setNode:
                        曲数++;
                        break;

                    case BoxNode boxNode:
                        曲数 += this._MusicNode数を返す( boxNode );
                        break;
                }
            }

            return 曲数;
        }

        private Node _指定されたインデックス番目の曲を返す( ref int index, Node parent )
        {
            foreach( var node in parent.子ノードリスト )
            {
                switch( node )
                {
                    case MusicNode musicNode:
                        {
                            index--;

                            if( index < 0 )
                                return musicNode;
                        }
                        break;

                    case SetNode setNode:
                        {
                            index--;

                            if( index < 0 )
                                return setNode;
                        }
                        break;

                    case BoxNode boxNode:
                        {
                            var mn = this._指定されたインデックス番目の曲を返す( ref index, boxNode );

                            if( index < 0 )
                                return mn;
                        }
                        break;
                }
            }

            return null;
        }
    }
}

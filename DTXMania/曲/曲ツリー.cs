using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FDK32;

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

        public void すべてのノードを削除する()
        {
            Debug.Assert( this.活性化していない );  // 活性化状態のノードが存在していないこと。

            this.フォーカスリスト = null;

            lock( this.ルートノード.子ノードリスト排他 )
                this.ルートノード.子ノードリスト.Clear();
        }

        public void 曲の検索を開始する( VariablePath フォルダパス )
        {
            this._検索フォルダを追加する( フォルダパス, this.ルートノード );
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


        private void フォーカスリスト_SelectionChanged( object sender, (Node 選択されたItem, Node 選択が解除されたItem) e )
        {
            // 間接呼び出し；
            // フォーカスリストの SelectedChanged イベントハンドラ　→　このクラス内で変更されうる
            // 外部に対するイベントハンドラ　→　このクラス内では変更されない
            this.フォーカスノードが変更された?.Invoke( sender, e );
        }


        // 曲検索・構築タスク

        private ConcurrentQueue<(Node parent, VariablePath path)> _検索フォルダキュー = new ConcurrentQueue<(Node parent, VariablePath path)>();

        private AutoResetEvent _検索フォルダキュー投入通知 = new AutoResetEvent( false );    // キューに格納した際には必ず Set すること。

        private Task _構築タスク = null;

        private string[] _対応する拡張子 = { ".sstf", ".dtx", ".gda", ".g2d", "bms", "bme" };


        /// <summary>
        ///     <see cref="_検索フォルダキュー"/> にフォルダを投入する。
        ///     また、構築タスクが起動していなければ、起動する。
        /// </summary>
        /// <param name="フォルダパス">検索対象のフォルダのパス。</param>
        /// <param name="追加先親ノード">検索結果を追加する先の親ノード</param>
        private void _検索フォルダを追加する( VariablePath フォルダパス, Node 追加先親ノード )
        {
            this._検索フォルダキュー.Enqueue( (追加先親ノード, フォルダパス) );
            this._検索フォルダキュー投入通知.Set();

            // 構築タスクを開始していなければ開始する。

            if( null == this._構築タスク )
            {
                this._構築タスク = Task.Run( () => {

                    #region " 構築タスクの内容；キューから取り出しては構築する。"
                    //----------------
                    Log.現在のスレッドに名前をつける( "曲検索" );
                    Log.Info( $"曲ツリーの構築タスクを開始します。" );

                    // キューに項目が投入されるまで待つ。
                    // → タイムアウトしたら、これ以上の投入はないものと見なして、ループを抜ける。
                    while( this._検索フォルダキュー投入通知.WaitOne( 5000 ) )
                    {
                        // キュー内のすべての項目について……
                        while( this._検索フォルダキュー.TryDequeue( out var item ) )
                        {
                            // フォルダを検索し、ツリーを構築する。
                            Log.Info( $"検索中: {item.path.変数付きパス}" );
                            this._構築する( item.parent, item.path );
                        }
                    }

                    Log.Info( $"曲ツリーの構築タスクを終了しました。" );
                    //----------------
                    #endregion

                } );
            }
        }

        /// <summary>
        ///     <see cref="_検索フォルダキュー"/> からフォルダと追加先親ノードを取り出し、
        ///     ノードリストを構築して、親ノードの子ノードリストに追加する。
        /// </summary>
        private void _構築する( Node 親ノード, VariablePath 基点フォルダパス, bool boxDefファイル有効 = true )
        {
            if( !( Directory.Exists( 基点フォルダパス.変数なしパス ) ) )
            {
                Log.WARNING( $"指定されたフォルダが存在しません。無視します。[{基点フォルダパス.変数付きパス}]" );
                return;
            }

            // 以下(A)～(C)で生成したノードはいったんこのノードリストに格納し、あとでまとめて曲ツリーに登録する(D)。
            List<Node> ノードリスト = new List<Node>();

            var dirInfo = new DirectoryInfo( 基点フォルダパス.変数なしパス );
            var boxDefPath = new VariablePath( Path.Combine( 基点フォルダパス.変数なしパス, @"box.def" ) );
            var setDefPath = new VariablePath( Path.Combine( 基点フォルダパス.変数なしパス, @"set.def" ) );
            bool サブフォルダを検索する = true;

            if( boxDefファイル有効 && File.Exists( boxDefPath.変数なしパス ) )
            {
                #region " (A) このフォルダに box.def がある → BOXノードを作成し、子ノードリストを作成する。"
                //----------------
                try
                {
                    // BOXノードを作成。
                    var boxNode = new BoxNode( boxDefPath, null );
                    ノードリスト.Add( boxNode );

                    // BOXノード内に "戻る" ノードを追加。
                    lock( boxNode.子ノードリスト排他 )
                    {
                        var backNode = new BackNode( boxNode );
                        boxNode.子ノードリスト.Add( backNode );
                    }

                    // box.defを無効にして、このフォルダを対象として、再度構築する。
                    // 構築結果のノードリストは、BOXノードの子として付与される。
                    this._構築する( boxNode, 基点フォルダパス, false );

                    // box.def があった場合、サブフォルダは検索しない。
                    サブフォルダを検索する = false;
                }
                catch
                {
                    Log.ERROR( $"box.def に対応するノードの生成に失敗しました。[{setDefPath.変数付きパス}]" );
                }
                //----------------
                #endregion
            }
            else if( File.Exists( setDefPath.変数なしパス ) )
            {
                #region " (B) このフォルダに set.def がある → その内容でSetノード（任意個）を作成する。"
                //----------------
                try
                {
                    // set.def を読み込む。
                    var setDef = SetDef.復元する( setDefPath );

                    // set.def 内のすべてのブロックについて……
                    foreach( var block in setDef.Blocks )
                    {
                        // Setノードを作成し、追加。
                        var setNode = new SetNode( block, 基点フォルダパス, null );
                        if( 0 < setNode.子ノードリスト.Count ) // L1～L5のいずれかが有効であるときのみ登録する。
                            ノードリスト.Add( setNode );
                    }

                    // set.def があった場合、サブフォルダは検索しない。
                    サブフォルダを検索する = false;
                }
                catch
                {
                    Log.ERROR( $"set.def からのノードの生成に失敗しました。[{setDefPath.変数付きパス}]" );
                }
                //----------------
                #endregion
            }
            else
            {
                #region " (C) このフォルダにあるすべての曲ファイルを検索して、曲ノードを作成する。"
                //----------------
                // 対応する拡張子を持つファイルを列挙する。
                var fileInfos = dirInfo.GetFiles( "*.*", SearchOption.TopDirectoryOnly )
                    .Where( ( fileInfo ) => _対応する拡張子.Any( 拡張子名 => ( Path.GetExtension( fileInfo.Name ).ToLower() == 拡張子名 ) ) );

                // 列挙されたそれぞれのファイルについて……
                foreach( var fileInfo in fileInfos )
                {
                    var vpath = new VariablePath( fileInfo.FullName );
                    try
                    {
                        // Musicノードを作成し、追加。
                        var music = new MusicNode( vpath, null );
                        ノードリスト.Add( music );
                    }
                    catch
                    {
                        Log.ERROR( $"MusicNode の生成に失敗しました。[{vpath.変数付きパス}]" );
                    }

                    // 続けて、サブフォルダも検索する。
                    サブフォルダを検索する = true;
                }
                //----------------
                #endregion
            }

            #region " (D) 作成したノードリストを親ノードの子として追加する。"
            //----------------
            lock( 親ノード.子ノードリスト排他 )  // lock は、ノードリスト単位で行う（ノード単位ではない）。
            {
                foreach( var item in ノードリスト )
                {
                    item.親ノード = 親ノード;
                    親ノード.子ノードリスト.Add( item );
                }
            }
            //----------------
            #endregion

            if( サブフォルダを検索する )
            {
                // (E) このフォルダ内のサブフォルダについて……
                foreach( var dir in dirInfo.GetDirectories() )
                {
                    if( dir.Name.StartsWith( "DTXFiles.", StringComparison.OrdinalIgnoreCase ) )
                    {
                        #region " (E-a) サブフォルダがBOXである(1) → BOXノードを追加し、検索キューに検索予約を投入する。"
                        //----------------
                        // BOXノードを作成し、ツリーに登録。
                        var boxNode = new BoxNode( dir.Name.Substring( 9 ), 親ノード );
                        lock( 親ノード.子ノードリスト排他 )
                            親ノード.子ノードリスト.Add( boxNode );

                        // BOXノード内に "戻る" ノードを追加。
                        lock( boxNode.子ノードリスト排他 )
                        {
                            var backNode = new BackNode( boxNode );
                            boxNode.子ノードリスト.Add( backNode );
                        }

                        // BOXノードを親として、検索予約をキューに投入。
                        this._検索フォルダキュー.Enqueue( (boxNode, dir.FullName) );
                        this._検索フォルダキュー投入通知.Set();
                        //----------------
                        #endregion
                    }
                    else if( File.Exists( Path.Combine( dir.FullName, @"box.def" ) ) )
                    {
                        #region " (E-b) サブフォルダがBOXである(2) → 検索キューに検索予約を投入する。"
                        //----------------
                        // 同じノードを親として、検索予約をキューに投入。
                        this._検索フォルダキュー.Enqueue( (親ノード, dir.FullName) );
                        this._検索フォルダキュー投入通知.Set();
                        //----------------
                        #endregion
                    }
                    else
                    {
                        #region " (E-c) サブフォルダの内容をこのノードリストに追加する。"
                        //----------------
                        // 同じノードを親として構築を続行。
                        this._構築する( 親ノード, dir.FullName );
                        //----------------
                        #endregion
                    }
                }
            }
        }
    }
}

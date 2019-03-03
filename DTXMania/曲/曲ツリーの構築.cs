using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FDK;

namespace DTXMania.曲
{
    class 曲ツリーの構築
    {
        public void 検索フォルダを追加する( VariablePath フォルダパス, Node 追加先親ノード )
        {
            this._検索フォルダキュー.Enqueue( (追加先親ノード, フォルダパス) );
            this._検索フォルダキュー投入通知.Set();

            // 開始していなければ開始する。
            if( null == this._構築タスク )
            {
                this._構築タスク = Task.Run( () => {

                    Log.現在のスレッドに名前をつける( "曲検索" );
                    Log.Info( $"曲ツリーの構築タスクを開始します。" );

                    while( this._検索フォルダキュー投入通知.WaitOne( 5000 ) )   // タイムアウトしたら、これ以上投入はないものとしてループを抜ける。
                    {
                        while( this._検索フォルダキュー.TryDequeue( out var item ) )
                        {
                            Log.Info( $"検索中: {item.path.変数なしパス}" );
                            this._構築する( item.parent, item.path );
                        }
                    }

                    Log.Info( $"曲ツリーの構築タスクを終了しました。" );

                } );
            }
        }


        private ConcurrentQueue<(Node parent, VariablePath path)> _検索フォルダキュー = new ConcurrentQueue<(Node parent, VariablePath path)>();

        private AutoResetEvent _検索フォルダキュー投入通知 = new AutoResetEvent( false );

        private Task _構築タスク = null;


        private void _構築する( Node 親ノード, VariablePath 基点フォルダパス, bool boxDefファイル有効 = true )
        {
            if( !( Directory.Exists( 基点フォルダパス.変数なしパス ) ) )
            {
                Log.WARNING( $"指定されたフォルダが存在しません。無視します。[{基点フォルダパス.変数付きパス}]" );
                return;
            }

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

                    // 子に "戻る" を追加。
                    lock( boxNode.子ノードリスト排他 )
                    {
                        var backNode = new BackNode( boxNode );
                        boxNode.子ノードリスト.Add( backNode );
                    }

                    // box.defを無効にして、このフォルダを再度構築する。
                    this._構築する( boxNode, 基点フォルダパス, false );

                    サブフォルダを検索する = false;    // しない
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
                    var setDef = SetDef.復元する( setDefPath );

                    foreach( var block in setDef.Blocks )
                    {
                        var setNode = new SetNode( block, 基点フォルダパス, null );

                        if( 0 < setNode.子ノードリスト.Count ) // L1～L5のいずれかが有効であるときのみ登録する。
                            ノードリスト.Add( setNode );
                    }

                    サブフォルダを検索する = false;    // しない
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
                var fileInfos = dirInfo.GetFiles( "*.*", SearchOption.TopDirectoryOnly )
                    .Where( ( fileInfo ) => _対応する拡張子.Any( 拡張子名 => ( Path.GetExtension( fileInfo.Name ).ToLower() == 拡張子名 ) ) );

                foreach( var fileInfo in fileInfos )
                {
                    var vpath = new VariablePath( fileInfo.FullName );

                    try
                    {
                        var music = new MusicNode( vpath, null );
                        ノードリスト.Add( music );
                    }
                    catch
                    {
                        Log.ERROR( $"MusicNode の生成に失敗しました。[{vpath.変数付きパス}]" );
                    }

                    サブフォルダを検索する = true;     // する
                }
                //----------------
                #endregion
            }

            #region " (D) 作成したノードリストを親ノードの子として追加する。"
            //----------------
            lock( 親ノード.子ノードリスト排他 )
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
                // (E) このフォルダ内のサブフォルダについて処理する。
                foreach( var dir in dirInfo.GetDirectories() )
                {
                    if( dir.Name.StartsWith( "DTXFiles.", StringComparison.OrdinalIgnoreCase ) )
                    {
                        #region " (E-a) サブフォルダがBOXである → BOXノードを追加し、検索キューに検索予約を投入する。"
                        //----------------
                        // BOXノードを作成。
                        var boxNode = new BoxNode( dir.Name.Substring( 9 ), 親ノード );
                        lock( 親ノード.子ノードリスト排他 )
                            親ノード.子ノードリスト.Add( boxNode );

                        // 子に "戻る" を追加。
                        lock( boxNode.子ノードリスト排他 )
                        {
                            var backNode = new BackNode( boxNode );
                            boxNode.子ノードリスト.Add( backNode );
                        }

                        // 検索予約をキューに投入。
                        this._検索フォルダキュー.Enqueue( (boxNode, dir.FullName) );
                        this._検索フォルダキュー投入通知.Set();
                        //----------------
                        #endregion
                    }
                    else if( File.Exists( Path.Combine( dir.FullName, @"box.def" ) ) )
                    {
                        #region " (E-b) サブフォルダがBOXである → 検索キューに検索予約を投入する。"
                        //----------------
                        this._検索フォルダキュー.Enqueue( (親ノード, dir.FullName) );
                        this._検索フォルダキュー投入通知.Set();
                        //----------------
                        #endregion
                    }
                    else
                    {
                        #region " (E-c) サブフォルダの内容をこのノードリストに追加する。"
                        //----------------
                        this._構築する( 親ノード, dir.FullName );
                        //----------------
                        #endregion
                    }
                }
            }
        }

        private string[] _対応する拡張子 = { ".sstf", ".dtx", ".gda", ".g2d", "bms", "bme" };
    }
}

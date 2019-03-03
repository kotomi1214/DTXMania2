using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX.Direct3D11;
using SSTFormat.v4;
using FDK;
using DTXMania.設定;
using DTXMania.データベース.曲;

namespace DTXMania.曲
{
    /// <summary>
    ///		曲ツリー階層において「曲」を表すノード。
    /// </summary>
    class MusicNode : Node
    {
        /// <summary>
        ///		この曲ノードに対応する曲ファイル。
        ///		絶対パス。
        /// </summary>
        public VariablePath 曲ファイルの絶対パス { get; protected set; } = null;

        /// <summary>
        ///		この曲ノードに対応する曲ファイルのハッシュ値。
        /// </summary>
        public string 曲ファイルハッシュ { get; protected set; } = null;

        /// <summary>
        ///		この曲ノードに対応する動画ファイル。
        ///		曲ファイルからの相対パス。
        /// </summary>
        public VariablePath 動画ファイルパス { get; protected set; } = null;


        public MusicNode( VariablePath 曲ファイルの絶対パス, Node 親ノード )
        {
            this.親ノード = 親ノード;
            this.曲ファイルの絶対パス = 曲ファイルの絶対パス;

            // （まだ存在してなければ）曲DBに追加する。
            曲DB.曲を追加または更新する( this.曲ファイルの絶対パス, App.ユーザ管理.ログオン中のユーザ );

            // 追加後、改めて曲DBから情報を取得する。
            using( var songdb = new SongDB() )
            {
                var song = songdb.Songs.Where( ( r ) => ( r.Path == this.曲ファイルの絶対パス.変数なしパス ) ).SingleOrDefault();

                if( null == song )
                    return;

                this.タイトル = song.Title;
                this.サブタイトル = "";
                this.サブタイトル = song.Artist;
                this.曲ファイルハッシュ = song.HashId;
                this.難易度[ 3 ] = ("FREE", (float) song.Level);       // [3]:MASTER相当。set.def 内にある MusicNode でも同じ。

                if( song.PreImage.Nullでも空でもない() )
                {
                    var プレビュー画像ファイルの絶対パス = Path.Combine( Path.GetDirectoryName( song.Path ), song.PreImage );
                    this.子Activityを追加する( this.ノード画像 = new テクスチャ( プレビュー画像ファイルの絶対パス ) );
                }

                if( song.PreSound.Nullでも空でもない() )
                    this.プレビュー音声ファイルの絶対パス = Path.Combine( Path.GetDirectoryName( song.Path ), song.PreSound );
            }
        }
    }
}

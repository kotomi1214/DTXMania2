using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;

namespace DTXMania
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
        ///     この曲のBGMの再生タイミングを、この時間[ms]分だけ前後にずらす。（負数で早める、正数で遅める）
        /// </summary>
        public int BGMAdjust { get; set; } = 0;



        // 生成と終了


        public MusicNode( VariablePath 曲ファイルの絶対パス, Node 親ノード )
        {
            this.親ノード = 親ノード;
            this.曲ファイルの絶対パス = 曲ファイルの絶対パス;

            if( !AppForm.ビュアーモードである )
            {
                // （まだ存在してなければ）曲DBに追加する。
                曲DB.曲を追加または更新する( this.曲ファイルの絶対パス, App進行描画.ユーザ管理.ログオン中のユーザ );
            }

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
                this.難易度ラベル = "FREE";
                this.難易度 = (float) song.Level;

                if( song.PreImage.Nullでも空でもない() )
                {
                    var プレビュー画像ファイルの絶対パス = Path.Combine( Path.GetDirectoryName( song.Path ), song.PreImage );
                    this.ノード画像 = new テクスチャ( プレビュー画像ファイルの絶対パス );
                }

                if( song.PreSound.Nullでも空でもない() )
                    this.プレビュー音声ファイルの絶対パス = Path.Combine( Path.GetDirectoryName( song.Path ), song.PreSound );

                this.BGMAdjust = song.BGMAdjust;
            }
        }

        public override void Dispose()
        {
            this.ノード画像?.Dispose();

            base.Dispose();
        }
    }
}

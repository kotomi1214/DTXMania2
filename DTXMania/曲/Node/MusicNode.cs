using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using FDK;
using SSTFormat.v4;

using Song = DTXMania.Song05;
using Record = DTXMania.Record06;

namespace DTXMania
{
    /// <summary>
    ///		曲ツリー階層において「曲」を表すノード。
    /// </summary>
    class MusicNode : Node
    {

        // プロパティ


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



        // プロパティ（最高成績）


        /// <summary>
        ///     これまでの最高達成率。0～100。
        ///     未設定なら null。
        /// </summary>
        public double? 達成率 { get; set; } = null;

        /// <summary>
        ///     これまでの最高ランク。
        ///     未設定なら null。
        /// </summary>
        public ランク種別? ランク => ( this.達成率.HasValue ) ? 成績.ランクを算出する( this.達成率.Value ) : (ランク種別?) null;



        // 現行化ステータス


        public bool 現行化未実施 { get; set; } = true;

        public bool 現行化済み
        {
            get => !this.現行化未実施;
            set => this.現行化未実施 = !value;
        }

        public readonly object 現行化処理の排他 = new object();



        // 生成と終了


        public MusicNode( VariablePath 曲ファイルの絶対パス, SongDB songdb = null, UserDB userdb = null, Node 親ノード = null )
        {
            this.親ノード = 親ノード;
            this.曲ファイルの絶対パス = 曲ファイルの絶対パス;
            this.ノード画像 = Node.現行化前のノード画像;

            // SongDB にレコードがある？
            var song = songdb?.Songs.Where( ( r ) => ( r.Path == this.曲ファイルの絶対パス.変数なしパス ) ).SingleOrDefault();
            if( null != song )
            {
                // (A) あれば、情報を転写する。
                this.タイトル = song.Title;
                this.サブタイトル = song.Artist;
                this.難易度 = (float) song.Level;
                this.難易度ラベル = "FREE";   // 既定値。set.def 内の MusicNode であれば、指定ラベルに上書きすること。
                this.曲ファイルハッシュ = song.HashId;
                this.BGMAdjust = song.BGMAdjust;

                // UserDB.Records にレコードがある？
                var record = userdb?.Records.Where( ( r ) => ( r.UserId == App進行描画.ユーザ管理.ログオン中のユーザ.ユーザID && r.SongHashId == song.HashId ) ).SingleOrDefault();
                if( null != record )
                {
                    // あれば、成績を転写する。
                    this.達成率 = record.Achievement;
                }
            }
            else
            {
                // (B) なければ、新曲である。
                this.タイトル = "(New song!)";
            }
        }

        public override void Dispose()
        {
            this.ノード画像?.Dispose();

            base.Dispose();
        }

        public void 現行化する()
        {
            if( this.現行化済み || AppForm.ビュアーモードである )
                return;

            lock( this.現行化処理の排他 )
            {
                this.現行化済み = true;  // 先に設定

                using( var songdb = new SongDB() )
                {
                    // SongDB へ反映（新規追加 or 更新）する。
                    _SongDBに曲を追加または更新する( songdb, this.曲ファイルの絶対パス, App進行描画.ユーザ管理.ログオン中のユーザ );

                    // そのSongDBレコードを取得。
                    var song = songdb.Songs.Where( ( r ) => ( r.Path == this.曲ファイルの絶対パス.変数なしパス ) ).SingleOrDefault();

                    if( null != song )
                    {
                        // 情報を更新する。
                        this.タイトル = song.Title;
                        this.サブタイトル = song.Artist;
                        this.難易度 = (float) song.Level;
                        this.BGMAdjust = song.BGMAdjust;

                        // ノード画像（プレビュー画像）を生成する。
                        this.ノード画像 = ( song.PreImage.Nullでも空でもない() ) ?
                            new テクスチャ( Path.Combine( Path.GetDirectoryName( song.Path ), song.PreImage ) ) : null;

                        // プレビューサウンドはパスだけ取得しておく。（生成は再生直前に行う。）
                        if( song.PreSound.Nullでも空でもない() )
                            this.プレビュー音声ファイルの絶対パス = Path.Combine( Path.GetDirectoryName( song.Path ), song.PreSound );
                    }
                    else
                    {
                        Log.ERROR( $"SongDBに曲レコードが存在していません。" );
                    }
                }
            }
        }


        
        // private(static)


        /// <summary>
        ///		指定された曲ファイルに対応するレコードがデータベースになければレコードを追加し、
        ///		あればそのレコードを更新する。
        /// </summary>
        private static void _SongDBに曲を追加または更新する( SongDB songdb, VariablePath 曲ファイルの絶対パス, ユーザ設定 ユーザ設定 )
        {
            try
            {
                var 同一パス検索クエリ = songdb.Songs.Where( ( song ) => ( song.Path == 曲ファイルの絶対パス.変数なしパス ) );

                if( 0 == 同一パス検索クエリ.Count() )
                {
                    // (A) 同一パスを持つレコードがDBになかった

                    var 調べる曲のハッシュ = _ファイルのハッシュを算出して返す( 曲ファイルの絶対パス );
                    var 同一ハッシュレコード = songdb.Songs.Where( ( song ) => ( song.HashId == 調べる曲のハッシュ ) ).SingleOrDefault();

                    if( null == 同一ハッシュレコード )
                    {
                        #region " (A-a) 同一ハッシュを持つレコードがDBになかった → 新規追加 "
                        //----------------
                        // スコアを読み込む。
                        var score = スコア.ファイルから生成する( 曲ファイルの絶対パス.変数なしパス );

                        // SongDB にレコードを新規追加する。
                        var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
                        var BPMs = _最小最大BPMを調べて返す( score );

                        songdb.Songs.InsertOnSubmit(
                            new Song() {
                                HashId = _ファイルのハッシュを算出して返す( 曲ファイルの絶対パス ),
                                Title = score.曲名,
                                Path = 曲ファイルの絶対パス.変数なしパス,
                                LastWriteTime = File.GetLastWriteTime( 曲ファイルの絶対パス.変数なしパス ).ToString( "G" ),
                                Level = score.難易度,
                                MinBPM = BPMs.最小BPM,
                                MaxBPM = BPMs.最大BPM,
                                TotalNotes_LeftCymbal = ノーツ数[ 表示レーン種別.LeftCymbal ],
                                TotalNotes_HiHat = ノーツ数[ 表示レーン種別.HiHat ],
                                TotalNotes_LeftPedal = ノーツ数[ 表示レーン種別.Foot ],
                                TotalNotes_Snare = ノーツ数[ 表示レーン種別.Snare ],
                                TotalNotes_Bass = ノーツ数[ 表示レーン種別.Bass ],
                                TotalNotes_HighTom = ノーツ数[ 表示レーン種別.Tom1 ],
                                TotalNotes_LowTom = ノーツ数[ 表示レーン種別.Tom2 ],
                                TotalNotes_FloorTom = ノーツ数[ 表示レーン種別.Tom3 ],
                                TotalNotes_RightCymbal = ノーツ数[ 表示レーン種別.RightCymbal ],
                                PreImage = ( score.プレビュー画像ファイル名.Nullでも空でもない() ) ? score.プレビュー画像ファイル名 : "",
                                Artist = score.アーティスト名,
                                PreSound = ( score.プレビュー音声ファイル名.Nullでも空でもない() ) ? score.プレビュー音声ファイル名 : "",
                                BGMAdjust = 0,
                            } );

                        songdb.DataContext.SubmitChanges();

                        Log.Info( $"DBに曲を追加しました。{曲ファイルの絶対パス.変数付きパス}" );
                        //----------------
                        #endregion
                    }
                    else
                    {
                        #region " (A-b) 同一ハッシュを持つレコードがDBにあった → 更新 "
                        //----------------
                        var song = 同一ハッシュレコード;

                        // スコアを読み込む。
                        var score = スコア.ファイルから生成する( 曲ファイルの絶対パス.変数なしパス );

                        // SongDB のレコードを更新する。
                        var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
                        var BPMs = _最小最大BPMを調べて返す( score );

                        song.Title = score.曲名;
                        song.Path = 曲ファイルの絶対パス.変数なしパス;
                        song.LastWriteTime = File.GetLastWriteTime( 曲ファイルの絶対パス.変数なしパス ).ToString( "G" );
                        song.Level = score.難易度;
                        song.MinBPM = BPMs.最小BPM;
                        song.MaxBPM = BPMs.最大BPM;
                        song.TotalNotes_LeftCymbal = ノーツ数[ 表示レーン種別.LeftCymbal ];
                        song.TotalNotes_HiHat = ノーツ数[ 表示レーン種別.HiHat ];
                        song.TotalNotes_LeftPedal = ノーツ数[ 表示レーン種別.Foot ];
                        song.TotalNotes_Snare = ノーツ数[ 表示レーン種別.Snare ];
                        song.TotalNotes_Bass = ノーツ数[ 表示レーン種別.Bass ];
                        song.TotalNotes_HighTom = ノーツ数[ 表示レーン種別.Tom1 ];
                        song.TotalNotes_LowTom = ノーツ数[ 表示レーン種別.Tom2 ];
                        song.TotalNotes_FloorTom = ノーツ数[ 表示レーン種別.Tom3 ];
                        song.TotalNotes_RightCymbal = ノーツ数[ 表示レーン種別.RightCymbal ];
                        song.PreImage = ( score.プレビュー画像ファイル名.Nullでも空でもない() ) ? score.プレビュー画像ファイル名 : "";
                        song.Artist = score.アーティスト名;
                        song.PreSound = ( score.プレビュー音声ファイル名.Nullでも空でもない() ) ? score.プレビュー音声ファイル名 : "";
                        song.BGMAdjust = song.BGMAdjust;

                        songdb.DataContext.SubmitChanges();

                        Log.Info( $"パスが異なりハッシュが同一であるレコードが検出されたため、曲の情報を更新しました。{曲ファイルの絶対パス.変数付きパス}" );
                        //----------------
                        #endregion
                    }
                }
                else
                {
                    // (B) 同一パスを持つレコードがDBにあった

                    var record = 同一パス検索クエリ.Single();

                    string レコードの最終更新日時 = record.LastWriteTime;
                    string 調べる曲の最終更新日時 = File.GetLastWriteTime( 曲ファイルの絶対パス.変数なしパス ).ToString( "G" );

                    if( レコードの最終更新日時 != 調べる曲の最終更新日時 )
                    {
                        #region " (B-a) 最終更新日時が変更されている → 更新 "
                        //----------------
                        // スコアを読み込む。
                        var score = スコア.ファイルから生成する( 曲ファイルの絶対パス.変数なしパス );
                        var hash = _ファイルのハッシュを算出して返す( 曲ファイルの絶対パス );
                        var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
                        var BPMs = _最小最大BPMを調べて返す( score );

                        // HashId 以外のカラムを更新する。
                        record.Title = score.曲名;
                        record.LastWriteTime = 調べる曲の最終更新日時;
                        record.Level = score.難易度;
                        record.MinBPM = BPMs.最小BPM;
                        record.MaxBPM = BPMs.最大BPM;
                        record.TotalNotes_LeftCymbal = ノーツ数[ 表示レーン種別.LeftCymbal ];
                        record.TotalNotes_HiHat = ノーツ数[ 表示レーン種別.HiHat ];
                        record.TotalNotes_LeftPedal = ノーツ数[ 表示レーン種別.Foot ];
                        record.TotalNotes_Snare = ノーツ数[ 表示レーン種別.Snare ];
                        record.TotalNotes_Bass = ノーツ数[ 表示レーン種別.Bass ];
                        record.TotalNotes_HighTom = ノーツ数[ 表示レーン種別.Tom1 ];
                        record.TotalNotes_LowTom = ノーツ数[ 表示レーン種別.Tom2 ];
                        record.TotalNotes_FloorTom = ノーツ数[ 表示レーン種別.Tom3 ];
                        record.TotalNotes_RightCymbal = ノーツ数[ 表示レーン種別.RightCymbal ];
                        record.PreImage = ( score.プレビュー画像ファイル名.Nullでも空でもない() ) ? score.プレビュー画像ファイル名 : "";
                        record.Artist = score.アーティスト名;
                        record.PreSound = ( score.プレビュー音声ファイル名.Nullでも空でもない() ) ? score.プレビュー音声ファイル名 : "";
                        record.BGMAdjust = record.BGMAdjust;

                        if( hash != record.HashId )
                        {
                            // ハッシュはキーなので、これが変わったら、古いレコードを削除して、新しいレコードを追加する。
                            var newRecord = record.Clone();
                            songdb.Songs.DeleteOnSubmit( record );
                            songdb.DataContext.SubmitChanges(); // 一度Submitして先にレコード削除を確定しないと、次のInsertがエラーになる。（PathカラムはUnique属性なので）

                            newRecord.HashId = hash;
                            songdb.Songs.InsertOnSubmit( newRecord );
                        }

                        songdb.DataContext.SubmitChanges();

                        Log.Info( $"最終更新日時が変更されているため、曲の情報を更新しました。{曲ファイルの絶対パス.変数付きパス}" );
                        //----------------
                        #endregion
                    }
                    else
                    {
                        #region " (B-b) それ以外 → 何もしない "
                        //----------------
                        //----------------
                        #endregion
                    }
                }
            }
            catch( Exception e )
            {
                Log.ERROR( $"曲DBへの曲の追加に失敗しました。({VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )})[{曲ファイルの絶対パス.変数付きパス}]" );
                //throw;
            }
        }

        private static string _ファイルのハッシュを算出して返す( VariablePath 曲ファイルの絶対パス )
        {
            var sha512 = new SHA512CryptoServiceProvider();
            byte[] hash = null;

            try
            {
                using( var fs = new FileStream( 曲ファイルの絶対パス.変数なしパス, FileMode.Open, FileAccess.Read ) )
                    hash = sha512.ComputeHash( fs );

                var hashString = new StringBuilder();
                foreach( byte b in hash )
                    hashString.Append( b.ToString( "X2" ) );

                return hashString.ToString();
            }
            catch( Exception e )
            {
                Log.ERROR( $"ファイルからのハッシュの作成に失敗しました。({e})[{曲ファイルの絶対パス.変数付きパス}]" );
                throw;
            }
        }

        private static Dictionary<表示レーン種別, int> _ノーツ数を算出して返す( スコア score, ユーザ設定 ユーザ設定 )
        {
            var ノーツ数 = new Dictionary<表示レーン種別, int>();

            foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                ノーツ数.Add( lane, 0 );

            foreach( var chip in score.チップリスト )
            {
                var ドラムチッププロパティ = ユーザ設定.ドラムチッププロパティ管理[ chip.チップ種別 ];

                // AutoPlay ON のチップは、すべてがONである場合を除いて、カウントしない。
                if( ユーザ設定.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ] )
                {
                    if( !( ユーザ設定.AutoPlayがすべてONである ) )
                        continue;
                }
                // AutoPlay OFF 時でもユーザヒットの対象にならないチップはカウントしない。
                if( !( ドラムチッププロパティ.AutoPlayOFF_ユーザヒット ) )
                    continue;

                ノーツ数[ ドラムチッププロパティ.表示レーン種別 ]++;
            }

            return ノーツ数;
        }

        private static (double 最小BPM, double 最大BPM) _最小最大BPMを調べて返す( スコア score )
        {
            var result = (最小BPM: double.MaxValue, 最大BPM: double.MinValue);

            var BPMchips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.BPM ) );
            foreach( var chip in BPMchips )
            {
                result.最小BPM = Math.Min( result.最小BPM, chip.BPM );
                result.最大BPM = Math.Max( result.最大BPM, chip.BPM );
            }

            if( result.最小BPM == double.MaxValue || result.最大BPM == double.MinValue )    // BPMチップがひとつもなかった
            {
                double 初期BPM = スコア.初期BPM;
                result = (初期BPM, 初期BPM);
            }

            return result;
        }
    }
}

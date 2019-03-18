using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK32;
using SSTFormatCurrent = SSTFormat.v4;

namespace DTXMania.データベース.曲
{
    using Song01 = old.Song01;
    using Song02 = old.Song02;
    using Song03 = old.Song03;
    using Song04 = old.Song04;
    using Song = Song05;    // 最新バージョンを指定(1/2)。

    /// <summary>
    ///		曲データベースに対応するエンティティクラス。
    /// </summary>
    class SongDB : SQLiteDBBase
    {
        public const long VERSION = 5;  // 最新バージョンを指定(2/2)。

        public static readonly VariablePath 曲DBファイルパス = @"$(AppData)SongDB.sqlite3";

        public Table<Song> Songs
            => base.DataContext.GetTable<Song>();


        public SongDB()
        {
            try
            {
                this.Open( 曲DBファイルパス, VERSION );
            }
            catch( Exception e )
            {
                Log.WARNING( $"エラーが発生したので、新しく作り直します。[{e.Message}]" );
                App.曲データベースを初期化する();
                this.Open( 曲DBファイルパス, VERSION );
            }
        }

        protected override void テーブルがなければ作成する()
        {
            using( var transaction = this.Connection.BeginTransaction() )
            {
                try
                {
                    // 最新のバージョンのテーブルを作成する。
                    this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Songs {Song.ColumnsList};" );
                    this.DataContext.SubmitChanges();

                    // 成功。
                    transaction.Commit();
                }
                catch
                {
                    // 失敗。
                    transaction.Rollback();
                }
            }
        }

        protected override void データベースのアップグレードマイグレーションを行う( long 移行元DBバージョン )
        {
            switch( 移行元DBバージョン )
            {
                case 1:
                    #region " 1 → 2 "
                    //----------------
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( $"CREATE TABLE new_Songs {Song02.ColumnsList}" );
                            this.DataContext.ExecuteCommand( $"INSERT INTO new_Songs SELECT *,null FROM Songs" );   // 追加されたカラムは null
                            this.DataContext.ExecuteCommand( $"DROP TABLE Songs" );
                            this.DataContext.ExecuteCommand( $"ALTER TABLE new_Songs RENAME TO Songs" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // すべてのレコードについて、追加されたカラムを更新する。
                            var song02s = base.DataContext.GetTable<Song02>();
                            foreach( var song02 in song02s )
                            {
                                var score = (SSTFormatCurrent.スコア) null;
                                var vpath = new VariablePath( song02.Path );

                                // スコアを読み込む
                                score = SSTFormatCurrent.スコア.ファイルから生成する( vpath.変数なしパス );

                                // PreImage カラムの更新。
                                if( score.プレビュー画像ファイル名.Nullでも空でもない() )
                                {
                                    // プレビュー画像は、曲ファイルからの相対パス。
                                    song02.PreImage = Path.Combine( Path.GetDirectoryName( vpath.変数なしパス ), score.プレビュー画像ファイル名 );
                                }
                                else
                                {
                                    song02.PreImage =
                                        ( from ファイル名 in Directory.GetFiles( Path.GetDirectoryName( vpath.変数なしパス ) )
                                          where _対応するサムネイル画像名.Any( thumbファイル名 => ( Path.GetFileName( ファイル名 ).ToLower() == thumbファイル名 ) )
                                          select ファイル名 ).FirstOrDefault();
                                }
                            }
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( "Songs テーブルをアップデートしました。[1→2]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            Log.ERROR( "Songs テーブルのアップデートに失敗しました。[1→2]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 2:
                    #region " 2 → 3 "
                    //----------------
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( $"CREATE TABLE new_Songs {Song03.ColumnsList}" );
                            this.DataContext.ExecuteCommand( $"INSERT INTO new_Songs SELECT *,null FROM Songs" );   // 追加されたカラムは null
                            this.DataContext.ExecuteCommand( $"DROP TABLE Songs" );
                            this.DataContext.ExecuteCommand( $"ALTER TABLE new_Songs RENAME TO Songs" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // すべてのレコードについて、追加されたカラムを更新する。
                            var song03s = base.DataContext.GetTable<Song03>();
                            foreach( var song03 in song03s )
                            {
                                var score = (SSTFormatCurrent.スコア) null;
                                var vpath = new VariablePath( song03.Path );

                                // スコアを読み込む 
                                score = SSTFormatCurrent.スコア.ファイルから生成する( vpath.変数なしパス );

                                // Artist カラムの更新。
                                if( score.アーティスト名.Nullでも空でもない() )
                                {
                                    song03.Artist = score.アーティスト名;
                                }
                                else
                                {
                                    song03.Artist = ""; // null不可
                                }
                            }
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( "Songs テーブルをアップデートしました。[2→3]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            Log.ERROR( "Songs テーブルのアップデートに失敗しました。[2→3]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 3:
                    #region " 3 → 4 "
                    //----------------
                    // 変更点：
                    // ・PreImage カラムの内容を、絶対パスから、曲譜面ファイルからの相対パスに変更。
                    // ・PreSound カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // (1) データベースにカラム PreSound を追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Songs ADD COLUMN PreSound NVARCHAR" );
                            this.DataContext.SubmitChanges();

                            // (2) すべてのレコードについて、変更のあったカラムを更新する。
                            var songs = this.DataContext.GetTable<Song04>();
                            foreach( var song in songs )
                            {
                                // スコアを読み込む 
                                var score = (SSTFormatCurrent.スコア) null;
                                var vpath = new VariablePath( song.Path );
                                score = SSTFormatCurrent.スコア.ファイルから生成する( vpath.変数なしパス, ヘッダだけ: true );

                                // PreImage フィールドを更新
                                song.PreImage = score.プレビュー画像ファイル名;
                                if( song.PreImage.Nullでも空でもない() && Path.IsPathRooted( song.PreImage ) )
                                    song.PreImage = FDK32.Folder.絶対パスを相対パスに変換する( Path.GetDirectoryName( song.Path ), song.PreImage );

                                // PreSound フィールドを更新。
                                song.PreSound = score.プレビュー音声ファイル名;
                                if( song.PreSound.Nullでも空でもない() && Path.IsPathRooted( song.PreSound ) )
                                    song.PreSound = FDK32.Folder.絶対パスを相対パスに変換する( Path.GetDirectoryName( song.Path ), song.PreSound );
                            }
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( $"Songs テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン+1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            Log.ERROR( $"Songs テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン+1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 4:
                    #region " 4 → 5 "
                    //----------------
                    // 変更点：
                    // ・BGMAdjust カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラム BGMAdjust を追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Songs ADD COLUMN BGMAdjust INTEGER NOT NULL DEFAILT 0" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( $"Songs テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            Log.ERROR( $"Songs テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                default:
                    throw new Exception( $"移行元DBのバージョン({移行元DBバージョン})がマイグレーションに未対応です。" );
            }
        }


        private readonly string[] _対応するサムネイル画像名 = { "thumb.png", "thumb.bmp", "thumb.jpg", "thumb.jpeg" };
    }
}

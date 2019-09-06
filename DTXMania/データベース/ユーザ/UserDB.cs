using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;

namespace DTXMania
{
    using User = User12;        // 最新バージョンを指定（１／２）
    using rRecord06 = データベース.成績.old.Record06;
    using rUser02 = データベース.ユーザ.old.User02;
    using rUser12 = User12;

    /// <summary>
    ///		ユーザデータベースに対応するエンティティクラス。
    /// </summary>
    class UserDB : SQLiteDBBase
    {
        public const long VERSION = 12;  // 最新バージョンを指定（２／２）

        public static readonly VariablePath DBファイルパス = @"$(AppData)UserDB.sqlite3";

        public Table<User> Users => base.DataContext.GetTable<User>();

        public UserDB()
        {
            try
            {
                this.Open( DBファイルパス, VERSION );
            }
            catch( Exception e )
            {
                Log.WARNING( $"エラーが発生しました。新しく作り直します。[{e.Message}]" );

                #region " DBファイルを削除 "
                //----------------
                try
                {
                    File.Delete( DBファイルパス.変数なしパス );  // ファイルがない場合には例外は出ない
                }
                catch( Exception e2 )
                {
                    var msg = $"ユーザデータベースファイルの削除に失敗しました。[{DBファイルパス.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]";
                    Log.ERROR( msg );
                    throw new Exception( msg, e2 );  // どうしようもないので例外発出
                }
                //----------------
                #endregion

                this.Open( DBファイルパス, VERSION );
            }
        }


        protected override void テーブルがなければ作成する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                using( var transaction = this.Connection.BeginTransaction() )
                {
                    try
                    {
                        // 最新のバージョンのテーブルを作成する。
                        this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Users {User.ColumnList};" );
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
        }

        protected override void データベースのアップグレードマイグレーションを行う( long 移行元DBバージョン )
        {
            switch( 移行元DBバージョン )
            {
                case 1:
                    #region " 1 → 2 "
                    //----------------
                    // 変更点:
                    // ・SongFolders カラムを削除。
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( $"CREATE TABLE new_Users {rUser02.ColumnsList}" );
                            this.DataContext.ExecuteCommand( "INSERT INTO new_Users SELECT Id,Name,ScrollSpeed,Fullscreen,AutoPlay_LeftCymbal,AutoPlay_HiHat,AutoPlay_LeftPedal,AutoPlay_Snare,AutoPlay_Bass,AutoPlay_HighTom,AutoPlay_LowTom,AutoPlay_FloorTom,AutoPlay_RightCymbal,MaxRange_Perfect,MaxRange_Great,MaxRange_Good,MaxRange_Ok,CymbalFree FROM Users" );
                            this.DataContext.ExecuteCommand( "DROP TABLE Users" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE new_Users RENAME TO Users" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( "Users テーブルをアップデートしました。[1→2]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( "Users テーブルのアップデートに失敗しました。[1→2]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 2:
                    #region " 2 → 3 "
                    //----------------
                    // 変更点:
                    // ・PlayMode カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラム PlayMode を追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN PlayMode INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( "Users テーブルをアップデートしました。[2→3]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( "Users テーブルのアップデートに失敗しました。[2→3]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 3:
                    #region " 3 → 4 "
                    //----------------
                    // 変更点:
                    // ・RideLeft, ChinaLeft, SplashLeft カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースに新しいカラムを追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN RideLeft INTEGER NOT NULL DEFAULT 0" );  // 2018.2.11 現在、SQLite で複数カラムを一度に追加できる構文はない。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN ChinaLeft INTEGER NOT NULL DEFAULT 0" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN SplashLeft INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( "Users テーブルをアップデートしました。[3→4]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( "Users テーブルのアップデートに失敗しました。[3→4]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 4:
                    #region " 4 → 5 "
                    //----------------
                    // 変更点:
                    // ・DrumSound カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラム DrumSound を追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN DrumSound INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 5:
                    #region " 5 → 6 "
                    //----------------
                    // 変更点:
                    // ・LaneType カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラム LaneType を追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN LaneType NVARCHAR NOT NULL DEFAULT 'TypeA'" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 6:
                    #region " 6 → 7 "
                    //----------------
                    // 変更点:
                    // ・LaneTrans カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラム LaneTrans を追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN LaneTrans INTEGER NOT NULL DEFAULT 50" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 7:
                    #region " 7 → 8 "
                    //----------------
                    // 変更点:
                    // ・BackgroundMovie カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラム BackgroundMovie を追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN BackgroundMovie INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 8:
                    #region " 8 → 9 "
                    //----------------
                    // 変更点:
                    // ・PlaySpeed, ShowPartLine, ShorPartNumber を追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラムを追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN PlaySpeed READ NOT NULL DEFAULT 1.0" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN ShowPartLine INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN ShowPartNumber INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 9:
                    #region " 9 → 10 "
                    //----------------
                    // 変更点:
                    // ・ShorScoreWall, BackgroundMovieSize を追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラムを追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN ShowScoreWall INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN BackgroundMovieSize INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 10:
                    #region " 10 → 11 "
                    //----------------
                    // 変更点:
                    // ・ShowFastSlow を追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // データベースにカラムを追加する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN ShowFastSlow INTEGER NOT NULL DEFAULT 0" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 11:
                    #region " 11 →12 "
                    //----------------
                    // 変更点：
                    // ・UserDB.Records テーブルを廃止し、新規に作成する RecordDB.Records テーブルに移行。
                    // ・PlayMode カラムを削除。
                    this.DataContext.SubmitChanges();

                    #region " RecordDB を新設し、UserDB11.Records の内容をコピーする。"
                    //----------------
                    using( var recorddb = new RecordDB() )
                    using( var transaction = recorddb.Connection.BeginTransaction() )
                    {
                        try
                        {
                            foreach( var src in this.DataContext.GetTable<rRecord06>().ToArray() )
                            {
                                recorddb.Records.InsertOnSubmit(
                                    new Record07() {
                                        UserId = src.UserId,
                                        SongHashId = src.SongHashId,
                                        Score = src.Score,
                                        CountMap = src.CountMap,
                                        //Skill = src.Skill,        Record07 で Skill カラムは廃止
                                        Achievement = src.Achievement,
                                    } );
                            }

                            // 成功。
                            transaction.Commit();
                            recorddb.DataContext.SubmitChanges();
                            Log.Info( "RecordDB を新設し、データを移行しました。" );
                        }
                        catch( Exception e )
                        {
                            // 失敗
                            transaction.Rollback();
                            throw new Exception( "RecordDB の新設とデータ移行に失敗しました。", e );
                        }
                    }
                    //----------------
                    #endregion

                    #region " UserDB.Records テーブルを削除する。"
                    //----------------
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            this.DataContext.ExecuteCommand( "DROP TABLE Records" );

                            // 成功。
                            transaction.Commit();
                            Log.Info( "UserDB.Records テーブルの削除に成功しました。" );
                        }
                        catch( Exception e )
                        {
                            transaction.Rollback();
                            throw new Exception( "UserDB.Records テーブルの削除に失敗しました。", e );
                        }
                    }
                    //----------------
                    #endregion

                    #region " PlayMode カラムを削除する。"
                    //----------------
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( $"CREATE TABLE new_Users {rUser12.ColumnList}" );
                            this.DataContext.ExecuteCommand( "INSERT INTO new_Users SELECT Id,Name,ScrollSpeed,Fullscreen,AutoPlay_LeftCymbal,AutoPlay_HiHat,AutoPlay_LeftPedal,AutoPlay_Snare,AutoPlay_Bass,AutoPlay_HighTom,AutoPlay_LowTom,AutoPlay_FloorTom,AutoPlay_RightCymbal,MaxRange_Perfect,MaxRange_Great,MaxRange_Good,MaxRange_Ok,CymbalFree,RideLeft,ChinaLeft,SplashLeft,DrumSound,LaneType,LaneTrans,BackgroundMovie,PlaySpeed,ShowPartLine,ShowPartNumber,ShowScoreWall,BackgroundMovieSize,ShowFastSlow FROM Users" );
                            this.DataContext.ExecuteCommand( "DROP TABLE Users" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE new_Users RENAME TO Users" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( "Users テーブルをアップデートしました。[1→2]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( "Users テーブルのアップデートに失敗しました。[1→2]" );
                        }
                    }
                    //----------------
                    #endregion

                    //----------------
                    #endregion
                    break;

                default:
                    throw new Exception( $"移行元DBのバージョン({移行元DBバージョン})がマイグレーションに未対応です。" );
            }
        }
    }
}

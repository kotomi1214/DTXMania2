using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;

namespace DTXMania
{
    using Record = Record08;    // 最新バージョンを指定（１／２）
    using rRecord08 = Record08;

    /// <summary>
    ///		成績データベースに対応するエンティティクラス。
    /// </summary>
    class RecordDB : SQLiteDBBase
    {
        public const long VERSION = 8;  // 最新バージョンを指定（２／２）

        public static readonly VariablePath DBファイルパス = @"$(AppData)RecordDB.sqlite3";

        public Table<Record> Records => base.DataContext.GetTable<Record>();


        public RecordDB()
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
                    var msg = $"成績データベースファイルの削除に失敗しました。[{DBファイルパス.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]";
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
                        this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Records {Record.ColumnList};" );
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
                case 1: // 1 → 2
                case 2: // 2 → 3
                case 3: // 3 → 4
                case 4: // 4 → 5
                case 5: // 5 → 6
                    break;  // 変更なし

                case 6:
                    #region " 6 → 7 "
                    //----------------
                    // 変更点：
                    // ・Skill カラムを削除。

                    // UserDB 側で RecordDB を作成する際に Record07 で生成されるので、ここでは何もしない。
                    
                    //----------------
                    #endregion
                    break;

                case 7:
                    #region " 7 → 8 "
                    //----------------
                    // 変更点：
                    // ・REAL 型を NUMERIC 型に変更。
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テーブルを削除して、空のテーブルで作り直す。
                            this.DataContext.ExecuteCommand( "DROP TABLE Records" );
                            this.DataContext.ExecuteCommand( $"CREATE TABLE Records {rRecord08.ColumnList}" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( $"Records テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Records テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                default:
                    throw new Exception( $"移行元DBのバージョン({移行元DBバージョン})がマイグレーションに未対応です。" );
            }
        }
    }
}

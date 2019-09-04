using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Record = DTXMania.Record07;

namespace DTXMania
{
    /// <summary>
    ///		曲データベースを管理するstaticクラス。
    /// </summary>
    static class 曲DB
    {
        /// <summary>
        ///		指定したユーザID＆曲ファイルハッシュに対応するレコードがデータベースになければレコードを追加し、
        ///		あればそのレコードを（最高記録であれば）更新する。
        /// </summary>
        public static void 成績を追加または更新する( 成績 今回の成績, string ユーザID, string 曲ファイルハッシュ )
        {
            using( var recorddb = new RecordDB() )
            {
                var record = recorddb.Records.Where( ( r ) => ( r.UserId == ユーザID && r.SongHashId == 曲ファイルハッシュ ) ).SingleOrDefault();
                if( null == record )
                {
                    // (A) レコードが存在しないので、追加する。
                    recorddb.Records.InsertOnSubmit( new Record() {
                        UserId = ユーザID,
                        SongHashId = 曲ファイルハッシュ,
                        Score = 今回の成績.Score,
                        // TODO: CountMap を成績クラスに保存する。
                        CountMap = "",
                        Skill = 今回の成績.Skill,
                        Achievement = 今回の成績.Achievement,
                    } );
                }
                else
                {
                    // (B) レコードがすでに存在するので、更新する。（記録更新したレコードのみ）

                    if( record.Achievement < 今回の成績.Achievement ||
                        record.Skill < 今回の成績.Skill )
                    {
                        record.Score = 今回の成績.Score;
                        record.Skill = 今回の成績.Skill;
                        record.Achievement = 今回の成績.Achievement;

                        // TODO: CountMap を成績クラスに保存する。
                    }
                }

                recorddb.DataContext.SubmitChanges();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;

namespace DTXMania
{
    /// <summary>
    ///		成績テーブルのエンティティクラス。
    /// </summary>
    [Table( Name = "Records" )]   // テーブル名は複数形
    class Record08 : ICloneable
    {
        /// <summary>
        ///		ユーザを一意に識別するID。[key1/2]
        /// </summary>
        [Column( DbType = "NVARCHAR", CanBeNull = false, IsPrimaryKey = true )]
        public string UserId { get; set; }

        /// <summary>
        ///		曲譜面ファイルのハッシュ値。[key2/2]
        /// </summary>
        [Column( DbType = "NVARCHAR", CanBeNull = false, IsPrimaryKey = true )]
        public string SongHashId { get; set; }

        /// <summary>
        ///		スコア。
        /// </summary>
        [Column( DbType = "INT", CanBeNull = false )]
        public int Score { get; set; }

        /// <summary>
        ///		カウントマップラインのデータ。
        ///		１ブロックを１文字（'0':0～'C':12）で表し、<see cref="DTXMania.演奏.クリアメーター.カウントマップの最大要素数"/> 個の文字が並ぶ。
        ///		もし不足分があれば、'0' とみなされる。
        /// </summary>
        [Column( DbType = "NVARCHAR", CanBeNull = false )]
        public string CountMap { get; set; }

        /// <summary>
        ///		達成率[%]。0～100。
        /// </summary>
        [Column( DbType = "NUMERIC", CanBeNull = false )]
        public double Achievement { get; set; }

        ///////////////////////////

        /// <summary>
        ///		規定値で初期化。
        /// </summary>
        public Record08()
        {
            this.UserId = "Anonymous";
            this.SongHashId = "";
            this.Score = 0;
            this.CountMap = "";
            this.Achievement = 0.0;
        }

        // ICloneable 実装
        public Record08 Clone()
        {
            return (Record08) this.MemberwiseClone();
        }
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        ///////////////////////////

        /// <summary>
        ///		テーブルのカラム部分を列挙したSQL。
        /// </summary>
        public static readonly string ColumnList =
            @"( UserId NVARCHAR NOT NULL" +
            @", SongHashId NVARCHAR NOT NULL" +
            @", Score INTEGER NOT NULL" +
            @", CountMap NVARCHAR NOT NULL" +
            @", Achievement NUMERIC NOT NULL" +
            @", PRIMARY KEY(`UserId`,`SongHashId`)" +
            @")";
    }
}

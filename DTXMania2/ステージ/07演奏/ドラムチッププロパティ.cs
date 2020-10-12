using System;
using System.Collections.Generic;
using System.Diagnostics;
using SSTF=SSTFormat.v004;

namespace DTXMania2.演奏
{
    /// <summary>
    ///     譜面のチップ（<see cref="チップ種別"/>）をキーとして、様々なコンフィグプロパティを定義する。
    /// </summary>
    class ドラムチッププロパティ
    {
        // 主キー; SSTFにおけるチップの種別と、チップが属するレーンの種別。

        public SSTF.チップ種別 チップ種別 { get; set; }
        public SSTF.レーン種別 レーン種別 { get; set; }

        // 表示

        /// <summary>
        ///     チップが表示される画面上のレーン。
        ///     オプション設定により変更可能な場合がある。
        /// </summary>
        public 表示レーン種別 表示レーン種別 { get; set; }
        /// <summary>
        ///     チップを描画形態（見た目）で分類したチップ種別。
        /// </summary>
        public 表示チップ種別 表示チップ種別 { get; set; }

        // 入力

        /// <summary>
        ///     チップにヒット可能な入力の種別。
        /// </summary>
        /// <remarks>
        ///     チップと <see cref="ドラム入力種別"/> は N : 1 である。
        ///     これは、1つのドラム入力がヒット可能なチップが複数存在する場合があることを意味する。
        ///     例えば、<see cref="ドラム入力種別.Ride"/> は、Ride チップと Ride_Cup チップをヒットすることができる。
        /// </remarks>
        public ドラム入力種別 ドラム入力種別 { get; set; }
        /// <summary>
        ///     チップが属する AutoPlay種別。
        /// </summary>
        /// <remarks>
        ///     AutoPlay は、（チップ単位でもレーン単位でも入力単位でもなく）<see cref="AutoPlay種別"/> 単位で ON / OFF される。
        ///     チップの属する <see cref="AutoPlay種別"/> が ON である場合、このチップの AutoPlay は ON である。OFF も同様。
        /// </remarks>
        public AutoPlay種別 AutoPlay種別 { get; set; }
        /// <summary>
        ///     チップの属する入力グループ種別。
        /// </summary>
        /// <remarks>
        ///     同じ <see cref="入力グループ種別"/> に属するチップは、各チップの <see cref="ドラム入力種別"/> の
        ///     いずれかでヒットすることができる。
        ///     例えば、Ride チップと HHClose が同じ入力グループ種別に属している場合、
        ///     Ride 入力で HHClose チップをヒットすることができる。
        /// </remarks>
        public 入力グループ種別 入力グループ種別 { get; set; }

        // ヒット

        /// <summary>
        ///     このチップのサウンドの再生時に、同じ<see cref="消音グループ種別"/>に属するチップのサウンドの
        ///     消音が必要であるかどうかを示す。
        /// </summary>
        public bool 発声前消音 { get; set; }
        /// <summary>
        ///     チップのサウンドが属する消音グループ。
        ///     どこにも属さないなら <see cref="消音グループ種別.Unknown"/>。
        /// </summary>
        /// <remarks>
        ///     同じ<see cref="消音グループ種別"/>に属するチップのサウンドは、同時に1つしか再生することができない。
        /// </remarks>
        public 消音グループ種別 消音グループ種別 { get; set; }

        /// <summary>
        ///     チップが AutoPlay ON のとき、ヒット判定バー上で何らかのヒット処理が自動で行われる場合は true。
        /// </summary>
        public bool AutoPlayON_自動ヒット => ( this.AutoPlayON_自動ヒット_再生 || this.AutoPlayON_自動ヒット_非表示 || this.AutoPlayON_自動ヒット_判定 );
        /// <summary>
        ///     チップが AutoPlay ON のとき、ヒット判定バー上で再生処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayON_自動ヒット_再生 { get; set; }
        /// <summary>
        ///     チップが AutoPlay ON のとき、ヒット判定バー上でチップが非表示になる場合は true。
        /// </summary>
        public bool AutoPlayON_自動ヒット_非表示 { get; set; }
        /// <summary>
        ///     チップが AutoPlay ON のとき、ヒット判定バー上でヒット判定処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayON_自動ヒット_判定 { get; set; }
        /// <summary>
        ///     チップが AutoPlay ON のとき、ヒット判定バー通過後にMISS判定処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayON_Miss判定 { get; set; }

        /// <summary>
        ///     チップが AutoPlay OFF のとき、ヒット判定バー上で何らかのヒット処理が自動で行われる場合は true。
        /// </summary>
        public bool AutoPlayOFF_自動ヒット => ( this.AutoPlayOFF_自動ヒット_再生 || this.AutoPlayOFF_自動ヒット_非表示 || this.AutoPlayOFF_自動ヒット_判定 );
        /// <summary>
        ///     チップが AutoPlay OFF のとき、ヒット判定バー上で再生処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayOFF_自動ヒット_再生 { get; set; }
        /// <summary>
        ///     チップが AutoPlay OFF のとき、ヒット判定バー上でチップが非表示になる場合は true。
        /// </summary>
        public bool AutoPlayOFF_自動ヒット_非表示 { get; set; }
        /// <summary>
        ///     チップが AutoPlay OFF のとき、ヒット判定バー上でヒット判定処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayOFF_自動ヒット_判定 { get; set; }

        /// <summary>
        ///     チップが AutoPlay OFF かつユーザの入力がヒットした時に、何らかの処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayOFF_ユーザヒット => ( this.AutoPlayOFF_ユーザヒット_再生 || this.AutoPlayOFF_ユーザヒット_非表示 || this.AutoPlayOFF_ユーザヒット_判定 );
        /// <summary>
        ///     チップが AutoPlay OFF かつユーザの入力がヒットした時に、再生処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayOFF_ユーザヒット_再生 { get; set; }
        /// <summary>
        ///     チップが AutoPlay OFF かつユーザの入力がヒットした時に、チップが非表示になる場合は true。
        /// </summary>
        public bool AutoPlayOFF_ユーザヒット_非表示 { get; set; }
        /// <summary>
        ///     チップが AutoPlay OFF かつユーザの入力がヒットした時に、ヒット判定処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayOFF_ユーザヒット_判定 { get; set; }
        /// <summary>
        ///     チップが AutoPlay OFF のとき、ヒット判定バー通過後にMISS判定処理が行われる場合は true。
        /// </summary>
        public bool AutoPlayOFF_Miss判定 { get; set; }
    }
}

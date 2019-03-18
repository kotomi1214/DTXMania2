using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXMania.設定
{
    enum システムサウンド種別
    {
        カーソル移動音,
        決定音,
        取消音,
        変更音,

        ステージ失敗,
        ステージクリア,
        フルコンボ,
        歓声,

        起動ステージ_開始音,
        起動ステージ_ループBGM,

        タイトルステージ_開始音,
        タイトルステージ_ループBGM,
        タイトルステージ_確定音,

        認証ステージ_開始音,
        認証ステージ_ループBGM,
        認証ステージ_ログイン音,

        選曲ステージ_開始音,
        //選曲ステージ_ループBGM,    --> なし
        選曲ステージ_曲決定音,

        オプション設定ステージ_開始音,

        曲読み込みステージ_開始音,
        曲読み込みステージ_ループBGM,

        終了ステージ_開始音,
    }
}

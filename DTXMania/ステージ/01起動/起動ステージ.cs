using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania
{
    class 起動ステージ : ステージ
    {
        public enum フェーズ
        {
            曲ツリー構築中,
            ドラムサウンド構築中,
            開始音終了待ち,
            確定,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; }



        // 生成と終了


        public 起動ステージ()
        {
        }

        public override void Dispose()
        {
        }



        // 活性化と非活性化


        public override void 活性化する()
        {
        }

        public override void 非活性化する()
        {
        }

        public override void グラフィックリソースを復元する()
        {
        }

        public override void グラフィックリソースを解放する()
        {
        }



        // 進行と描画


        public override void 進行する()
        {
        }

        public override void 描画する()
        {
        }
    }
}

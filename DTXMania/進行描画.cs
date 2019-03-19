using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FDK;

namespace DTXMania
{
    class 進行描画 : FDK.進行描画
    {
        protected override void 進行する()
        {
            if( this._fps.FPSをカウントしプロパティを更新する() )
                this._FPSが変更された();

            base.進行する();
        }

        protected override void 描画する()
        {
            this._fps.VPSをカウントする();

            base.描画する();
        }

        protected override void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            base.スワップチェーンに依存するグラフィックリソースを作成する();
        }

        protected override void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            base.スワップチェーンに依存するグラフィックリソースを解放する();
        }


        private void _FPSが変更された()
        {
            Debug.WriteLine( $"{this._fps.現在のVPS}vps, {this._fps.現在のFPS}fps" );
        }

        private FPS _fps = new FPS();



        // IDTXManiaService の実装


        public AutoResetEvent ViewerPlay( string path, int startPart = 0, bool drumsSound = true )
        {
            var msg = new ViewerPlayメッセージ {
                path = path,
                startPart = startPart,
                drumSound = drumsSound,
            };
            this._メッセージキュー.Enqueue( msg );
            return msg.完了通知;
        }

        private class ViewerPlayメッセージ : 通知メッセージ
        {
            public string path = "";
            public int startPart = 0;
            public bool drumSound = true;
        }

        private void _ViewrePlay( ViewerPlayメッセージ msg )
        {
            // undone: ViewerPlay の実装
            throw new NotImplementedException();
        }


        public AutoResetEvent ViewerStop()
        {
            var msg = new ViewerStopメッセージ();
            this._メッセージキュー.Enqueue( msg );
            return msg.完了通知;
        }

        private class ViewerStopメッセージ : 通知メッセージ
        {
        }

        private void _ViewerStop( ViewerStopメッセージ msg )
        {
            // undone: ViewerStop の実装
            throw new NotImplementedException();
        }


        public float GetSoundDelay()    // 常に同期
        {
            // undone: GetSoundDelay の実装
            throw new NotImplementedException();
        }
        
    }
}

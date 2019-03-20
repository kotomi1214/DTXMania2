using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using FDK;

namespace DTXMania
{
    class 進行描画 : FDK.進行描画
    {

        // 生成と終了


        protected override void On開始する()
        {
            this._fps = new FPS();

            this.起動ステージ = new 起動ステージ();
            this.終了ステージ = new 終了ステージ();


            // 最初のステージを設定し、活性化する。

            this.現在のステージ = this.終了ステージ;
            this.終了ステージ.活性化する();
        }

        protected override void On終了する()
        {
            this.現在のステージ = null;

            this.終了ステージ?.Dispose();
            this.起動ステージ?.Dispose();

            this._fps?.Dispose();
        }



        // 進行と描画


        protected ステージ 現在のステージ;

        protected override void 進行する()
        {
            if( this._fps.FPSをカウントしプロパティを更新する() )
                this._FPSが変更された();

            
            // ステージを進行する。

            this.現在のステージ?.進行する();

            
            // 進行結果により処理分岐。

            switch( this.現在のステージ )
            {
                case 起動ステージ stage:
                    break;
            }
        }

        protected override void 描画する()
        {
            this._fps.VPSをカウントする();


            // ステージを描画する。

            this.現在のステージ.描画する();
        }



        // ステージ


        protected 起動ステージ 起動ステージ;

        protected 終了ステージ 終了ステージ;



        // サイズ変更


        protected override void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            base.スワップチェーンに依存するグラフィックリソースを作成する();
        }

        protected override void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            base.スワップチェーンに依存するグラフィックリソースを解放する();
        }



        // VPS, FPS


        private void _FPSが変更された()
        {
            Debug.WriteLine( $"{this._fps.現在のVPS}vps, {this._fps.現在のFPS}fps" );
        }

        private FPS _fps;



        // IDTXManiaService の実装


        #region " IDTXManiaService.ViewerPlay "
        //----------------
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
        //----------------
        #endregion

        #region " IDTXManiaService.ViewerStop "
        //----------------
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
        //----------------
        #endregion

        #region " IDTXManiaService.GetSoundDelay "
        //----------------
        public float GetSoundDelay()    // 常に同期
        {
            // undone: GetSoundDelay の実装
            throw new NotImplementedException();
        }
        //----------------
        #endregion
    }
}

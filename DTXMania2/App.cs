using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;

namespace DTXMania2
{
    /// <summary>
    ///     アプリケーションの進行描画（とそのタスク）。
    /// </summary>
    class App
    {

        // 生成と終了


        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public App()
        {
            using var lb = new LogBlock( Log.現在のメソッド名 );
        }

        /// <summary>
        ///     進行描画タスクを生成し、進行描画処理を開始する。
        /// </summary>
        public void 進行描画タスクを開始する( Size2F 設計画面サイズ, Size2F 物理画面サイズ )
        {
            using var lb = new LogBlock( Log.現在のメソッド名 );

            Task.Run( () => {
                
                Log.現在のスレッドに名前をつける( "進行描画" );
                
                this._進行描画のメインループを実行する( 設計画面サイズ, 物理画面サイズ );

            } );
        }

        /// <summary>
        ///     進行描画処理を終了し、タスクを終了する。
        /// </summary>
        public void 進行描画タスクを終了する()
        {
            using var lb = new LogBlock( Log.現在のメソッド名 );

            // 進行描画タスクに終了を指示する。
            var msg = new TaskMessage( TaskMessage.タスク名.進行描画, TaskMessage.内容名.終了指示 );
            Global.TaskMessageQueue.Post( msg );

            // 進行描画タスクからの完了通知を待つ。
            if( !msg.完了通知.Wait( 5000 ) )
                throw new Exception( "進行描画タスクの終了がタイムアウトしました。" );
        }



        // 進行と描画


        /// <summary>
        ///     進行描画処理の初期化、メインループ、終了処理を行う。
        ///     <see "TaskMessage"/>で終了が指示されるまで、このメソッドからは戻らない。
        /// </summary>
        private void _進行描画のメインループを実行する( Size2F 設計画面サイズ, Size2F 物理画面サイズ )
        {
            #region " 初期化する。"
            //----------------
            QueueTimer timer;
            AutoResetEvent tick通知;

            using( new LogBlock( "進行描画タスクの開始" ) )
            {
                // グローバルリソースの大半は、進行描画タスクの中で生成する。
                Global.生成する( 設計画面サイズ, 物理画面サイズ );

                // 1ms ごとに進行描画ループを行うよう仕込む。
                tick通知 = new AutoResetEvent( false );
                timer = new QueueTimer( 1, 1, () => tick通知.Set() );   // 1ms ごとに Tick通知を set する
            }
            //----------------
            #endregion

            #region " 進行描画ループを実行する。"
            //----------------
            Log.Info( "進行描画ループを開始します。" );

            var スワップチェーン表示タスク = new PresentSwapChainVSync();
            TaskMessage? 終了指示メッセージ = null;

            while( tick通知.WaitOne() )     // Tick 通知が来るまで待機。
            {
                #region " 自分宛のメッセージが届いていたら、すべて処理する。"
                //----------------
                foreach( var msg in Global.TaskMessageQueue.Get( TaskMessage.タスク名.進行描画 ) )
                {
                    switch( msg.内容 )
                    {
                        case TaskMessage.内容名.終了指示:
                            終了指示メッセージ = msg;
                            break;
                    }
                }

                // 終了指示が来てたらループを抜ける。
                if( 終了指示メッセージ is { } )
                    break;
                //----------------
                #endregion

                #region " 進行・描画する。"
                //----------------
                this._進行する();

                if( スワップチェーン表示タスク.表示待機中 )
                {
                    // 表示タスクがすでに起動されているなら、今回は描画も表示も行わない。
                }
                else
                {
                    // 表示タスクが起動していないなら、描画して、表示タスクを起動する。

                    this._描画する();
                    スワップチェーン表示タスク.表示するAsync( Global.DXGIOutput1!, Global.DXGISwapChain1! );
                }
                //----------------
                #endregion
            }
            //----------------
            #endregion

            #region " 終了する。"
            //----------------
            using( new LogBlock( "進行描画タスクの終了" ) )
            {
                Global.解放する();

                // 終了指示を受け取っていた場合は完了を通知する。
                
                終了指示メッセージ?.完了通知.Set();
            }
            //----------------
            #endregion
        }

        /// <summary>
        ///     進行処理を行う。
        /// </summary>
        private void _進行する()
        {
            Global.Animation!.進行する();
        }

        /// <summary>
        ///     描画処理を行う。
        /// </summary>
        private void _描画する()
        {
        }
    }
}

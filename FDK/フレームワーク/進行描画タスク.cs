using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDK
{
    public class 進行描画タスク
    {
        /// <summary>
        ///     進行描画スレッドを生成し、タスクの実行を開始する。
        /// </summary>
        public void 開始する( Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow, IntPtr hControl )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.タスク = Task.Run( () => {    // Task.Run は常に MTAThread
                    this._タスクエントリ( 物理画面サイズ, 設計画面サイズ, hWindow, hControl );
                } );
            }
        }

        /// <summary>
        ///     タスクを終了し、進行描画スレッドを終了する。
        /// </summary>    
        /// <remarks>
        ///     このメソッドは、信楽描画スレッドの終了が完了するまでブロックする。
        /// </remarks>
        public void 終了する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // メインループに終了を通知。
                this._終了指示通知.Set();
                this._Tick通知.Set();

                // 終了完了通知が来るまでブロックする。
                if( !this._終了完了通知.Wait( 5000 ) )
                    Debug.WriteLine( "終了処理がタイムアウトしました。" );
            }
        }


        // protected

        protected bool 未初期化 = true;

        /// <summary>
        ///     進行描画タスクのインスタンス。
        /// </summary>
        protected Task タスク;

        /// <summary>
        ///     WASAPI サウンドデバイス。
        /// </summary>
        protected サウンドデバイス サウンドデバイス;

        /// <summary>
        ///     WindowsAnimation。
        /// </summary>
        protected アニメーション アニメーション;


        /// <summary>
        ///     進行処理（描画以外の処理）を行う。
        /// </summary>
        protected virtual void 進行する()
        {
            // 派生クラスで実装する。
        }

        /// <summary>
        ///     描画処理（スワップチェーンへの描画）を行う。
        /// </summary>
        /// <remarks>
        ///     スワップチェーンの Present は呼び出し元で行うので不要。
        /// </remarks>
        protected virtual void 描画する()
        {
            // 派生クラスで実装する。
        }

        
        // private

        /// <summary>
        ///     進行描画用タイマ。
        ///     定間隔で <see cref="_Tick通知"/> を Set する。
        /// </summary>
        private QueueTimer _タイマ;

        /// <summary>
        ///     進行イベント。
        ///     このイベントが Set されるごとに、進行または描画が１回行われる。
        /// </summary>
        private AutoResetEvent _Tick通知;

        /// <summary>
        ///     終了を指示するイベント。
        ///     これを Set して <see cref="_Tick通知"/> を Set すると、進行描画タスクが終了処理を開始する。
        /// </summary>
        private ManualResetEventSlim _終了指示通知;

        /// <summary>
        ///     進行描画タスクが終了処理を完了すると Set される。
        /// </summary>
        private ManualResetEventSlim _終了完了通知;


        /// <summary>
        ///     進行描画タスクの本体。
        /// </summary>
        private void _タスクエントリ( Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow, IntPtr hControl )
        {
            // 初期化。
            this._タスクの起動処理を行う( 物理画面サイズ, 設計画面サイズ, hWindow, hControl );
            var 表示タスク = new 表示タスク();

            // メインループ。
            while( this._Tick通知.WaitOne() ) // Tick 通知が来るまで待機。
            {
                // 終了通知が来てたらループを抜ける。
                if( this._終了指示通知.IsSet )
                    break;

                // 進行する。
                this.アニメーション.進行する();
                this.進行する();

                // 表示中じゃないなら描画する。
                if( !( 表示タスク.ただいま表示中 ) )
                {
                    this.描画する();

                    // 垂直帰線を待ってスワップチェーンを表示するタスクを開始する。
                    表示タスク.表示開始();
                }
            }

            // 終了。
            this._タスクの終了処理を行う();
        }

        /// <summary>
        ///     初期化。進行描画スレッドから呼び出される。
        /// </summary>
        private void _タスクの起動処理を行う( Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow, IntPtr hControl )
        {
            if( !this.未初期化 )
                throw new InvalidOperationException( "進行描画タスクは既に実行されています。" );

            this.未初期化 = false;

            Thread.CurrentThread.Name = "進行描画";

            グラフィックデバイス.インスタンスを生成する( hWindow, 物理画面サイズ, 設計画面サイズ );
            this.サウンドデバイス = new サウンドデバイス( CSCore.CoreAudioAPI.AudioClientShareMode.Shared );
            this.アニメーション = new アニメーション();
            this._Tick通知 = new AutoResetEvent( false );
            this._終了指示通知 = new ManualResetEventSlim( false );
            this._終了完了通知 = new ManualResetEventSlim( false );

            // タイマを生成し、開始する。
            this._タイマ = new QueueTimer( 1, 1, () => this._Tick通知.Set() );
        }

        /// <summary>
        ///     終了。進行描画スレッドから呼び出される。
        /// </summary>
        private void _タスクの終了処理を行う()
        {
            if( this.未初期化 )
                throw new InvalidOperationException( "進行描画タスクは実行されていません。" );

            // 進行用タイマを停止する。
            this._タイマ?.Dispose();

            this.アニメーション?.Dispose();
            this.サウンドデバイス?.Dispose();
            グラフィックデバイス.インスタンスを解放する();

            // 終了処理完了。
            this._終了完了通知.Set();
        }
    }
}

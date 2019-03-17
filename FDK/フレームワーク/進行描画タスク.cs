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
                this.タスク = Task.Run( () => {

                    // 初期化。
                    this.タスクの起動処理を行う( 物理画面サイズ, 設計画面サイズ, hWindow, hControl );

                    // メインループ。
                    while( this.Tick通知.WaitOne() ) // Tick 通知が来るまで待機。
                    {
                        // 終了通知が来てたらループを抜ける。
                        if( this.終了指示通知.IsSet )
                            break;

                        if( Interlocked.Read( ref this._ただいま表示中 ) == 0 )
                        {
                            this.描画する();

                            // SwapChain を表示するタスクを起動。
                            Interlocked.Increment( ref this._ただいま表示中 );     // 1: 表示中
                            Task.Run( () => {
                                GraphicsContext.Instance[ hControl ].DXGIOutput1.WaitForVerticalBlank();
                                GraphicsContext.Instance[ hControl ].DXGISwapChain1.Present( 1, SharpDX.DXGI.PresentFlags.None );
                                Interlocked.Decrement( ref this._ただいま表示中 ); // 0: 表示完了
                            } );
                        }
                        else
                        {
                            this.アニメーション.進行する();
                            this.進行する();
                        }
                    }

                    // 終了。
                    this.タスクの終了処理を行う();

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
                this.終了指示通知.Set();
                this.Tick通知.Set();

                // 終了完了通知が来るまでブロックする。
                if( !this.終了完了通知.Wait( 5000 ) )
                    Debug.WriteLine( "終了処理がタイムアウトしました。" );
            }
        }



        protected bool 未初期化 = true;

        /// <summary>
        ///     DirectComposition のターゲットとなるウィンドウハンドル。
        /// </summary>
        protected IntPtr hWindow;

        /// <summary>
        ///     スワップチェーンが作成される描画先コントロールのハンドル。
        /// </summary>
        protected IntPtr hControl;

        /// <summary>
        ///     進行描画タスクのインスタンス。
        /// </summary>
        protected Task タスク;

        /// <summary>
        ///     WindowsAnimation。
        /// </summary>
        protected WindowsAnimation アニメーション;


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


        /// <summary>
        ///     進行イベント。
        ///     このイベントが Set されるごとに、進行または描画が１回行われる。
        /// </summary>
        private AutoResetEvent Tick通知;

        /// <summary>
        ///     終了を指示するイベント。
        ///     これを Set して <see cref="Tick通知"/> を Set すると、進行描画タスクが終了処理を開始する。
        /// </summary>
        private ManualResetEventSlim 終了指示通知;

        /// <summary>
        ///     進行描画タスクが終了処理を完了すると Set される。
        /// </summary>
        private ManualResetEventSlim 終了完了通知;

        /// <summary>
        ///     進行描画用タイマ。
        ///     定間隔で <see cref="Tick通知"/> を Set する。
        /// </summary>
        private QueueTimer タイマ;

        /// <summary>
        ///     0 なら描画処理が可能、非 0 なら描画処理は不可（スワップチェーンの表示待機中のため）。
        ///     Interlocked クラスを使ってアクセスすること。
        /// </summary>
        private long _ただいま表示中 = 0;


        /// <summary>
        ///     初期化。進行描画スレッドから呼び出される。
        /// </summary>
        private void タスクの起動処理を行う( Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow, IntPtr hControl )
        {
            if( !this.未初期化 )
                throw new InvalidOperationException( "進行描画タスクは既に実行されています。" );

            this.未初期化 = false;

            Thread.CurrentThread.Name = "進行描画";

            this.hWindow = hWindow;
            this.hControl = hControl;

            // グラフィックリソースを生成する。
            GraphicsContext.生成する( 物理画面サイズ, 設計画面サイズ, this.hWindow, this.hControl );

            this.アニメーション = new WindowsAnimation();
            this.Tick通知 = new AutoResetEvent( false );
            this.終了指示通知 = new ManualResetEventSlim( false );
            this.終了完了通知 = new ManualResetEventSlim( false );
            Interlocked.Exchange( ref this._ただいま表示中, 0 );

            // タイマを生成し、開始する。
            this.タイマ = new QueueTimer( 1, 1, () => this.Tick通知.Set() );
        }

        /// <summary>
        ///     終了。進行描画スレッドから呼び出される。
        /// </summary>
        private void タスクの終了処理を行う()
        {
            if( this.未初期化 )
                throw new InvalidOperationException( "進行描画タスクは実行されていません。" );

            // 進行用タイマを停止する。
            this.タイマ?.Dispose();

            this.アニメーション?.Dispose();

            // グラフィックリソースを解放する。
            GraphicsContext.Instance[ this.hControl ].Dispose();

            // 終了処理完了。
            this.終了完了通知.Set();
        }
    }
}

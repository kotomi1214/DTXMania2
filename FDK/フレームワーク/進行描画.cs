using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDK
{
    /// <summary>
    ///     アプリの進行描画を受け持つ。
    /// </summary>
    /// <remarks>
    ///     STAThread である <see cref="AppForm"/> とは異なり、MTAThread で動作する。
    /// </remarks>
    public class 進行描画
    {
        public AppForm AppForm { get; protected set; }



        // 開始、終了


        public void 開始する( AppForm appForm, Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow )
        {
            this.AppForm = AppForm;

            this._タスク = Task.Run( () => {
                this._タスクエントリ( 物理画面サイズ, 設計画面サイズ, hWindow );
            } );
        }

        public AutoResetEvent 終了する()
        {
            var msg = new 終了メッセージ();
            this._メッセージキュー.Enqueue( msg );
            return msg.完了通知;
        }


        /// <summary>
        ///     進行描画スレッドのインスタンス。
        /// </summary>
        private Task _タスク;

        /// <summary>
        ///     進行描画用タイマ。
        ///     定間隔で <see cref="_Tick通知"/> を Set する。
        /// </summary>
        private QueueTimer _タイマ;

        /// <summary>
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

        private class 終了メッセージ : 通知メッセージ
        {
        }



        // 進行、描画


        protected virtual void 進行する()
        {
            // 派生クラスで実装すること。
        }

        protected virtual void 描画する()
        {
            // 派生クラスで実装すること。
        }


        private void _タスクエントリ( Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow )
        {
            #region " 初期化 "
            //----------------
            Thread.CurrentThread.Name = "進行描画";
            Debug.Assert( Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA, "MTAThread で実行してください。" );

            グラフィックデバイス.インスタンスを生成する( hWindow, 物理画面サイズ, 設計画面サイズ );

            this._メッセージキュー = new ConcurrentQueue<通知メッセージ>();
            this._Tick通知 = new AutoResetEvent( false );
            this._終了指示通知 = new ManualResetEventSlim( false );
            this._終了完了通知 = new ManualResetEventSlim( false );

            this._タイマ = new QueueTimer( 1, 1, () => this._Tick通知.Set() );
            //----------------
            #endregion

            #region " メインループ "
            //----------------
            var 表示タスク = new 表示タスク();

            while( this._Tick通知.WaitOne() )     // Tick 通知が来るまで待機。
            {
                // メッセージがあれば全部処理する。

                bool ループを抜ける = false;
                while( this._メッセージキュー.TryDequeue( out 通知メッセージ msg ) )
                {
                    if( msg is 終了メッセージ )
                    {
                        // メインループを抜ける。
                        msg.完了通知.Set();
                        ループを抜ける = true;
                        break;
                    }
                    else
                    {
                        this._メッセージを処理する( msg );
                    }
                }
                if( ループを抜ける )
                    break;

                // 進行する。

                グラフィックデバイス.Instance.アニメーション.進行する();
                this.進行する();


                // 描画する。

                if( false == 表示タスク.ただいま表示中 )    // 描画は、表示中ではないときに限る。
                {
                    this.描画する();

                    // 垂直帰線を待ってスワップチェーンを表示するタスクを開始する。
                    表示タスク.表示開始();
                }
            }
            //----------------
            #endregion

            #region " 終了 "
            //----------------
            this._タイマ?.Dispose();   // 進行用タイマを停止する。

            グラフィックデバイス.インスタンスを解放する();
            this.AppForm = null;

            this._終了完了通知.Set();
            //----------------
            #endregion
        }



        // スレッドメッセージとキュー


        protected abstract class 通知メッセージ
        {
            public AutoResetEvent 完了通知 = new AutoResetEvent( false );
        }

        protected ConcurrentQueue<通知メッセージ> _メッセージキュー;

        protected void _メッセージを処理する( 通知メッセージ msg )
        {
            switch( msg )
            {
                case 終了メッセージ msg2:
                    break;  // 特別にメインループ内で処理するのでここでは何もしない。

                case サイズ変更メッセージ msg2:
                    this._サイズを変更する( msg2 );
                    break;

                default:
                    Log.WARNING( "未定義のメッセージが指定されました。" );
                    break;
            }
        }



        // グラフィックリソースのサイズ変更


        /// <summary>
        ///		グラフィックリソースを、指定された物理画面サイズに合わせて変更する。
        /// </summary>
        public AutoResetEvent サイズを変更する( Size 新物理画面サイズ )
        {
            var msg = new サイズ変更メッセージ {
                新物理画面サイズ = 新物理画面サイズ,
            };
            this._メッセージキュー.Enqueue( msg );
            return msg.完了通知;
        }

        protected class サイズ変更メッセージ : 通知メッセージ
        {
            public Size 新物理画面サイズ;
        }

        protected void _サイズを変更する( サイズ変更メッセージ msg )
        {
            // リソースを解放して、
            this.スワップチェーンに依存するグラフィックリソースを解放する();

            // スワップチェーンを再構築して、
            グラフィックデバイス.Instance.サイズを変更する( msg.新物理画面サイズ );

            // リソースを再作成する。
            this.スワップチェーンに依存するグラフィックリソースを作成する();

            // 完了。
            msg.完了通知.Set();
        }

        protected virtual void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            // 派生クラスで実装すること。
        }

        protected virtual void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            // 派生クラスで実装すること。
        }
    }
}

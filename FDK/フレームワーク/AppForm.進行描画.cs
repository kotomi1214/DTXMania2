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
    public partial class AppForm
    {
		/// <summary>
        ///		進行描画スレッドで行う機能をまとめたクラス。
        /// </summary>
		internal class 進行描画 : IDisposable
		{
            // 起動、メインループ、終了

            /// <summary>
            ///		進行描画タスクを生成し、メインループを開始する。
            /// </summary>
            public 進行描画( AppForm appForm, Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow )
            {
                this._AppForm = appForm;

				// 進行描画スレッドを生成する。
                this._タスク = Task.Run( () => {
                    this._タスクエントリ( 物理画面サイズ, 設計画面サイズ, hWindow );
                } );
            }

            /// <summary>
            ///		メインループを停止し、進行描画タスクを破棄する。
            /// </summary>
            public void Dispose()
            {
                // メインループに終了を通知。
                this._終了指示通知.Set();
                this._Tick通知.Set();

                // 終了完了通知が来るまでブロックする。
                if( !this._終了完了通知.Wait( 5000 ) )
                    Debug.WriteLine( "終了処理がタイムアウトしました。" );
            }


            /// <summary>
            ///		進行描画スレッドのエントリ。
            /// </summary>
            private void _タスクエントリ( Size 物理画面サイズ, Size 設計画面サイズ, IntPtr hWindow )
            {
                #region " 初期化 "
                //----------------
                Thread.CurrentThread.Name = "進行描画";
                Debug.Assert( Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA, "MTAThread で実行してください。" );

                グラフィックデバイス.インスタンスを生成する( hWindow, 物理画面サイズ, 設計画面サイズ );

                this._メッセージキュー = new ConcurrentQueue<Message>();
                this._Tick通知 = new AutoResetEvent( false );
                this._終了指示通知 = new ManualResetEventSlim( false );
                this._終了完了通知 = new ManualResetEventSlim( false );

                this._タイマ = new QueueTimer( 1, 1, () => this._Tick通知.Set() );   // タイマ開始。
                //----------------
                #endregion

                #region " メインループ "
                //----------------
                var 表示タスク = new 表示タスク();

                while( this._Tick通知.WaitOne() )		// Tick 通知が来るまで待機。
                {
                    if( this._終了指示通知.IsSet )
                        break;  // 終了通知が来てたらループを抜ける。

                    lock( this._AppForm._進行描画排他ロック )
                    {
                        // メッセージがあれば全部処理する。

                        while( this._メッセージキュー.TryDequeue( out Message msg ) )
                        {
                            this._メッセージを処理する( msg );
                        }


                        // 進行する。

                        グラフィックデバイス.Instance.アニメーション.進行する();
                        this._AppForm.進行する();


                        // 描画する。

                        if( false == 表示タスク.ただいま表示中 )    // 描画は、表示中ではないときに限る。
                        {
                            this._AppForm.描画する();

                            // 垂直帰線を待ってスワップチェーンを表示するタスクを開始する。
                            表示タスク.表示開始();
                        }
                    }
                }
                //----------------
                #endregion

                #region " 終了 "
                //----------------
                this._タイマ?.Dispose();   // 進行用タイマを停止する。

                グラフィックデバイス.インスタンスを解放する();
               
                this._終了完了通知.Set();	// 終了処理完了。
                //----------------
                #endregion
            }


            /// <summary>
            ///		進行描画タスクが含まれる AppForm への参照。
            ///		借り物なのでDispose しないこと。
            /// </summary>
            private AppForm _AppForm;

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


            // スレッドメッセージ

            private abstract class Message
            {
                public AutoResetEvent 完了通知 = new AutoResetEvent( false );
            }

            private ConcurrentQueue<Message> _メッセージキュー;

            private void _メッセージを処理する( Message msg )
            {
                switch( msg )
                {
                    case サイズを変更するMessage msg2:
                        this._サイズを変更する( msg2 );
                        break;

                    default:
                        Log.WARNING( "未定義のメッセージが指定されました。" );
                        break;
                }
            }


            // サイズ変更

            /// <summary>
            ///		グラフィックリソースを、指定された物理画面サイズに合わせて変更する。
            /// </summary>
            public AutoResetEvent サイズを変更する( Size 新物理画面サイズ )
            {
                var msg = new サイズを変更するMessage {
                    新物理画面サイズ = 新物理画面サイズ,
                };
                this._メッセージキュー.Enqueue( msg );
                return msg.完了通知;
            }

            private class サイズを変更するMessage : Message
            {
                public Size 新物理画面サイズ;
            }

            private void _サイズを変更する( サイズを変更するMessage msg )
            {
                // リソースを解放して、
                this._AppForm.スワップチェーンに依存するグラフィックリソースを解放する();

                // スワップチェーンを再構築して、
                グラフィックデバイス.Instance.サイズを変更する( msg.新物理画面サイズ );

                // リソースを再作成する。
                this._AppForm.スワップチェーンに依存するグラフィックリソースを作成する();

                // 完了。
                msg.完了通知.Set();
            }
        }
    }
}

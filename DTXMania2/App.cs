using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDK;

namespace DTXMania2
{
    [DebuggerDisplay("Form")]   // Form.ToString() の評価タイムアウト回避用。
    public partial class App : Form
    {

        // プロパティ


        /// <summary>
        ///     アプリケーション再起動指示フラグ。
        /// </summary>
        /// <remarks>
        ///     <see cref="App"/> インスタンスの終了時にこのフラグが true になっている場合には、
        ///     このインスタンスの保持者（おそらくProgramクラス）は適切に再起動を行うこと。
        /// </remarks>
        public bool 再起動が必要 { get; protected set; } = false;



        // 生成と終了


        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     アプリケーションの起動処理を行う。
        /// </summary>
        protected override void OnLoad( EventArgs e )
        {
            Log.Header( "アプリケーション起動" );
            using var _ = new LogBlock( Log.現在のメソッド名 );

            // フォームを設定する。
            this.Text = $"DTXMania2 Release {int.Parse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ) ):000}";
            this.ClientSize = new Size( 1024, 576 );
            this.Icon = Properties.Resources.DTXMania2;
            this._ScreenMode = new ScreenMode( this );

            // 入力デバイスを初期化する。
            this._KeyboardHID = new KeyboardHID();
            this._GameControllersHID = new GameControllersHID( this.Handle );
            this._MidiIns = new MidiIns();

            // グローバルリソースを生成する。
            Global.生成する( this,
                設計画面サイズ: new SharpDX.Size2F( 1920f, 1080f ),
                物理画面サイズ: new SharpDX.Size2F( this.ClientSize.Width, this.ClientSize.Height ) );
            画像.全インスタンスで共有するリソースを作成する( Global.D3D11Device1, @"$(Images)\TextureVS.cso", @"$(Images)\TexturePS.cso" );

            // メインループを別スレッドで開始する。
            if( !this._進行描画タスクを起動する().WaitOne( 5000 ) )
                throw new TimeoutException( "進行描画タスクの起動処理がタイムアウトしました。" );

            // 初期化完了。（進行描画タスクの起動後に）
            this._未初期化 = false;

            // 全画面モードが設定されているならここで全画面に切り替える。
            // TODO: if( Global.App.システム設定.全画面モードである )
            //    this.ScreenMode.ToFullscreenMode();

            base.OnLoad( e );
        }

        /// <summary>
        ///     アプリケーションの終了処理を行う。
        /// </summary>
        protected override void OnClosing( CancelEventArgs e )
        {
            Log.Header( "アプリケーション終了" );
            using var _ = new LogBlock( Log.現在のメソッド名 );

            // メインループを終了させる。
            this._進行描画タスクを終了する();

            // グローバルリソースを解放する。
            画像.全インスタンスで共有するリソースを解放する();
            Global.解放する();

            // 入力デバイスを破棄する。
            this._MidiIns.Dispose();
            this._GameControllersHID.Dispose();
            this._KeyboardHID.Dispose();

            // 未初期化状態へ。
            this._未初期化 = true;

            base.OnClosing( e );
        }

        /// <summary>
        ///     再起動フラグをセットして、アプリケーションを終了する。
        /// </summary>
        public void 再起動する()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this.再起動が必要 = true;
            this.Close();
        }



        // 進行と描画


        private ManualResetEvent _進行描画タスクを起動する()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            var 起動完了通知 = new ManualResetEvent( false );

            Task.Run( () => {

                try
                {
                    Log.現在のスレッドに名前をつける( "進行描画" );
                    this._進行描画タスクのNETスレッドID = Thread.CurrentThread.ManagedThreadId;

                    Log.Info( "進行描画タスクを起動しました。" );
                    起動完了通知.Set();


                    // TODO: メインループ

                    while( true )
                    {
                        #region " 自分宛のメッセージが届いていたら、すべて処理する。"
                        //----------------
                        TaskMessage? 終了指示メッセージ = null;
                        
                        foreach( var msg in Global.TaskMessageQueue.Get( TaskMessage.タスク名.進行描画 ) )
                        {
                            switch( msg.内容 )
                            {
                                case TaskMessage.内容名.終了指示:
                                    終了指示メッセージ = msg;
                                    msg.完了通知.Set();
                                    break;

                                case TaskMessage.内容名.サイズ変更:
                                    this._リソースを再構築する( msg );
                                    break;
                            }
                        }

                        // 終了指示が来てたらループを抜ける。
                        if( null != 終了指示メッセージ )
                            break;
                        //----------------
                        #endregion

                        Thread.Sleep( 100 );


                    }
                }
#if !DEBUG
                // GUIスレッド以外のスレッドで発生した例外は、Debug 版だとデバッガがキャッチするが、
                // Release 版だと何も表示されずスルーされるので、念のためログ出力しておく。
                catch( Exception e )
                {
                    Log.ERROR( $"例外が発生しました。\n{e}" );
                }
#endif
                finally
                {
                    Log.Info( "進行描画タスクを終了しました。" );
                }

            } );

            return 起動完了通知;
        }

        private void _進行描画タスクを終了する()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            if( Thread.CurrentThread.ManagedThreadId != this._進行描画タスクのNETスレッドID )
            {
                // 進行描画タスクに終了を指示し、完了を待つ。
                var msg = new TaskMessage(
                    宛先: TaskMessage.タスク名.進行描画,
                    内容: TaskMessage.内容名.終了指示 );

                if( !Global.TaskMessageQueue.Post( msg ).Wait( 5000 ) )     // 最大5秒待つ
                    throw new TimeoutException( "進行描画タスクの終了がタイムアウトしました。" );
            }
            else
            {
                // ハングアップ回避; 念のため。
                Log.WARNING( "進行描画タスクから呼び出されました。完了通知待ちをスキップします。" );
            }
        }




        // ウィンドウサイズの変更


        /* 次の２通りがある。
         * 
         * A.ユーザのドラッグによるサイズ変更。
         *      → ResizeBegin ～ ResizeEnd の範囲内で Resize が発生するがいったん無視し、ResizeEnd のタイミングでサイズの変更を行う。
         * 
         * B.最大化、最小化など。
         *      → ResizeBegin ～ ResizeEnd の範囲外で Resize が発生するので、そのタイミングでサイズの変更を行う。
         */

        protected override void OnResizeBegin( EventArgs e )
        {
            this._リサイズ中 = true; // リサイズ開始

            base.OnResizeBegin( e );
        }

        protected override void OnResizeEnd( EventArgs e )
        {
            this._リサイズ中 = false;    // リサイズ終了（先に設定）

            if( this.WindowState == FormWindowState.Minimized )
            {
                // (A) 最小化された → 何もしない
            }
            else if( this.ClientSize.IsEmpty )
            {
                // (B) クライアントサイズが空 → たまに起きるらしい。スキップする。
            }
            else
            {
                // (C) それ以外は Resize イベントハンドラへ委譲。
                this.OnResize( e );
            }

            base.OnResizeEnd( e );
        }

        protected override void OnResize( EventArgs e )
        {
            if( this._未初期化 || this._リサイズ中 )
            {
                //Log.Info( "未初期化、またはリサイズ中なので無視します。" );
                return;
            }
            else
            {
                using var _ = new LogBlock( Log.現在のメソッド名 );

                Log.Info( $"新画面サイズ: {this.ClientSize}" );

                // スワップチェーンとその依存リソースを解放し、改めて作成しなおすように進行描画タスクへ指示する。
                var msg = new TaskMessage(
                    宛先: TaskMessage.タスク名.進行描画,
                    内容: TaskMessage.内容名.サイズ変更,
                    引数: new object[] { this.ClientSize } );

                // 進行描画タスクからの完了通知を待つ。
                if( !Global.TaskMessageQueue.Post( msg ).Wait( 5000 ) )
                    throw new TimeoutException( "サイズ変更タスクメッセージがタイムアウトしました。" );
            }

            base.OnResize( e );
        }

        private bool _リサイズ中 = false;

        private void _リソースを再構築する( TaskMessage msg )
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            // リソースを解放して、
            //this.Onスワップチェーンに依存するグラフィックリソースの解放();

            // スワップチェーンを再構築して、
            var size = (System.Drawing.Size) msg.引数![ 0 ];
            Global.物理画面サイズを変更する( new SharpDX.Size2F( size.Width, size.Height ) );

            // リソースを再作成する。
            //this.Onスワップチェーンに依存するグラフィックリソースの作成();

            // 完了。
            msg.完了通知.Set();
        }



        // ローカル


        /// <summary>
        ///     アプリの初期化が完了していなければ true。
        ///     起動直後は true, OnLoad() で false, OnClosing() で true になる。
        /// </summary>
        /// <remarks>
        ///     アプリの OnLoad() より前に OnResize() が呼び出されることがあるので、その対策用。
        /// </remarks>
        private bool _未初期化 = true;

        /// <summary>
        ///     画面モード（ウィンドウ、全画面）。
        /// </summary>
        private ScreenMode _ScreenMode = null!;

        /// <summary>
        ///     HIDキーボード入力。
        /// </summary>
        /// <remarks>
        ///     接続されているHIDキーボードを管理する。
        ///     RawInputを使うので、フォームのスレッドで管理する。（RawInputはUIスレッドでのみ動作する）
        /// </remarks>
        private KeyboardHID _KeyboardHID = null!;

        /// <summary>
        ///     すべてのHIDゲームパッド、HIDジョイスティック入力。
        /// </summary>
        /// <remarks>
        ///     すべてのゲームパッド／ジョイスティックデバイスを管理する（<see cref="GameControllersHID.Devices"/>参照）。
        ///     RawInputを使うので、フォームのスレッドで管理する。（RawInputはUIスレッドでのみ動作する）
        /// </remarks>
        private GameControllersHID _GameControllersHID = null!;

        /// <summary>
        ///     すべてのMIDI入力デバイス。
        /// </summary>
        private MidiIns _MidiIns = null!;

        private int _進行描画タスクのNETスレッドID;
    }
}

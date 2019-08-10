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

namespace FDK
{
    /// <summary>
    ///     アプリケーションフォームの基本クラス。
    ///     GUIスレッドとメッセージディスパッチを受け持つ。
    /// </summary>
    public partial class AppFormBase : Form
    {
        /// <summary>
        ///     アプリケーション再起動指示フラグ。
        /// </summary>
        /// <remarks>
        ///     インスタンスの終了時にこのフラグが true になっている場合には、
        ///     このインスタンスの保持者（おそらくProgramクラス）は適切に再起動（<see cref="Application.Restart"/>）を行うこと。
        /// </remarks>
        public bool 再起動が必要 { get; protected set; } = false;



        // 起動、終了


        /// <summary>
        ///     コンストラクタ。
        ///     <see cref="App進行描画Base"/> インスタンスを受け取る。
        /// </summary>
        public AppFormBase( App進行描画Base work )
        {
            InitializeComponent();

            this.App進行描画 = work;
        }

        // アプリケーション開始のトリガ。
        protected override void OnLoad( EventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                base.OnLoad( e );

                this._未初期化 = false;

                TimeGetTime.timeBeginPeriod( 1 );
                PowerManagement.システムの自動スリープと画面の自動非表示を抑制する();
                this.Activate();    // ウィンドウが後ろに隠れることがあるので、最前面での表示を保証する。

                this.キーボード = new キーボードデバイス();
                this.ゲームコントローラ = new ゲームコントローラデバイス( this.Handle );

                this.On開始();

                this.App進行描画.開始する( this, this.ClientSize, new Size( 1920, 1080 ), this.Handle );
            }
        }

        // アプリケーション終了のトリガ。
        protected override void OnClosing( CancelEventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.On終了();

                this.App進行描画.終了を通知する().WaitOne();  // 終了するまで待つ

                this.ゲームコントローラ?.Dispose();
                this.キーボード?.Dispose();

                PowerManagement.システムの自動スリープと画面の自動非表示の抑制を解除する();
                TimeGetTime.timeEndPeriod( 1 );

                this._未初期化 = true;

                base.OnClosing( e );
            }
        }

        protected virtual void On開始()
        {
            // 必要あれば、派生クラスで実装する。
        }

        protected virtual void On終了()
        {
            // 必要あれば、派生クラスで実装する。
        }

        public void 再起動する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.再起動が必要 = true;
                this.Close();
            }
        }


        /// <summary>
        ///     進行描画タスクのインスタンス。
        /// </summary>
        protected App進行描画Base App進行描画;

        /// <summary>
        ///     起動直後は true, OnLoad されて false, OnClosing で true。
        /// </summary>
        private bool _未初期化 = true;

        /// <summary>
        ///		フォーム生成時のパラメータを編集して返す。
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                // DWM によってトップウィンドウごとに割り当てられるリダイレクトサーフェスを持たない。（リダイレクトへの画像転送がなくなる分、少し速くなるらしい）
                const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOREDIRECTIONBITMAP;
                return cp;
            }
        }



        // スレッド間通知、ウィンドウメッセージ

        /* 通知とメッセージについて：
         * 
         * スレッド間で通知を連絡するために、受信スレッドでは通知キューを実装し、ポーリングを行う。
         * このフォームでは、定期的なポーリングチェックを行わないので、通知キューに通知を投入した後に
         * WM_APP_MESSAGE_ARRIVED イベントを送信する必要がある。
         */


        /// <summary>
        ///     いずれかのスレッドで例外が発生したことを通知する。
        /// </summary>
        /// <returns>通知が受信されれば set されるイベント。</returns>
        /// <remarks>
        ///     メインスレッド以外のスレッドで致命的な例外が発生した場合に
        ///     このメソッドを呼び出すこと。
        /// </remarks>
        public AutoResetEvent 例外を通知する( Exception ex )
        {
            var msg = new 例外発生通知() { 発生した例外 = ex };
            this.通知キュー.Enqueue( msg );

            // 通知したことをウィンドウメッセージで伝える。
            this.BeginInvoke( new Action( () => {   // UIスレッドで実行する
                PostMessage( this.Handle, WM_APP_MESSAGE_ARRIVED, 0, 0 );
            } ) );

            return msg.完了通知;
        }

        private const int WM_INPUT = 0x00FF;
        private const int WM_APP = 0x8000;
        private const int WM_APP_MESSAGE_ARRIVED = WM_APP + 1;

        protected 通知キュー 通知キュー = new 通知キュー();

        /// <summary>
        ///     このフォームのウィンドウメッセージ処理。
        /// </summary>
        protected override void WndProc( ref Message msg )
        {
            switch( msg.Msg )
            {
                case WM_INPUT:
                    this.OnInput( msg );
                    break;

                case WM_APP_MESSAGE_ARRIVED:
                    this.OnAppMessageArrivec( msg );
                    break;
            }

            base.WndProc( ref msg );
        }

        /// <summary>
        ///     <see cref="WM_APP_MESSAGE_ARRIVED"/> ハンドラ。
        /// </summary>
        protected virtual void OnAppMessageArrivec( in Message msg )
        {
            if( this.通知キュー.TryDequeue( out 通知 threadMessage ) )
            {
                switch( threadMessage )
                {
                    case 例外発生通知 msg2:
                        throw new Exception( "スレッド内で例外が発生しました。", msg2.発生した例外 );
                }
            }
        }

        [DllImport( "user32.dll", SetLastError = true )]
        private static extern bool PostMessage( IntPtr hWnd, int Msg, int wParam, int lParam );



        // Raw Input


        public キーボードデバイス キーボード { get; protected set; }

        public ゲームコントローラデバイス ゲームコントローラ { get; protected set; }

        /// <summary>
        ///     WM_INPUT ハンドラ。GUIスレッドで実行される。 
        /// </summary>
        /// <param name="msg">WM_INPUT のメッセージ。</param>
        protected virtual void OnInput( in Message msg )
        {
            RawInput.RawInputData rawInputData;

            #region " RawInput データを取得する。"
            //----------------
            {
                // ※ RawInputData は可変長構造体である。
                int dataSize = Marshal.SizeOf<RawInput.RawInputData>(); // 仮サイズ。

                // 実サイズを取得する。
                IntPtr dataPtr = IntPtr.Zero;
                if( 0 > RawInput.GetRawInputData( msg.LParam, RawInput.DataType.Input, ref dataPtr, ref dataSize, Marshal.SizeOf<RawInput.RawInputHeader>() ) )
                {
                    Debug.WriteLine( $"GetRawInputData(): error = { Marshal.GetLastWin32Error()}" );
                    return;
                }

                // 実データを取得する。
                var dataBytes = new byte[ dataSize ];
                if( 0 > RawInput.GetRawInputData( msg.LParam, RawInput.DataType.Input, dataBytes, ref dataSize, Marshal.SizeOf<RawInput.RawInputHeader>() ) )
                {
                    Debug.WriteLine( $"GetRawInputData(): error = { Marshal.GetLastWin32Error()}" );
                    return;
                }

                // 実データ byte[] を RawInputData 構造体に変換する。
                var gch = GCHandle.Alloc( dataBytes, GCHandleType.Pinned );
                rawInputData = Marshal.PtrToStructure<RawInput.RawInputData>( gch.AddrOfPinnedObject() );
                gch.Free();
            }
            //----------------
            #endregion

            this.キーボード?.WM_INPUTを処理する( rawInputData );
            this.ゲームコントローラ?.WM_INPUTを処理する( rawInputData );
        }

        
        
        // フォームサイズの変更

        /* 以下の２通りがある。
         * 
         * ・ユーザのドラッグによるサイズ変更。
         *      → ResizeBegin ～ ResizeEnd が発生するので、ResizeEnd のタイミングでサイズの変更を行う。
         * 
         * ・最大化、最小化など。
         *      → ResizeBegin ～ ResizeEnd の範囲外で Resize が発生するので、そのタイミングでサイズの変更を行う。
         */


        protected override void OnResizeBegin( EventArgs e )
        {
            this._リサイズ中 = true;

            base.OnResizeBegin( e );
        }

        protected override void OnResizeEnd( EventArgs e )
        {
            this._リサイズ中 = false;

            if( this.WindowState == FormWindowState.Minimized )
            {
                // (A) 最小化された → 何もしない
            }
            else if( this.ClientSize.IsEmpty )
            {
                // (B) クライアントサイズが空 → たまに起きるらしい。スキップする。
            }
            else if( this._未初期化 )
            {
                // (C) メインループが始まる前にも数回呼び出されることがある → スキップする。
            }
            else
            {
                // (D) スワップチェーンとその依存リソースを解放し、改めて作成しなおす。
                this.App進行描画.サイズ変更を通知する( this.ClientSize ).WaitOne();   // 完了するまで待つ
            }

            base.OnResizeEnd( e );
        }

        protected override void OnResize( EventArgs e )
        {
            if( !this._未初期化 && !this._リサイズ中 )   // 未初期化、またはリサイズ中なら無視。
            {
                // スワップチェーンとその依存リソースを解放し、改めて作成しなおす。
                this.App進行描画.サイズ変更を通知する( this.ClientSize ).WaitOne();   // 完了するまで待つ
            }

            base.OnResize( e );
        }


        private bool _リサイズ中 = false;



        // 画面モードの変更


        /// <summary>
        ///     現在の画面モード。
        ///     値を set することで画面モードを変更することができる。
        /// </summary>
        public 画面モード 画面モード
        {
            get => this._画面モード;
            set => this.BeginInvoke(    // UIスレッドでの実行を保証。
                new Action( () => this._画面モードを変更する( value ) ) );
        }


        private void _画面モードを変更する( 画面モード 新モード )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                switch( 新モード )
                {
                    case 画面モード.ウィンドウ:

                        if( this._画面モード != 画面モード.ウィンドウ )
                        {
                            Log.Info( $"ウィンドウモードに切り替えます。" );

                            this.WindowState = FormWindowState.Normal;
                            this.ClientSize = this._ウィンドウモードの情報のバックアップ.clientSize;
                            this.FormBorderStyle = this._ウィンドウモードの情報のバックアップ.formBorderStyle;

                            Cursor.Show();

                            this._画面モード = 画面モード.ウィンドウ;
                        }
                        else
                        {
                            Log.WARNING( $"すでにウィンドウモードなので、何もしません。" );
                        }
                        break;

                    case 画面モード.全画面:

                        if( this._画面モード != 画面モード.全画面 )
                        {
                            Log.Info( $"全画面モードに切り替えます。" );

                            this._ウィンドウモードの情報のバックアップ.clientSize = this.ClientSize;
                            this._ウィンドウモードの情報のバックアップ.formBorderStyle = this.FormBorderStyle;

                            // 正確には、「全画面(fullscreen)」ではなく「最大化(maximize)」。
                            // 参考: http://www.atmarkit.co.jp/ait/articles/0408/27/news105.html
                            this.WindowState = FormWindowState.Normal;
                            this.FormBorderStyle = FormBorderStyle.None;
                            this.WindowState = FormWindowState.Maximized;

                            Cursor.Hide();

                            this._画面モード = 画面モード.全画面;
                        }
                        else
                        {
                            Log.WARNING( $"すでに全画面モードなので、何もしません。" );
                        }
                        break;
                }
            }
        }

        private 画面モード _画面モード = 画面モード.ウィンドウ;

        /// <summary>
        ///		ウィンドウを全画面モードにする直前に取得し、
        ///		再びウィンドウモードに戻して状態を復元する時に参照する。
        /// </summary>
        private (Size clientSize, FormBorderStyle formBorderStyle) _ウィンドウモードの情報のバックアップ;
    }
}

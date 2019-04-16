using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FDK
{
    public partial class AppForm : Form
    {
        /// <summary>
        ///     アプリの再起動が指示されたときはこれを true にするので、Program 側で適切に確認して処理すること。
        /// </summary>
        public bool 再起動が必要 { get; protected set; } = false;


        // 起動、終了


        public AppForm( 進行描画 work )
        {
            InitializeComponent();

            this.進行描画 = work;
        }

        public virtual void 開始する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._未初期化 = false;
                TimeGetTime.timeBeginPeriod( 1 );
                PowerManagement.システムの自動スリープと画面の自動非表示を抑制する();

                this.Activate();    // ウィンドウが後ろに隠れることがあるので、最前面での表示を保証する。

                this.進行描画.開始する( this, this.ClientSize, new Size( 1920, 1080 ), this.Handle );
            }
        }

        public virtual void 終了する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.進行描画.終了する().WaitOne();  // 終了するまで待つ

                PowerManagement.システムの自動スリープと画面の自動非表示の抑制を解除する();
                TimeGetTime.timeEndPeriod( 1 );

                this._未初期化 = true;
            }
        }

        public void 再起動する()
        {
            this.再起動が必要 = true;
            this.Close();
        }

        // 開始のトリガ
        protected override void OnLoad( EventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.開始する();
                base.OnLoad( e );
            }
        }

        // 終了のトリガ
        protected override void OnClosing( CancelEventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.終了する();
                base.OnClosing( e );
            }
        }


        /// <summary>
        ///     進行描画タスクのインスタンス。
        /// </summary>
        protected 進行描画 進行描画;

        /// <summary>
        ///     起動直後は true, OnLoad されて false, OnClosing で true。
        /// </summary>
        private bool _未初期化 = true;

        /// <summary>
        ///     このフォームと進行描画タスク間での排他用。
        /// </summary>
        protected readonly object 進行描画排他ロック = new object();

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



        // ウィンドウアクティベート


        public bool ウィンドウがアクティブである { get; set; } = false;

        protected override void OnActivated( EventArgs e )
        {
            this.ウィンドウがアクティブである = true;
            Log.Info( "ウィンドウがアクティブ化されました。" );

            base.OnActivated( e );
        }

        protected override void OnDeactivate( EventArgs e )
        {
            this.ウィンドウがアクティブである = false;
            Log.Info( "ウィンドウが非アクティブ化されました。" );

            base.OnDeactivate( e );
        }



        // Raw Input


        public キーボードデバイス キーボード { get; protected set; }

        private const int WM_INPUT = 0x00FF;

        /// <summary>
        ///     WM_INPUT ハンドラ。GUIスレッドで実行される。 
        /// </summary>
        /// <param name="msg">WM_INPUT のメッセージ。</param>
        protected virtual void OnInput( in Message msg )
        {
            this.キーボード.WM_INPUTを処理する( msg );
        }

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
            }

            base.WndProc( ref msg );
        }



        // フォームサイズの変更

        // 以下の２通りがある。
        // ・ユーザのドラッグによるサイズ変更。→ ResizeEnd のタイミングで、サイズの変更を行う。
        // ・最大化、最小化など。→ ResizeBegin～ResizeEnd の範囲外で発生した Resize でもサイズの変更を行う。


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
                this.進行描画.サイズを変更する( this.ClientSize ).WaitOne();   // 完了するまで待つ
            }

            base.OnResizeEnd( e );
        }

        protected override void OnResize( EventArgs e )
        {
            if( !this._未初期化 && !this._リサイズ中 )   // 未初期化、またはリサイズ中なら無視。
                this.進行描画.サイズを変更する( this.ClientSize ).WaitOne();   // 完了するまで待つ

            base.OnResize( e );
        }

        private bool _リサイズ中 = false;



        // 画面モードの変更


        public 画面モード 画面モード
        {
            get => this._画面モード;
            set => this.BeginInvoke( new Action( () => this._画面モードを変更する( value ) ) );   // UIスレッドで実行
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

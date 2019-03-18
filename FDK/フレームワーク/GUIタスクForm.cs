using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FDK
{
    /// <summary>
    ///     GUIスレッドで実行されるフォーム。
    /// </summary>
    public partial class GUIタスクForm : Form
    {
        /// <summary>
        ///     現在の画面モード。
        /// </summary>
        public 画面モード 画面モード
        {
            get => this._画面モード;
            set => this._画面モードを変更する( value );
        }


        /// <summary>
        ///     アプリの生成、初期化。
        /// </summary>
        public GUIタスクForm( 進行描画タスク task = null )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                InitializeComponent();

                TimeGetTime.timeBeginPeriod( 1 );
                PowerManagement.システムの自動スリープと画面の自動非表示を抑制する();

                this.キーボードデバイス = new キーボードデバイス();
                this.進行描画タスク = task ?? new 進行描画タスク();

                this.未初期化 = false;
            }
        }


        /// <summary>
        ///		コンストラクタでの初期化が終わっていれば false。
        /// </summary>
        protected bool 未初期化 = true;

        /// <summary>
        ///     アプリの進行描画を行うタスク。
        /// </summary>
        /// <remarks>
        ///     フォームは STAThread だが、進行描画タスクは MTAThread で実行すること。
        /// </remarks>
        protected 進行描画タスク 進行描画タスク;

        /// <summary>
        ///     HIDキーボードからの入力を扱う。
        /// </summary>
        /// <remarks>
        ///     RawInput は WM_INPUT で通知されるので、フォームクラスのメンバとする。
        /// </remarks>
        protected キーボードデバイス キーボードデバイス;

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


        /// <summary>
        ///     アプリの起動
        /// </summary>
        protected override void OnLoad( EventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.進行描画タスク.開始する( this.ClientSize, new Size( 1920, 1080 ), this.Handle, this.Handle );

                base.OnLoad( e );

                this.Activate();    // ウィンドウが後ろに隠れることがあるので、最前面での表示を保証する。
            }
        }

        /// <summary>
        ///     アプリの終了
        /// </summary>
        protected override void OnClosing( CancelEventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.進行描画タスク.終了する();
                this.キーボードデバイス?.Dispose();
                this.未初期化 = true;

                PowerManagement.システムの自動スリープと画面の自動非表示の抑制を解除する();
                TimeGetTime.timeEndPeriod( 1 );

                base.OnClosing( e );
            }
        }

        /// <summary>
        ///     ユーザによるフォームのリサイズ終了
        /// </summary>
        protected override void OnResizeEnd( EventArgs e )
        {
            if( this.WindowState == FormWindowState.Minimized )
            {
                // (A) 最小化された → 何もしない
            }
            else if( this.ClientSize.IsEmpty )
            {
                // (B) クライアントサイズが空 → たまに起きるらしい。スキップする。
            }
            else if( this.未初期化 )
            {
                // (C) メインループが始まる前にも数回呼び出されることがある → スキップする。
            }
            else
            {
                // (D) スワップチェーンとその依存リソースを解放し、改めて作成しなおす。

                this.スワップチェーンに依存するグラフィックリソースを解放する();

                グラフィックデバイス.Instance.サイズを変更する( this.ClientSize );

                this.スワップチェーンに依存するグラフィックリソースを作成する();
            }

            base.OnResizeEnd( e );
        }

        /// <summary>
        ///     WM_INPUT ハンドラ
        /// </summary>
        /// <param name="msg">WM_INPUT のメッセージ。</param>
        protected virtual void OnInput( in Message msg )
        {
            this.キーボードデバイス.WM_INPUTを処理する( msg );
        }

        /// <summary>
        ///     フォームのウィンドウメッセージディスパッチ
        /// </summary>
        protected override void WndProc( ref Message m )
        {
            switch( m.Msg )
            {
                case WM_INPUT:
                    this.OnInput( m );
                    break;
            }

            base.WndProc( ref m );
        }

        /// <summary>
        ///     スワップチェーンの作成直後に呼び出される。
        /// </summary>
        protected virtual void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            // 派生クラスで実装すること。
        }

        /// <summary>
        ///     スワップチェーンの破棄直前に呼び出される。
        /// </summary>
        protected virtual void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            // 派生クラスで実装すること。
        }


        /// <summary>
        ///     RawInput からの入力
        /// </summary>
        private const int WM_INPUT = 0x00FF;

        /// <summary>
        ///     現在の画面モード。
        /// </summary>
        private 画面モード _画面モード = 画面モード.ウィンドウ;

        /// <summary>
        ///		ウィンドウを全画面モードにする直前に取得し、
        ///		再びウィンドウモードに戻して状態を復元する時に参照する。
        ///		（参照：<see cref="_画面モードを変更する(画面モード)"/>）
        /// </summary>
        private (Size clientSize, FormBorderStyle formBorderStyle) _ウィンドウモードの情報のバックアップ;


        /// <summary>
        ///     フォームの表示形態を切り替える。
        /// </summary>
        private void _画面モードを変更する( 画面モード mode )
        {
            switch( mode )
            {
                case 画面モード.ウィンドウ:

                    if( this._画面モード != 画面モード.ウィンドウ )
                    {
                        this._ウィンドウモードの情報のバックアップ.clientSize = this.ClientSize;
                        this._ウィンドウモードの情報のバックアップ.formBorderStyle = this.FormBorderStyle;

                        // (参考) http://www.atmarkit.co.jp/ait/articles/0408/27/news105.html
                        this.WindowState = FormWindowState.Normal;
                        this.FormBorderStyle = FormBorderStyle.None;
                        this.WindowState = FormWindowState.Maximized;

                        Cursor.Hide();

                        this._画面モード = 画面モード.ウィンドウ;
                    }
                    break;

                case 画面モード.全画面:

                    if( this._画面モード != 画面モード.全画面 )
                    {
                        // 正確には、「全画面(fullscreen)」ではなく「最大化(maximize)」。
                        this.WindowState = FormWindowState.Normal;
                        this.ClientSize = this._ウィンドウモードの情報のバックアップ.clientSize;
                        this.FormBorderStyle = this._ウィンドウモードの情報のバックアップ.formBorderStyle;

                        Cursor.Show();

                        this._画面モード = 画面モード.全画面;
                    }
                    break;
            }
        }
    }
}

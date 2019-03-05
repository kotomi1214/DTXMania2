using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;

namespace FDK
{
    public class ApplicationForm : SharpDX.Windows.RenderForm
    {
        /// <summary>
        ///		ウィンドウの表示モード（全画面 or ウィンドウ）を示す。
        ///		true なら全画面モード、false ならウィンドウモードである。
        ///		値を set することで、モードを変更することもできる。
        /// </summary>
        /// <remarks>
        ///		正確には、「全画面(fullscreen)」ではなく「最大化(maximize)」。
        /// </remarks>
        public bool 全画面モード
        {
            get
                => this.IsFullscreen;

            set
            {
                Trace.Assert( this._初期化完了 );

                if( value )
                {
                    if( !( this.IsFullscreen ) )
                    {
                        this._ウィンドウモードの情報のバックアップ.clientSize = this.ClientSize;
                        this._ウィンドウモードの情報のバックアップ.formBorderStyle = this.FormBorderStyle;

                        // (参考) http://www.atmarkit.co.jp/ait/articles/0408/27/news105.html
                        this.WindowState = FormWindowState.Normal;
                        this.FormBorderStyle = FormBorderStyle.None;
                        this.WindowState = FormWindowState.Maximized;

                        Cursor.Hide();
                        this.IsFullscreen = true;
                    }
                    else
                    {
                        // すでに全画面モードなので何もしない。
                    }
                }
                else
                {
                    if( this.IsFullscreen )
                    {
                        this.WindowState = FormWindowState.Normal;
                        this.ClientSize = this._ウィンドウモードの情報のバックアップ.clientSize;
                        this.FormBorderStyle = this._ウィンドウモードの情報のバックアップ.formBorderStyle;

                        Cursor.Show();
                        this.IsFullscreen = false;
                    }
                    else
                    {
                        // すでにウィンドウモードなので何もしない。
                    }
                }
            }
        }

        /// <summary>
        ///		初期化処理。
        /// </summary>
        public ApplicationForm( SizeF 設計画面サイズ, SizeF 物理画面サイズ, bool 深度ステンシルを使う = true )
        {
            this.SetStyle( ControlStyles.ResizeRedraw, true );
            this.ClientSize = 物理画面サイズ.ToSize();
            this.MinimumSize = new Size( 640, 360 );
            this.Text = "FDK.ApplicationForm";

            グラフィックデバイス.インスタンスを生成する( 
                this.Handle, 
                new Size2F( 設計画面サイズ.Width, 設計画面サイズ.Height ),
                new Size2F( 物理画面サイズ.Width, 物理画面サイズ.Height ),
                深度ステンシルを使う );

			PowerManagement.システムの自動スリープと画面の自動非表示を抑制する();

            this.UserResized += this._UserResize;

            this._初期化完了 = true;
        }

        /// <summary>
        ///		終了処理。
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing && this._初期化完了 )
            {
                this._初期化完了 = false;

                PowerManagement.システムの自動スリープと画面の自動非表示の抑制を解除する();

                グラフィックデバイス.インスタンスを解放する();
            }

            base.Dispose( disposing );
        }

        /// <summary>
        ///     描画処理。
        /// </summary>
        public virtual void 描画する()
        {
            //
            // 以下はサンプル。派生クラスで適宜オーバーライドすること。
            //

            var gd = グラフィックデバイス.Instance;

            // アニメーションを進行する。
            gd.Animation.進行する();    // 必ずメイン(UI)スレッドから呼び出すこと。

            // 全面を黒で塗りつぶすだけのサンプル。
            gd.D2DDeviceContext.BeginDraw();
            gd.D2DDeviceContext.Clear( Color4.Black );
            gd.D2DDeviceContext.EndDraw();
        }

        /// <summary>
        ///     進行処理。（描画以外の処理。）
        /// </summary>
        public virtual void 進行する()
        {
            //
            // 描画以外の反復処理があれば、これをオーバーライドすること。
            //
        }


        /// <summary>
        ///		メインループ。
        /// </summary>
        public virtual void Run()
        {
            SharpDX.Windows.RenderLoop.Run( this, () => {

                // 最小化されてたら何もしない。
                if( this.FormWindowState == FormWindowState.Minimized )
                    return;

                if( Interlocked.Read( ref this._PresentNow ) == 0 )
                {
                    this.描画する();

                    // SwapChain を表示するタスクを起動。
                    Interlocked.Increment( ref this._PresentNow );        // 1: 表示開始
                    Task.Run( () => {
                        グラフィックデバイス.Instance.DXGIOutput.WaitForVerticalBlank();
                        グラフィックデバイス.Instance.SwapChain.Present( 1, SharpDX.DXGI.PresentFlags.None );
                        Interlocked.Decrement( ref this._PresentNow );    // 0: 表示完了
                    } );

                }
                else
                {
                    this.進行する();
                }

            } );
        }

        /// <summary>
        ///		コンストラクタでの初期化が終わっていれば true。
        /// </summary>
        protected bool _初期化完了 = false;

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

        protected FormWindowState FormWindowState = FormWindowState.Normal;

        protected virtual void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            // スワップチェーンの作成直後に呼び出される。
            // 派生クラスで実装すること。
        }
        protected virtual void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            // スワップチェーンの破棄直前に呼び出される。
            // 派生クラスで実装すること。
        }

        /// <summary>
        ///     0 なら描画処理が可能、非 0 なら描画処理は不可（スワップチェーンの表示待機中のため）。
        /// </summary>
        private long _PresentNow = 0;

        /// <summary>
        ///		ウィンドウを全画面モードにする直前に取得し、
        ///		再びウィンドウモードに戻して状態を復元する時に参照する。
        ///		（<see cref="全画面モード"/> を参照。）
        /// </summary>
        private (Size clientSize, FormBorderStyle formBorderStyle) _ウィンドウモードの情報のバックアップ;

        /// <summary>
        ///     ユーザによるフォームのリサイズ終了イベント。
        /// </summary>
        private void _UserResize( object sender, EventArgs e )
        {
            this.FormWindowState = this.WindowState;

            if( this.FormWindowState == FormWindowState.Minimized )
            {
                // 最小化されたので何もしない。
            }
            else if( this.ClientSize.IsEmpty )
            {
                // たまに起きるらしい。スキップする。
            }
            else
            {
                // メインループ（RenderLoop）が始まる前にも数回呼び出されることがあるので、それをはじく。
                if( !( this._初期化完了 ) )
                    return;

                // スワップチェーンとその依存リソースを解放し、改めて作成しなおす。

                this.スワップチェーンに依存するグラフィックリソースを解放する();

                グラフィックデバイス.Instance.サイズを変更する( this.ClientSize );

                this.スワップチェーンに依存するグラフィックリソースを作成する();
            }
        }
    }
}

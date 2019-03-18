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
        public GUIタスクForm( 進行描画タスク task = null )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                InitializeComponent();

                this.キーボードデバイス = new キーボードデバイス();
                this.進行描画タスク = task ?? new 進行描画タスク();
            }
        }


        protected 進行描画タスク 進行描画タスク;

        protected キーボードデバイス キーボードデバイス;


        protected override void OnLoad( EventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.進行描画タスク.開始する( this.ClientSize, new Size( 1920, 1080 ), this.Handle, this.Handle );

                base.OnLoad( e );

                this.Activate();    // ウィンドウが後ろに隠れることがあるので、最前面での表示を保証する。
            }
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.進行描画タスク.終了する();
                this.キーボードデバイス?.Dispose();

                base.OnClosing( e );
            }
        }

        protected virtual void OnInput( in Message msg )
        {
            this.キーボードデバイス.WM_INPUTを処理する( msg );
        }

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


        private const int WM_INPUT = 0x00FF;
    }
}

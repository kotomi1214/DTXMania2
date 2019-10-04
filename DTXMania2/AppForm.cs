using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DTXMania2
{
    /// <summary>
    ///     アプリケーションのメインフォーム（とUIスレッド）。
    /// </summary>
    partial class AppForm : Form
    {
        
        // 生成と終了


        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public AppForm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     アプリケーションを起動する。
        /// </summary>
        protected override void OnLoad( EventArgs e )
        {
            using var lb = new LogBlock( Log.現在のメソッド名 );

            this.Text = $"DTXMania2 Release {int.Parse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ) )} {( ( Environment.Is64BitProcess ) ? "" : "(x86)" )}";

            Global.AppForm = this;
            Global.Handle = this.Handle;
            Global.App = new App();
            Global.App.進行描画タスクを開始する( new SharpDX.Size2F( 1920f, 1080f ), new SharpDX.Size2F( this.ClientSize.Width, this.ClientSize.Height ) );

            base.OnLoad( e );
        }

        /// <summary>
        ///     アプリケーションを終了する。
        /// </summary>
        protected override void OnClosing( CancelEventArgs e )
        {
            using var lb = new LogBlock( Log.現在のメソッド名 );

            Global.App.進行描画タスクを終了する();

            base.OnClosing( e );
        }
    }
}

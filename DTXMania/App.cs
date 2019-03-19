using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDK;

namespace DTXMania
{
    public partial class App : AppForm
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );
        }

        protected override void 進行する()
        {
            if( this._fps.FPSをカウントしプロパティを更新する() )
                this._FPSが変更された();

            base.進行する();
        }

        protected override void 描画する()
        {
            this._fps.VPSをカウントする();

            base.描画する();
        }

        protected override void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            base.スワップチェーンに依存するグラフィックリソースを作成する();
        }

        protected override void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            base.スワップチェーンに依存するグラフィックリソースを解放する();
        }


        private void _FPSが変更された()
        {
            //Debug.WriteLine( $"{this._fps.現在のVPS}vps, {this._fps.現在のFPS}fps" );
        }

        private FPS _fps = new FPS();
    }
}

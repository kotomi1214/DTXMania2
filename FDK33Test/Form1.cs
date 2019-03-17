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
using CSCore.CoreAudioAPI;
using FDK;

namespace FDK33Test
{
    public partial class Form1 : GUIタスクForm
    {
        public Form1()
            : base( new 進行描画タスク() )
        {
            InitializeComponent();

            this._サウンドデバイス = new SoundDevice( AudioClientShareMode.Shared );
            this._サウンドデバイス.レンダリングを開始する();
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            this._サウンドデバイス.レンダリングを停止する();

            base.OnClosing( e );
        }

        private SoundDevice _サウンドデバイス;
    }
}

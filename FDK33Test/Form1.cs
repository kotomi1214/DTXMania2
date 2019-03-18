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
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );
        }
    }
}

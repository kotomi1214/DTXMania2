using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DTXMania
{
    partial class 未処理例外検出ダイアログ : Form
    {
        public 未処理例外検出ダイアログ()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked( object sender, LinkLabelLinkClickedEventArgs e )
        {
            // 今回のログファイルを選択した状態で explorer を起動する。
            Process.Start( "explorer.exe", $@"/select,""{Program.ログファイル名}""" );

            this.Close();
        }
    }
}

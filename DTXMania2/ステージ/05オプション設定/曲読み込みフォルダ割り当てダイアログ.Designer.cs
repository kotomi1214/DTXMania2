namespace DTXMania2.オプション設定
{
    partial class 曲読み込みフォルダ割り当てダイアログ
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(曲読み込みフォルダ割り当てダイアログ));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listViewフォルダ一覧 = new System.Windows.Forms.ListView();
            this.columnHeaderフォルダ名 = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.button選択 = new System.Windows.Forms.Button();
            this.button削除 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // listViewフォルダ一覧
            // 
            resources.ApplyResources(this.listViewフォルダ一覧, "listViewフォルダ一覧");
            this.listViewフォルダ一覧.AllowColumnReorder = true;
            this.listViewフォルダ一覧.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderフォルダ名});
            this.listViewフォルダ一覧.GridLines = true;
            this.listViewフォルダ一覧.HideSelection = false;
            this.listViewフォルダ一覧.Name = "listViewフォルダ一覧";
            this.listViewフォルダ一覧.UseCompatibleStateImageBehavior = false;
            this.listViewフォルダ一覧.View = System.Windows.Forms.View.Details;
            this.listViewフォルダ一覧.SelectedIndexChanged += new System.EventHandler(this.listViewフォルダ一覧_SelectedIndexChanged);
            // 
            // columnHeaderフォルダ名
            // 
            resources.ApplyResources(this.columnHeaderフォルダ名, "columnHeaderフォルダ名");
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // button選択
            // 
            resources.ApplyResources(this.button選択, "button選択");
            this.button選択.Name = "button選択";
            this.button選択.UseVisualStyleBackColor = true;
            this.button選択.Click += new System.EventHandler(this.button選択_Click);
            // 
            // button削除
            // 
            resources.ApplyResources(this.button削除, "button削除");
            this.button削除.Name = "button削除";
            this.button削除.UseVisualStyleBackColor = true;
            this.button削除.Click += new System.EventHandler(this.button削除_Click);
            // 
            // 曲読み込みフォルダ割り当てダイアログ
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.button削除);
            this.Controls.Add(this.button選択);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listViewフォルダ一覧);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "曲読み込みフォルダ割り当てダイアログ";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.曲読み込みフォルダ割り当てダイアログ_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ListView listViewフォルダ一覧;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeaderフォルダ名;
        private System.Windows.Forms.Button button選択;
        private System.Windows.Forms.Button button削除;
    }
}
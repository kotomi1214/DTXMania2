namespace DTXMania2.オプション設定
{
    partial class 入力割り当てダイアログ
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(入力割り当てダイアログ));
            this.listView入力リスト = new System.Windows.Forms.ListView();
            this.columnHeaderMIDIノート情報 = new System.Windows.Forms.ColumnHeader();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonFootPedalリセット = new System.Windows.Forms.Button();
            this.labelFootPedal現在値 = new System.Windows.Forms.Label();
            this.textBoxFootPedal現在値 = new System.Windows.Forms.TextBox();
            this.labelFootPedal最大値 = new System.Windows.Forms.Label();
            this.labelFootPedal最小値 = new System.Windows.Forms.Label();
            this.pictureBoxFootPedal = new System.Windows.Forms.PictureBox();
            this.textBoxFootPedal最小値 = new System.Windows.Forms.TextBox();
            this.textBoxFootPedal最大値 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.richTextBox3 = new System.Windows.Forms.RichTextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.comboBoxパッドリスト = new System.Windows.Forms.ComboBox();
            this.listView割り当て済み入力リスト = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.button割り当て解除 = new System.Windows.Forms.Button();
            this.button追加 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFootPedal)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView入力リスト
            // 
            resources.ApplyResources(this.listView入力リスト, "listView入力リスト");
            this.listView入力リスト.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderMIDIノート情報});
            this.listView入力リスト.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.listView入力リスト.FullRowSelect = true;
            this.listView入力リスト.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView入力リスト.HideSelection = false;
            this.listView入力リスト.Name = "listView入力リスト";
            this.listView入力リスト.UseCompatibleStateImageBehavior = false;
            this.listView入力リスト.View = System.Windows.Forms.View.Details;
            this.listView入力リスト.DoubleClick += new System.EventHandler(this.listView入力リスト_DoubleClick);
            // 
            // columnHeaderMIDIノート情報
            // 
            resources.ApplyResources(this.columnHeaderMIDIノート情報, "columnHeaderMIDIノート情報");
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
            // buttonFootPedalリセット
            // 
            resources.ApplyResources(this.buttonFootPedalリセット, "buttonFootPedalリセット");
            this.buttonFootPedalリセット.Name = "buttonFootPedalリセット";
            this.buttonFootPedalリセット.UseVisualStyleBackColor = true;
            // 
            // labelFootPedal現在値
            // 
            resources.ApplyResources(this.labelFootPedal現在値, "labelFootPedal現在値");
            this.labelFootPedal現在値.Name = "labelFootPedal現在値";
            // 
            // textBoxFootPedal現在値
            // 
            resources.ApplyResources(this.textBoxFootPedal現在値, "textBoxFootPedal現在値");
            this.textBoxFootPedal現在値.Name = "textBoxFootPedal現在値";
            this.textBoxFootPedal現在値.ReadOnly = true;
            // 
            // labelFootPedal最大値
            // 
            resources.ApplyResources(this.labelFootPedal最大値, "labelFootPedal最大値");
            this.labelFootPedal最大値.Name = "labelFootPedal最大値";
            // 
            // labelFootPedal最小値
            // 
            resources.ApplyResources(this.labelFootPedal最小値, "labelFootPedal最小値");
            this.labelFootPedal最小値.Name = "labelFootPedal最小値";
            // 
            // pictureBoxFootPedal
            // 
            resources.ApplyResources(this.pictureBoxFootPedal, "pictureBoxFootPedal");
            this.pictureBoxFootPedal.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBoxFootPedal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxFootPedal.Name = "pictureBoxFootPedal";
            this.pictureBoxFootPedal.TabStop = false;
            // 
            // textBoxFootPedal最小値
            // 
            resources.ApplyResources(this.textBoxFootPedal最小値, "textBoxFootPedal最小値");
            this.textBoxFootPedal最小値.Name = "textBoxFootPedal最小値";
            this.textBoxFootPedal最小値.ReadOnly = true;
            // 
            // textBoxFootPedal最大値
            // 
            resources.ApplyResources(this.textBoxFootPedal最大値, "textBoxFootPedal最大値");
            this.textBoxFootPedal最大値.Name = "textBoxFootPedal最大値";
            this.textBoxFootPedal最大値.ReadOnly = true;
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.richTextBox3);
            this.groupBox2.Controls.Add(this.pictureBoxFootPedal);
            this.groupBox2.Controls.Add(this.buttonFootPedalリセット);
            this.groupBox2.Controls.Add(this.textBoxFootPedal現在値);
            this.groupBox2.Controls.Add(this.labelFootPedal最小値);
            this.groupBox2.Controls.Add(this.labelFootPedal最大値);
            this.groupBox2.Controls.Add(this.labelFootPedal現在値);
            this.groupBox2.Controls.Add(this.textBoxFootPedal最大値);
            this.groupBox2.Controls.Add(this.textBoxFootPedal最小値);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // richTextBox3
            // 
            resources.ApplyResources(this.richTextBox3, "richTextBox3");
            this.richTextBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox3.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox3.DetectUrls = false;
            this.richTextBox3.Name = "richTextBox3";
            this.richTextBox3.ReadOnly = true;
            this.richTextBox3.ShortcutsEnabled = false;
            this.richTextBox3.TabStop = false;
            // 
            // richTextBox1
            // 
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ShortcutsEnabled = false;
            this.richTextBox1.TabStop = false;
            // 
            // comboBoxパッドリスト
            // 
            resources.ApplyResources(this.comboBoxパッドリスト, "comboBoxパッドリスト");
            this.comboBoxパッドリスト.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxパッドリスト.FormattingEnabled = true;
            this.comboBoxパッドリスト.Name = "comboBoxパッドリスト";
            this.comboBoxパッドリスト.SelectedIndexChanged += new System.EventHandler(this.comboBoxパッドリスト_SelectedIndexChanged);
            // 
            // listView割り当て済み入力リスト
            // 
            resources.ApplyResources(this.listView割り当て済み入力リスト, "listView割り当て済み入力リスト");
            this.listView割り当て済み入力リスト.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView割り当て済み入力リスト.FullRowSelect = true;
            this.listView割り当て済み入力リスト.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView割り当て済み入力リスト.HideSelection = false;
            this.listView割り当て済み入力リスト.Name = "listView割り当て済み入力リスト";
            this.listView割り当て済み入力リスト.Scrollable = false;
            this.listView割り当て済み入力リスト.UseCompatibleStateImageBehavior = false;
            this.listView割り当て済み入力リスト.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // button割り当て解除
            // 
            resources.ApplyResources(this.button割り当て解除, "button割り当て解除");
            this.button割り当て解除.Name = "button割り当て解除";
            this.button割り当て解除.UseVisualStyleBackColor = true;
            this.button割り当て解除.Click += new System.EventHandler(this.button割り当て解除_Click);
            // 
            // button追加
            // 
            resources.ApplyResources(this.button追加, "button追加");
            this.button追加.Name = "button追加";
            this.button追加.UseVisualStyleBackColor = true;
            this.button追加.Click += new System.EventHandler(this.button追加_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // richTextBox2
            // 
            resources.ApplyResources(this.richTextBox2, "richTextBox2");
            this.richTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox2.DetectUrls = false;
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.ShortcutsEnabled = false;
            this.richTextBox2.TabStop = false;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.richTextBox1);
            this.groupBox1.Controls.Add(this.listView入力リスト);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // groupBox3
            // 
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Controls.Add(this.richTextBox2);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.comboBoxパッドリスト);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.listView割り当て済み入力リスト);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // 入力割り当てダイアログ
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button割り当て解除);
            this.Controls.Add(this.button追加);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "入力割り当てダイアログ";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.入力割り当てダイアログ_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFootPedal)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView入力リスト;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ColumnHeader columnHeaderMIDIノート情報;
        private System.Windows.Forms.Button buttonFootPedalリセット;
        private System.Windows.Forms.Label labelFootPedal現在値;
        private System.Windows.Forms.TextBox textBoxFootPedal現在値;
        private System.Windows.Forms.Label labelFootPedal最大値;
        private System.Windows.Forms.Label labelFootPedal最小値;
        private System.Windows.Forms.PictureBox pictureBoxFootPedal;
        private System.Windows.Forms.TextBox textBoxFootPedal最小値;
        private System.Windows.Forms.TextBox textBoxFootPedal最大値;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RichTextBox richTextBox3;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ComboBox comboBoxパッドリスト;
        private System.Windows.Forms.ListView listView割り当て済み入力リスト;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button button割り当て解除;
        private System.Windows.Forms.Button button追加;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}
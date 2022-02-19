
namespace AcqDevice
{
    partial class frm_AcqHKVision_Attr
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage_Basic = new System.Windows.Forms.TabPage();
            this.bt_Save = new System.Windows.Forms.Button();
            this.bt_Exit = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Bt_StopTestAcq = new System.Windows.Forms.Button();
            this.Bt_StartTestAcq = new System.Windows.Forms.Button();
            this.Bt_SetExposeTime = new System.Windows.Forms.Button();
            this.bt_solftTrigger = new System.Windows.Forms.Button();
            this.txt_ExposeTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmb_TrigerMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_IO = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.tabPage_Basic.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage_Basic);
            this.tabControl.Controls.Add(this.tabPage_IO);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(378, 635);
            this.tabControl.TabIndex = 8;
            // 
            // tabPage_Basic
            // 
            this.tabPage_Basic.Controls.Add(this.bt_Save);
            this.tabPage_Basic.Controls.Add(this.bt_Exit);
            this.tabPage_Basic.Controls.Add(this.groupBox1);
            this.tabPage_Basic.Location = new System.Drawing.Point(4, 25);
            this.tabPage_Basic.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_Basic.Name = "tabPage_Basic";
            this.tabPage_Basic.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_Basic.Size = new System.Drawing.Size(370, 606);
            this.tabPage_Basic.TabIndex = 1;
            this.tabPage_Basic.Text = "基本设置";
            this.tabPage_Basic.UseVisualStyleBackColor = true;
            // 
            // bt_Save
            // 
            this.bt_Save.Location = new System.Drawing.Point(44, 548);
            this.bt_Save.Margin = new System.Windows.Forms.Padding(4);
            this.bt_Save.Name = "bt_Save";
            this.bt_Save.Size = new System.Drawing.Size(100, 39);
            this.bt_Save.TabIndex = 7;
            this.bt_Save.Text = "确认";
            this.bt_Save.UseVisualStyleBackColor = true;
            this.bt_Save.Click += new System.EventHandler(this.bt_Save_Click);
            // 
            // bt_Exit
            // 
            this.bt_Exit.Location = new System.Drawing.Point(203, 548);
            this.bt_Exit.Margin = new System.Windows.Forms.Padding(4);
            this.bt_Exit.Name = "bt_Exit";
            this.bt_Exit.Size = new System.Drawing.Size(100, 39);
            this.bt_Exit.TabIndex = 8;
            this.bt_Exit.Text = "取消";
            this.bt_Exit.UseVisualStyleBackColor = true;
            this.bt_Exit.Click += new System.EventHandler(this.bt_Exit_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Bt_StopTestAcq);
            this.groupBox1.Controls.Add(this.Bt_StartTestAcq);
            this.groupBox1.Controls.Add(this.Bt_SetExposeTime);
            this.groupBox1.Controls.Add(this.bt_solftTrigger);
            this.groupBox1.Controls.Add(this.txt_ExposeTime);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cmb_TrigerMode);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(362, 257);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // Bt_StopTestAcq
            // 
            this.Bt_StopTestAcq.Enabled = false;
            this.Bt_StopTestAcq.Location = new System.Drawing.Point(199, 191);
            this.Bt_StopTestAcq.Margin = new System.Windows.Forms.Padding(4);
            this.Bt_StopTestAcq.Name = "Bt_StopTestAcq";
            this.Bt_StopTestAcq.Size = new System.Drawing.Size(100, 37);
            this.Bt_StopTestAcq.TabIndex = 25;
            this.Bt_StopTestAcq.Text = "停止";
            this.Bt_StopTestAcq.UseVisualStyleBackColor = true;
            this.Bt_StopTestAcq.Click += new System.EventHandler(this.Bt_StopTestAcq_Click);
            // 
            // Bt_StartTestAcq
            // 
            this.Bt_StartTestAcq.Enabled = false;
            this.Bt_StartTestAcq.Location = new System.Drawing.Point(47, 191);
            this.Bt_StartTestAcq.Margin = new System.Windows.Forms.Padding(4);
            this.Bt_StartTestAcq.Name = "Bt_StartTestAcq";
            this.Bt_StartTestAcq.Size = new System.Drawing.Size(100, 37);
            this.Bt_StartTestAcq.TabIndex = 24;
            this.Bt_StartTestAcq.Text = "测试运行";
            this.Bt_StartTestAcq.UseVisualStyleBackColor = true;
            this.Bt_StartTestAcq.Click += new System.EventHandler(this.Bt_StartTestAcq_Click);
            // 
            // Bt_SetExposeTime
            // 
            this.Bt_SetExposeTime.Location = new System.Drawing.Point(47, 125);
            this.Bt_SetExposeTime.Margin = new System.Windows.Forms.Padding(4);
            this.Bt_SetExposeTime.Name = "Bt_SetExposeTime";
            this.Bt_SetExposeTime.Size = new System.Drawing.Size(100, 37);
            this.Bt_SetExposeTime.TabIndex = 23;
            this.Bt_SetExposeTime.Text = "设置曝光";
            this.Bt_SetExposeTime.UseVisualStyleBackColor = true;
            this.Bt_SetExposeTime.Click += new System.EventHandler(this.Bt_SetExposeTime_Click);
            // 
            // bt_solftTrigger
            // 
            this.bt_solftTrigger.Location = new System.Drawing.Point(199, 125);
            this.bt_solftTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.bt_solftTrigger.Name = "bt_solftTrigger";
            this.bt_solftTrigger.Size = new System.Drawing.Size(100, 37);
            this.bt_solftTrigger.TabIndex = 22;
            this.bt_solftTrigger.Text = "软触发一次";
            this.bt_solftTrigger.UseVisualStyleBackColor = true;
            this.bt_solftTrigger.Click += new System.EventHandler(this.bt_solftTrigger_Click);
            // 
            // txt_ExposeTime
            // 
            this.txt_ExposeTime.Location = new System.Drawing.Point(157, 26);
            this.txt_ExposeTime.Margin = new System.Windows.Forms.Padding(4);
            this.txt_ExposeTime.Name = "txt_ExposeTime";
            this.txt_ExposeTime.Size = new System.Drawing.Size(142, 25);
            this.txt_ExposeTime.TabIndex = 16;
            this.txt_ExposeTime.Text = "49900";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 29);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "曝光时间[us]：";
            // 
            // cmb_TrigerMode
            // 
            this.cmb_TrigerMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_TrigerMode.FormattingEnabled = true;
            this.cmb_TrigerMode.Items.AddRange(new object[] {
            "硬触发",
            "软件触发",
            "上升沿触发",
            "下降沿触发"});
            this.cmb_TrigerMode.Location = new System.Drawing.Point(157, 74);
            this.cmb_TrigerMode.Margin = new System.Windows.Forms.Padding(4);
            this.cmb_TrigerMode.Name = "cmb_TrigerMode";
            this.cmb_TrigerMode.Size = new System.Drawing.Size(142, 23);
            this.cmb_TrigerMode.TabIndex = 1;
            this.cmb_TrigerMode.SelectedIndexChanged += new System.EventHandler(this.cmb_TrigerMode_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(49, 77);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "触发方式:";
            // 
            // tabPage_IO
            // 
            this.tabPage_IO.Location = new System.Drawing.Point(4, 25);
            this.tabPage_IO.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_IO.Name = "tabPage_IO";
            this.tabPage_IO.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_IO.Size = new System.Drawing.Size(370, 606);
            this.tabPage_IO.TabIndex = 2;
            this.tabPage_IO.Text = " IO 控制";
            this.tabPage_IO.UseVisualStyleBackColor = true;
            // 
            // frm_AcqHKVision_Attr
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1224, 635);
            this.Controls.Add(this.tabControl);
            this.Name = "frm_AcqHKVision_Attr";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "海康相机设置";
            this.Load += new System.EventHandler(this.frm_AcqHKVision_Attr_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPage_Basic.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage_Basic;
        private System.Windows.Forms.Button bt_Save;
        private System.Windows.Forms.Button bt_Exit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txt_ExposeTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmb_TrigerMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_IO;
        private System.Windows.Forms.Button Bt_StopTestAcq;
        private System.Windows.Forms.Button Bt_StartTestAcq;
        private System.Windows.Forms.Button Bt_SetExposeTime;
        private System.Windows.Forms.Button bt_solftTrigger;
    }
}
namespace SudokuSolver
{
    partial class Form1
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.buttonReset = new System.Windows.Forms.Button();
			this.buttonSnapShot = new System.Windows.Forms.Button();
			this.buttonRestore = new System.Windows.Forms.Button();
			this.checkBoxAuto = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelVersionInfo = new System.Windows.Forms.Label();
			this.textBoxDummy = new System.Windows.Forms.TextBox();
			this.buttonRecheck = new System.Windows.Forms.Button();
			this.buttonPopSnapshot = new System.Windows.Forms.Button();
			this.checkBoxClutter = new System.Windows.Forms.CheckBox();
			this.panelDebugTools = new System.Windows.Forms.Panel();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.buttonTestCase = new System.Windows.Forms.Button();
			this.labelDebugInfo = new System.Windows.Forms.Label();
			this.checkBoxDebugTools = new System.Windows.Forms.CheckBox();
			this.buttonExtract = new System.Windows.Forms.Button();
			this.panelDebugTools.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(12, 12);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(78, 23);
			this.buttonReset.TabIndex = 1;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// buttonSnapShot
			// 
			this.buttonSnapShot.Location = new System.Drawing.Point(3, 3);
			this.buttonSnapShot.Name = "buttonSnapShot";
			this.buttonSnapShot.Size = new System.Drawing.Size(110, 23);
			this.buttonSnapShot.TabIndex = 2;
			this.buttonSnapShot.Text = "Push Snapshot";
			this.buttonSnapShot.UseVisualStyleBackColor = true;
			this.buttonSnapShot.Click += new System.EventHandler(this.buttonSnapShot_Click);
			// 
			// buttonRestore
			// 
			this.buttonRestore.Enabled = false;
			this.buttonRestore.Location = new System.Drawing.Point(119, 32);
			this.buttonRestore.Name = "buttonRestore";
			this.buttonRestore.Size = new System.Drawing.Size(110, 23);
			this.buttonRestore.TabIndex = 3;
			this.buttonRestore.Text = "Peek Snapshot";
			this.buttonRestore.UseVisualStyleBackColor = true;
			this.buttonRestore.Click += new System.EventHandler(this.buttonRestore_Click);
			// 
			// checkBoxAuto
			// 
			this.checkBoxAuto.AutoSize = true;
			this.checkBoxAuto.Checked = true;
			this.checkBoxAuto.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAuto.Location = new System.Drawing.Point(96, 16);
			this.checkBoxAuto.Name = "checkBoxAuto";
			this.checkBoxAuto.Size = new System.Drawing.Size(78, 17);
			this.checkBoxAuto.TabIndex = 4;
			this.checkBoxAuto.Text = "Auto Solve";
			this.checkBoxAuto.UseVisualStyleBackColor = true;
			this.checkBoxAuto.CheckedChanged += new System.EventHandler(this.checkBoxAuto_CheckedChanged);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Black;
			this.panel1.Location = new System.Drawing.Point(12, 41);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(198, 198);
			this.panel1.TabIndex = 5;
			// 
			// labelVersionInfo
			// 
			this.labelVersionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelVersionInfo.AutoSize = true;
			this.labelVersionInfo.BackColor = System.Drawing.SystemColors.Control;
			this.labelVersionInfo.Location = new System.Drawing.Point(9, 386);
			this.labelVersionInfo.Name = "labelVersionInfo";
			this.labelVersionInfo.Size = new System.Drawing.Size(185, 13);
			this.labelVersionInfo.TabIndex = 7;
			this.labelVersionInfo.Text = "v2.4                      by: George Randall";
			// 
			// textBoxDummy
			// 
			this.textBoxDummy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxDummy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxDummy.Location = new System.Drawing.Point(12, 361);
			this.textBoxDummy.Name = "textBoxDummy";
			this.textBoxDummy.Size = new System.Drawing.Size(100, 22);
			this.textBoxDummy.TabIndex = 8;
			this.textBoxDummy.Visible = false;
			// 
			// buttonRecheck
			// 
			this.buttonRecheck.Location = new System.Drawing.Point(235, 32);
			this.buttonRecheck.Name = "buttonRecheck";
			this.buttonRecheck.Size = new System.Drawing.Size(59, 23);
			this.buttonRecheck.TabIndex = 9;
			this.buttonRecheck.Text = "Recheck";
			this.buttonRecheck.UseVisualStyleBackColor = true;
			this.buttonRecheck.Click += new System.EventHandler(this.buttonRecheck_Click);
			// 
			// buttonPopSnapshot
			// 
			this.buttonPopSnapshot.Enabled = false;
			this.buttonPopSnapshot.Location = new System.Drawing.Point(3, 32);
			this.buttonPopSnapshot.Name = "buttonPopSnapshot";
			this.buttonPopSnapshot.Size = new System.Drawing.Size(110, 23);
			this.buttonPopSnapshot.TabIndex = 10;
			this.buttonPopSnapshot.Text = "Pop Snapshot (0)";
			this.buttonPopSnapshot.UseVisualStyleBackColor = true;
			this.buttonPopSnapshot.Click += new System.EventHandler(this.buttonPopSnapshot_Click);
			// 
			// checkBoxClutter
			// 
			this.checkBoxClutter.AutoSize = true;
			this.checkBoxClutter.Location = new System.Drawing.Point(180, 16);
			this.checkBoxClutter.Name = "checkBoxClutter";
			this.checkBoxClutter.Size = new System.Drawing.Size(77, 17);
			this.checkBoxClutter.TabIndex = 11;
			this.checkBoxClutter.Text = "Possiblities";
			this.checkBoxClutter.UseVisualStyleBackColor = true;
			this.checkBoxClutter.CheckedChanged += new System.EventHandler(this.checkBoxClutter_CheckedChanged);
			// 
			// panelDebugTools
			// 
			this.panelDebugTools.Controls.Add(this.buttonExtract);
			this.panelDebugTools.Controls.Add(this.numericUpDown1);
			this.panelDebugTools.Controls.Add(this.buttonTestCase);
			this.panelDebugTools.Controls.Add(this.labelDebugInfo);
			this.panelDebugTools.Controls.Add(this.buttonRestore);
			this.panelDebugTools.Controls.Add(this.buttonPopSnapshot);
			this.panelDebugTools.Controls.Add(this.buttonRecheck);
			this.panelDebugTools.Controls.Add(this.buttonSnapShot);
			this.panelDebugTools.Location = new System.Drawing.Point(12, 245);
			this.panelDebugTools.Name = "panelDebugTools";
			this.panelDebugTools.Size = new System.Drawing.Size(314, 89);
			this.panelDebugTools.TabIndex = 12;
			this.panelDebugTools.Visible = false;
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(85, 64);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(28, 20);
			this.numericUpDown1.TabIndex = 14;
			// 
			// buttonTestCase
			// 
			this.buttonTestCase.Location = new System.Drawing.Point(3, 61);
			this.buttonTestCase.Name = "buttonTestCase";
			this.buttonTestCase.Size = new System.Drawing.Size(76, 23);
			this.buttonTestCase.TabIndex = 13;
			this.buttonTestCase.Text = "Test Case:";
			this.buttonTestCase.UseVisualStyleBackColor = true;
			this.buttonTestCase.Click += new System.EventHandler(this.buttonTestCase_Click);
			// 
			// labelDebugInfo
			// 
			this.labelDebugInfo.AutoSize = true;
			this.labelDebugInfo.Location = new System.Drawing.Point(119, 8);
			this.labelDebugInfo.Name = "labelDebugInfo";
			this.labelDebugInfo.Size = new System.Drawing.Size(16, 13);
			this.labelDebugInfo.TabIndex = 12;
			this.labelDebugInfo.Text = "   ";
			// 
			// checkBoxDebugTools
			// 
			this.checkBoxDebugTools.AutoSize = true;
			this.checkBoxDebugTools.Location = new System.Drawing.Point(263, 16);
			this.checkBoxDebugTools.Name = "checkBoxDebugTools";
			this.checkBoxDebugTools.Size = new System.Drawing.Size(87, 17);
			this.checkBoxDebugTools.TabIndex = 13;
			this.checkBoxDebugTools.Text = "Debug Tools";
			this.checkBoxDebugTools.UseVisualStyleBackColor = true;
			this.checkBoxDebugTools.CheckedChanged += new System.EventHandler(this.checkBoxDebugTools_CheckedChanged);
			// 
			// buttonExtract
			// 
			this.buttonExtract.Location = new System.Drawing.Point(119, 61);
			this.buttonExtract.Name = "buttonExtract";
			this.buttonExtract.Size = new System.Drawing.Size(110, 23);
			this.buttonExtract.TabIndex = 15;
			this.buttonExtract.Text = "Extract Test Case";
			this.buttonExtract.UseVisualStyleBackColor = true;
			this.buttonExtract.Click += new System.EventHandler(this.buttonExtract_Click);
			// 
			// Form1
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(352, 408);
			this.Controls.Add(this.checkBoxDebugTools);
			this.Controls.Add(this.panelDebugTools);
			this.Controls.Add(this.textBoxDummy);
			this.Controls.Add(this.checkBoxClutter);
			this.Controls.Add(this.labelVersionInfo);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.checkBoxAuto);
			this.Controls.Add(this.buttonReset);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Sudoku Solver";
			this.panelDebugTools.ResumeLayout(false);
			this.panelDebugTools.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonSnapShot;
        private System.Windows.Forms.Button buttonRestore;
        private System.Windows.Forms.CheckBox checkBoxAuto;
		private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelVersionInfo;
        private System.Windows.Forms.TextBox textBoxDummy;
		private System.Windows.Forms.Button buttonRecheck;
		private System.Windows.Forms.Button buttonPopSnapshot;
		private System.Windows.Forms.CheckBox checkBoxClutter;
		private System.Windows.Forms.Panel panelDebugTools;
		private System.Windows.Forms.CheckBox checkBoxDebugTools;
		private System.Windows.Forms.Label labelDebugInfo;
		private System.Windows.Forms.Button buttonTestCase;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.Button buttonExtract;
        
    }
}


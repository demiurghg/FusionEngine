namespace Fusion.Development {
	partial class Dashboard {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.mainMenu = new System.Windows.Forms.MenuStrip();
			this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.buttonRefresh = new System.Windows.Forms.Button();
			this.buttonExit = new System.Windows.Forms.Button();
			this.buttonBuild = new System.Windows.Forms.Button();
			this.consolePanel = new System.Windows.Forms.Panel();
			this.consoleOutput = new System.Windows.Forms.TextBox();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panel1 = new System.Windows.Forms.Panel();
			this.mainTabControl = new System.Windows.Forms.TabControl();
			this.tabConfig = new System.Windows.Forms.TabPage();
			this.propertyGridConfig = new System.Windows.Forms.PropertyGrid();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.listBoxConfig = new System.Windows.Forms.ListBox();
			this.tabShaders = new System.Windows.Forms.TabPage();
			this.panel2 = new System.Windows.Forms.Panel();
			this.buttonShaderEdit = new System.Windows.Forms.Button();
			this.buttonShaderShowReport = new System.Windows.Forms.Button();
			this.buttonShaderCompile = new System.Windows.Forms.Button();
			this.listBoxShaders = new System.Windows.Forms.ListBox();
			this.mainMenu.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.consolePanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.tabConfig.SuspendLayout();
			this.tabShaders.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenu
			// 
			this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameToolStripMenuItem});
			this.mainMenu.Location = new System.Drawing.Point(0, 0);
			this.mainMenu.Name = "mainMenu";
			this.mainMenu.Size = new System.Drawing.Size(584, 24);
			this.mainMenu.TabIndex = 0;
			this.mainMenu.Text = "menuStrip1";
			// 
			// gameToolStripMenuItem
			// 
			this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
			this.gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
			this.gameToolStripMenuItem.Text = "Game";
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.buttonRefresh);
			this.bottomPanel.Controls.Add(this.buttonExit);
			this.bottomPanel.Controls.Add(this.buttonBuild);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = new System.Drawing.Point(0, 675);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(584, 36);
			this.bottomPanel.TabIndex = 1;
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.Location = new System.Drawing.Point(118, 2);
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.Size = new System.Drawing.Size(110, 32);
			this.buttonRefresh.TabIndex = 1;
			this.buttonRefresh.Text = "Refresh";
			this.buttonRefresh.UseVisualStyleBackColor = true;
			// 
			// buttonExit
			// 
			this.buttonExit.Location = new System.Drawing.Point(2, 2);
			this.buttonExit.Name = "buttonExit";
			this.buttonExit.Size = new System.Drawing.Size(110, 32);
			this.buttonExit.TabIndex = 0;
			this.buttonExit.Text = "Exit";
			this.buttonExit.UseVisualStyleBackColor = true;
			this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
			// 
			// buttonBuild
			// 
			this.buttonBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBuild.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.buttonBuild.Location = new System.Drawing.Point(472, 2);
			this.buttonBuild.Name = "buttonBuild";
			this.buttonBuild.Size = new System.Drawing.Size(110, 32);
			this.buttonBuild.TabIndex = 0;
			this.buttonBuild.Text = "Launch";
			this.buttonBuild.UseVisualStyleBackColor = true;
			this.buttonBuild.Click += new System.EventHandler(this.buttonBuild_Click);
			// 
			// consolePanel
			// 
			this.consolePanel.Controls.Add(this.consoleOutput);
			this.consolePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.consolePanel.Location = new System.Drawing.Point(0, 556);
			this.consolePanel.Name = "consolePanel";
			this.consolePanel.Size = new System.Drawing.Size(584, 119);
			this.consolePanel.TabIndex = 2;
			// 
			// consoleOutput
			// 
			this.consoleOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.consoleOutput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.consoleOutput.Location = new System.Drawing.Point(3, 0);
			this.consoleOutput.Multiline = true;
			this.consoleOutput.Name = "consoleOutput";
			this.consoleOutput.ReadOnly = true;
			this.consoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.consoleOutput.Size = new System.Drawing.Size(578, 119);
			this.consoleOutput.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.Control;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = new System.Drawing.Point(0, 553);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(584, 3);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.mainTabControl);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 24);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(584, 529);
			this.panel1.TabIndex = 4;
			// 
			// mainTabControl
			// 
			this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainTabControl.Controls.Add(this.tabConfig);
			this.mainTabControl.Controls.Add(this.tabShaders);
			this.mainTabControl.Location = new System.Drawing.Point(3, 0);
			this.mainTabControl.Name = "mainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = new System.Drawing.Size(580, 530);
			this.mainTabControl.TabIndex = 0;
			// 
			// tabConfig
			// 
			this.tabConfig.Controls.Add(this.propertyGridConfig);
			this.tabConfig.Controls.Add(this.splitter2);
			this.tabConfig.Controls.Add(this.listBoxConfig);
			this.tabConfig.Location = new System.Drawing.Point(4, 22);
			this.tabConfig.Name = "tabConfig";
			this.tabConfig.Padding = new System.Windows.Forms.Padding(0, 2, 2, 1);
			this.tabConfig.Size = new System.Drawing.Size(572, 504);
			this.tabConfig.TabIndex = 0;
			this.tabConfig.Text = "Configuration";
			this.tabConfig.UseVisualStyleBackColor = true;
			// 
			// propertyGridConfig
			// 
			this.propertyGridConfig.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.propertyGridConfig.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridConfig.Location = new System.Drawing.Point(223, 2);
			this.propertyGridConfig.Name = "propertyGridConfig";
			this.propertyGridConfig.Size = new System.Drawing.Size(347, 501);
			this.propertyGridConfig.TabIndex = 1;
			// 
			// splitter2
			// 
			this.splitter2.Location = new System.Drawing.Point(220, 2);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(3, 501);
			this.splitter2.TabIndex = 2;
			this.splitter2.TabStop = false;
			// 
			// listBoxConfig
			// 
			this.listBoxConfig.Dock = System.Windows.Forms.DockStyle.Left;
			this.listBoxConfig.FormattingEnabled = true;
			this.listBoxConfig.IntegralHeight = false;
			this.listBoxConfig.Location = new System.Drawing.Point(0, 2);
			this.listBoxConfig.Name = "listBoxConfig";
			this.listBoxConfig.Size = new System.Drawing.Size(220, 501);
			this.listBoxConfig.TabIndex = 0;
			// 
			// tabShaders
			// 
			this.tabShaders.Controls.Add(this.panel2);
			this.tabShaders.Controls.Add(this.listBoxShaders);
			this.tabShaders.Location = new System.Drawing.Point(4, 22);
			this.tabShaders.Name = "tabShaders";
			this.tabShaders.Padding = new System.Windows.Forms.Padding(3);
			this.tabShaders.Size = new System.Drawing.Size(572, 504);
			this.tabShaders.TabIndex = 1;
			this.tabShaders.Text = "Shaders";
			this.tabShaders.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.buttonShaderEdit);
			this.panel2.Controls.Add(this.buttonShaderShowReport);
			this.panel2.Controls.Add(this.buttonShaderCompile);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel2.Location = new System.Drawing.Point(428, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(141, 498);
			this.panel2.TabIndex = 2;
			// 
			// buttonShaderEdit
			// 
			this.buttonShaderEdit.Location = new System.Drawing.Point(3, 73);
			this.buttonShaderEdit.Name = "buttonShaderEdit";
			this.buttonShaderEdit.Size = new System.Drawing.Size(134, 29);
			this.buttonShaderEdit.TabIndex = 0;
			this.buttonShaderEdit.Text = "Open";
			this.buttonShaderEdit.UseVisualStyleBackColor = true;
			this.buttonShaderEdit.Click += new System.EventHandler(this.buttonShaderEdit_Click);
			// 
			// buttonShaderShowReport
			// 
			this.buttonShaderShowReport.Location = new System.Drawing.Point(3, 38);
			this.buttonShaderShowReport.Name = "buttonShaderShowReport";
			this.buttonShaderShowReport.Size = new System.Drawing.Size(134, 29);
			this.buttonShaderShowReport.TabIndex = 0;
			this.buttonShaderShowReport.Text = "Show Report";
			this.buttonShaderShowReport.UseVisualStyleBackColor = true;
			this.buttonShaderShowReport.Click += new System.EventHandler(this.buttonShaderShowReport_Click);
			// 
			// buttonShaderCompile
			// 
			this.buttonShaderCompile.Location = new System.Drawing.Point(3, 3);
			this.buttonShaderCompile.Name = "buttonShaderCompile";
			this.buttonShaderCompile.Size = new System.Drawing.Size(134, 29);
			this.buttonShaderCompile.TabIndex = 0;
			this.buttonShaderCompile.Text = "Compile";
			this.buttonShaderCompile.UseVisualStyleBackColor = true;
			this.buttonShaderCompile.Click += new System.EventHandler(this.buttonShaderCompile_Click);
			// 
			// listBoxShaders
			// 
			this.listBoxShaders.Dock = System.Windows.Forms.DockStyle.Left;
			this.listBoxShaders.FormattingEnabled = true;
			this.listBoxShaders.IntegralHeight = false;
			this.listBoxShaders.Location = new System.Drawing.Point(3, 3);
			this.listBoxShaders.Name = "listBoxShaders";
			this.listBoxShaders.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBoxShaders.Size = new System.Drawing.Size(152, 498);
			this.listBoxShaders.TabIndex = 1;
			// 
			// Dashboard
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 711);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.consolePanel);
			this.Controls.Add(this.bottomPanel);
			this.Controls.Add(this.mainMenu);
			this.MainMenuStrip = this.mainMenu;
			this.Name = "Dashboard";
			this.Text = "Dashboard";
			this.mainMenu.ResumeLayout(false);
			this.mainMenu.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.consolePanel.ResumeLayout(false);
			this.consolePanel.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.mainTabControl.ResumeLayout(false);
			this.tabConfig.ResumeLayout(false);
			this.tabShaders.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip mainMenu;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.Button buttonBuild;
		private System.Windows.Forms.Button buttonExit;
		private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
		private System.Windows.Forms.Panel consolePanel;
		private System.Windows.Forms.TextBox consoleOutput;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Button buttonRefresh;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TabControl mainTabControl;
		private System.Windows.Forms.TabPage tabConfig;
		private System.Windows.Forms.TabPage tabShaders;
		private System.Windows.Forms.PropertyGrid propertyGridConfig;
		private System.Windows.Forms.ListBox listBoxConfig;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button buttonShaderEdit;
		private System.Windows.Forms.Button buttonShaderShowReport;
		private System.Windows.Forms.Button buttonShaderCompile;
		private System.Windows.Forms.ListBox listBoxShaders;
	}
}
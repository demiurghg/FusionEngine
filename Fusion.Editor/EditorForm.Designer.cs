namespace Fusion.Editor {
	partial class EditorForm {
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.editorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.exitButton = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.updateButton = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.mainPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.mainTreeView = new System.Windows.Forms.TreeView();
			this.label1 = new System.Windows.Forms.Label();
			this.menuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editorToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(491, 24);
			this.menuStrip1.TabIndex = 4;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// editorToolStripMenuItem
			// 
			this.editorToolStripMenuItem.Name = "editorToolStripMenuItem";
			this.editorToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
			this.editorToolStripMenuItem.Text = "Editor";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.exitButton);
			this.panel1.Controls.Add(this.button1);
			this.panel1.Controls.Add(this.updateButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 569);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(491, 35);
			this.panel1.TabIndex = 6;
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.exitButton.Location = new System.Drawing.Point(3, 2);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(75, 30);
			this.exitButton.TabIndex = 7;
			this.exitButton.Text = "Exit";
			this.exitButton.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button1.Location = new System.Drawing.Point(84, 2);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 30);
			this.button1.TabIndex = 6;
			this.button1.Text = "Exit";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.updateButton.Location = new System.Drawing.Point(376, 2);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = new System.Drawing.Size(112, 30);
			this.updateButton.TabIndex = 1;
			this.updateButton.Text = "Update";
			this.updateButton.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.mainPropertyGrid);
			this.panel2.Controls.Add(this.splitter1);
			this.panel2.Controls.Add(this.mainTreeView);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 24);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(4);
			this.panel2.Size = new System.Drawing.Size(491, 542);
			this.panel2.TabIndex = 7;
			// 
			// mainPropertyGrid
			// 
			this.mainPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPropertyGrid.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mainPropertyGrid.Location = new System.Drawing.Point(216, 4);
			this.mainPropertyGrid.Name = "mainPropertyGrid";
			this.mainPropertyGrid.Size = new System.Drawing.Size(271, 534);
			this.mainPropertyGrid.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(212, 4);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(4, 534);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// mainTreeView
			// 
			this.mainTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.mainTreeView.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mainTreeView.Location = new System.Drawing.Point(4, 4);
			this.mainTreeView.Name = "mainTreeView";
			this.mainTreeView.Size = new System.Drawing.Size(208, 534);
			this.mainTreeView.TabIndex = 1;
			this.mainTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.mainTreeView_NodeMouseClick);
			// 
			// label1
			// 
			this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.label1.Location = new System.Drawing.Point(0, 566);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(491, 3);
			this.label1.TabIndex = 9;
			this.label1.Text = "label1";
			// 
			// EditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(491, 604);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panel1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "EditorForm";
			this.Text = "EditorForm";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem editorToolStripMenuItem;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button updateButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.TreeView mainTreeView;
		private System.Windows.Forms.PropertyGrid mainPropertyGrid;
		private System.Windows.Forms.Label label1;
	}
}
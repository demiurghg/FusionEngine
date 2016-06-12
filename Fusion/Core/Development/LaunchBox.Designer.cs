namespace Fusion.Core.Development {
	partial class LaunchBox {
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
			this.button1 = new System.Windows.Forms.Button();
			this.trackObjects = new System.Windows.Forms.CheckBox();
			this.debugDevice = new System.Windows.Forms.CheckBox();
			this.button3 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.startupCommand = new System.Windows.Forms.TextBox();
			this.openConfig = new System.Windows.Forms.Button();
			this.openContent = new System.Windows.Forms.Button();
			this.buildContent = new System.Windows.Forms.Button();
			this.rebuildContent = new System.Windows.Forms.Button();
			this.fullscreen = new System.Windows.Forms.CheckBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.versionLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.stereoMode = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.displayWidth = new System.Windows.Forms.NumericUpDown();
			this.displayHeight = new System.Windows.Forms.NumericUpDown();
			this.openConfigDir = new System.Windows.Forms.Button();
			this.openContentDir = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.displayWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.displayHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.Location = new System.Drawing.Point(178, 483);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(110, 32);
			this.button1.TabIndex = 0;
			this.button1.Text = "Launch";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// trackObjects
			// 
			this.trackObjects.AutoSize = true;
			this.trackObjects.Location = new System.Drawing.Point(114, 240);
			this.trackObjects.Name = "trackObjects";
			this.trackObjects.Size = new System.Drawing.Size(93, 17);
			this.trackObjects.TabIndex = 5;
			this.trackObjects.Text = "Track Objects";
			this.trackObjects.UseVisualStyleBackColor = true;
			// 
			// debugDevice
			// 
			this.debugDevice.AutoSize = true;
			this.debugDevice.Location = new System.Drawing.Point(114, 263);
			this.debugDevice.Name = "debugDevice";
			this.debugDevice.Size = new System.Drawing.Size(162, 17);
			this.debugDevice.TabIndex = 6;
			this.debugDevice.Text = "Use Debug Direct3D Device";
			this.debugDevice.UseVisualStyleBackColor = true;
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button3.Location = new System.Drawing.Point(12, 483);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(83, 32);
			this.button3.TabIndex = 14;
			this.button3.Text = "Exit";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(18, 289);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Startup command";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// startupCommand
			// 
			this.startupCommand.Location = new System.Drawing.Point(114, 286);
			this.startupCommand.Name = "startupCommand";
			this.startupCommand.Size = new System.Drawing.Size(174, 20);
			this.startupCommand.TabIndex = 7;
			// 
			// openConfig
			// 
			this.openConfig.Location = new System.Drawing.Point(114, 312);
			this.openConfig.Name = "openConfig";
			this.openConfig.Size = new System.Drawing.Size(144, 23);
			this.openConfig.TabIndex = 8;
			this.openConfig.Text = "Open Config.ini";
			this.openConfig.UseVisualStyleBackColor = true;
			this.openConfig.Click += new System.EventHandler(this.openConfig_Click);
			// 
			// openContent
			// 
			this.openContent.Location = new System.Drawing.Point(114, 341);
			this.openContent.Name = "openContent";
			this.openContent.Size = new System.Drawing.Size(144, 23);
			this.openContent.TabIndex = 10;
			this.openContent.Text = "Open .content";
			this.openContent.UseVisualStyleBackColor = true;
			this.openContent.Click += new System.EventHandler(this.openContent_Click);
			// 
			// buildContent
			// 
			this.buildContent.Location = new System.Drawing.Point(114, 370);
			this.buildContent.Name = "buildContent";
			this.buildContent.Size = new System.Drawing.Size(144, 23);
			this.buildContent.TabIndex = 12;
			this.buildContent.Text = "Build Content";
			this.buildContent.UseVisualStyleBackColor = true;
			this.buildContent.Click += new System.EventHandler(this.buildContent_Click);
			// 
			// rebuildContent
			// 
			this.rebuildContent.Location = new System.Drawing.Point(114, 399);
			this.rebuildContent.Name = "rebuildContent";
			this.rebuildContent.Size = new System.Drawing.Size(144, 23);
			this.rebuildContent.TabIndex = 13;
			this.rebuildContent.Text = "Rebuild Content";
			this.rebuildContent.UseVisualStyleBackColor = true;
			this.rebuildContent.Click += new System.EventHandler(this.rebuildContent_Click);
			// 
			// fullscreen
			// 
			this.fullscreen.AutoSize = true;
			this.fullscreen.Location = new System.Drawing.Point(114, 217);
			this.fullscreen.Name = "fullscreen";
			this.fullscreen.Size = new System.Drawing.Size(74, 17);
			this.fullscreen.TabIndex = 4;
			this.fullscreen.Text = "Fullscreen";
			this.fullscreen.UseVisualStyleBackColor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Fusion.Properties.Resources.launchHeader;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(300, 150);
			this.pictureBox1.TabIndex = 14;
			this.pictureBox1.TabStop = false;
			// 
			// versionLabel
			// 
			this.versionLabel.Location = new System.Drawing.Point(12, 447);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(276, 23);
			this.versionLabel.TabIndex = 15;
			this.versionLabel.Text = "Fusion Engine v 0.1 (Debug, x64)";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(46, 167);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(62, 13);
			this.label2.TabIndex = 18;
			this.label2.Text = "Display size";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label4.Location = new System.Drawing.Point(12, 470);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(276, 2);
			this.label4.TabIndex = 19;
			// 
			// stereoMode
			// 
			this.stereoMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.stereoMode.FormattingEnabled = true;
			this.stereoMode.Location = new System.Drawing.Point(114, 190);
			this.stereoMode.Name = "stereoMode";
			this.stereoMode.Size = new System.Drawing.Size(174, 21);
			this.stereoMode.TabIndex = 3;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(41, 193);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(67, 13);
			this.label5.TabIndex = 21;
			this.label5.Text = "Stereo mode";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// displayWidth
			// 
			this.displayWidth.Location = new System.Drawing.Point(114, 164);
			this.displayWidth.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.displayWidth.Minimum = new decimal(new int[] {
            320,
            0,
            0,
            0});
			this.displayWidth.Name = "displayWidth";
			this.displayWidth.Size = new System.Drawing.Size(84, 20);
			this.displayWidth.TabIndex = 1;
			this.displayWidth.Value = new decimal(new int[] {
            320,
            0,
            0,
            0});
			// 
			// displayHeight
			// 
			this.displayHeight.Location = new System.Drawing.Point(204, 164);
			this.displayHeight.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.displayHeight.Minimum = new decimal(new int[] {
            240,
            0,
            0,
            0});
			this.displayHeight.Name = "displayHeight";
			this.displayHeight.Size = new System.Drawing.Size(84, 20);
			this.displayHeight.TabIndex = 2;
			this.displayHeight.Value = new decimal(new int[] {
            240,
            0,
            0,
            0});
			// 
			// openConfigDir
			// 
			this.openConfigDir.Location = new System.Drawing.Point(264, 312);
			this.openConfigDir.Name = "openConfigDir";
			this.openConfigDir.Size = new System.Drawing.Size(24, 23);
			this.openConfigDir.TabIndex = 9;
			this.openConfigDir.Text = "...";
			this.openConfigDir.UseVisualStyleBackColor = true;
			this.openConfigDir.Click += new System.EventHandler(this.openConfigDir_Click);
			// 
			// openContentDir
			// 
			this.openContentDir.Location = new System.Drawing.Point(264, 341);
			this.openContentDir.Name = "openContentDir";
			this.openContentDir.Size = new System.Drawing.Size(24, 23);
			this.openContentDir.TabIndex = 11;
			this.openContentDir.Text = "...";
			this.openContentDir.UseVisualStyleBackColor = true;
			this.openContentDir.Click += new System.EventHandler(this.openContentDir_Click);
			// 
			// LaunchBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(300, 527);
			this.Controls.Add(this.openContentDir);
			this.Controls.Add(this.openConfigDir);
			this.Controls.Add(this.displayHeight);
			this.Controls.Add(this.displayWidth);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.stereoMode);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.fullscreen);
			this.Controls.Add(this.rebuildContent);
			this.Controls.Add(this.buildContent);
			this.Controls.Add(this.openContent);
			this.Controls.Add(this.openConfig);
			this.Controls.Add(this.startupCommand);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.debugDevice);
			this.Controls.Add(this.trackObjects);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LaunchBox";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LaunchBox";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.displayWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.displayHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.CheckBox trackObjects;
		private System.Windows.Forms.CheckBox debugDevice;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox startupCommand;
		private System.Windows.Forms.Button openConfig;
		private System.Windows.Forms.Button openContent;
		private System.Windows.Forms.Button buildContent;
		private System.Windows.Forms.Button rebuildContent;
		private System.Windows.Forms.CheckBox fullscreen;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox stereoMode;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown displayWidth;
		private System.Windows.Forms.NumericUpDown displayHeight;
		private System.Windows.Forms.Button openConfigDir;
		private System.Windows.Forms.Button openContentDir;
	}
}
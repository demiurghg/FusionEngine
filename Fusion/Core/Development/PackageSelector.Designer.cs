namespace Fusion.Core.Development {
	partial class PackageSelector {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.searchFilter = new System.Windows.Forms.TextBox();
			this.packageList = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// searchFilter
			// 
			this.searchFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchFilter.Location = new System.Drawing.Point(0, 3);
			this.searchFilter.Name = "searchFilter";
			this.searchFilter.Size = new System.Drawing.Size(211, 20);
			this.searchFilter.TabIndex = 1;
			// 
			// packageList
			// 
			this.packageList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.packageList.FormattingEnabled = true;
			this.packageList.IntegralHeight = false;
			this.packageList.Items.AddRange(new object[] {
            "FusionEngine",
            "FusionEngine_Old",
            "FusionFramework",
            "IBLBaker",
            "ini-parser",
            "KopiLua",
            "LevelEditor",
            "medicine-bloodflow",
            "MonoGame",
            "NLua",
            "Papers",
            "paradox",
            "pulse-project",
            "ShooterDemo",
            "sirius",
            "SpaceMarines",
            "SpaceTDS",
            "SSAO",
            "Subkrieg",
            "vkQuake"});
			this.packageList.Location = new System.Drawing.Point(0, 29);
			this.packageList.Name = "packageList";
			this.packageList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.packageList.Size = new System.Drawing.Size(211, 385);
			this.packageList.TabIndex = 2;
			// 
			// PackageSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.packageList);
			this.Controls.Add(this.searchFilter);
			this.Name = "PackageSelector";
			this.Size = new System.Drawing.Size(211, 414);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox searchFilter;
		private System.Windows.Forms.ListBox packageList;
	}
}

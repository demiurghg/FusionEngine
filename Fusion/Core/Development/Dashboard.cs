using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Engine.Common;

namespace Fusion.Core.Development {
	public partial class Dashboard : Form {
		
		public readonly Game Game;

		
		public Dashboard ( Game game, string configPath, bool dialogMode )
		{
			this.Game	=	game;
			InitializeComponent();

			SetupConfigPage();
			this.Icon	=	Fusion.Properties.Resources.fusionIconGrayscale;


			if (dialogMode) {
				this.StartPosition = FormStartPosition.CenterScreen;
			} else {
				buttonBuild.Text = "Build";
				buttonExit.Text = "Close";
			}

			foreach ( var tab in mainTabControl.TabPages.Cast<TabPage>() ) {
				tab.Padding = new Padding(0,2,2,1);
			}

			Log.AddListener( new TextBoxLogListener( consoleOutput ) );
		}




		void RefreshSelector ()
		{
		}


		private void buttonExit_Click ( object sender, EventArgs e )
		{
			Close();
		}


		private void buttonBuild_Click ( object sender, EventArgs e )
		{
			this.DialogResult	=	DialogResult.OK;
			this.Close();
		}

	}
}

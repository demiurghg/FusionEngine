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
using Fusion.Engine.Graphics;
using System.IO;
using System.Diagnostics;


namespace Fusion.Core.Development {

	internal partial class LaunchBoxForm : Form {

		readonly Game game;

		string configPath;
		string configName;


		public LaunchBoxForm ( Game game, string config )
		{
			this.game	=	game;
			configName	=	config;
			configPath	=	game.UserStorage.GetFullPath(config);


			InitializeComponent();
			this.Icon	=	Fusion.Properties.Resources.fusionIconGrayscale;


			this.Text	=	game.GameTitle;

			UpdateControls();
		}



		void UpdateControls ()
		{

			//	version :
			versionLabel.Text	=	game.GetReleaseInfo();

			//	stereo mode :
			stereoMode.Items.Clear();
			stereoMode.Items.AddRange( Enum.GetValues(typeof(StereoMode)).Cast<object>().ToArray() );
			stereoMode.SelectedItem = game.RenderSystem.StereoMode;

			//	display mode :
			displayWidth.Value	=	game.RenderSystem.Width;
			displayHeight.Value	=	game.RenderSystem.Height;

			//	fullscreen
			fullscreen.Checked	=	game.RenderSystem.Fullscreen;

			//	track objects
			trackObjects.Checked	=	game.TrackObjects;

			//	use debug device :
			debugDevice.Checked	=	game.RenderSystem.UseDebugDevice;
		}



		private void button1_Click ( object sender, EventArgs e )
		{
			// stereo mode :
			game.RenderSystem.StereoMode	=	(StereoMode)stereoMode.SelectedItem;

			//	displya mode :
			game.RenderSystem.Width	=	(int)displayWidth.Value;
			game.RenderSystem.Height	=	(int)displayHeight.Value;

			//	fullscreen
			game.RenderSystem.Fullscreen	=	fullscreen.Checked;

			//	track objects
			game.TrackObjects	=	trackObjects.Checked;

			//	use debug device :
			game.RenderSystem.UseDebugDevice	=	debugDevice.Checked;

			if (!string.IsNullOrWhiteSpace(startupCommand.Text)) {
				game.Invoker.Push( startupCommand.Text );
			}

			this.DialogResult	=	DialogResult.OK;
			this.Close();
		}



		private void button3_Click ( object sender, EventArgs e )
		{
			this.Close();
		}



		void ShellExecute ( string path, bool wait = false )
		{
			ProcessStartInfo psi = new ProcessStartInfo(path);
			psi.UseShellExecute = true;
			var proc = Process.Start(psi);
			if (wait) {
				proc.WaitForExit();
			}
		}



		private void openConfig_Click ( object sender, EventArgs e )
		{
			ShellExecute( configPath, true );
			game.Config.Load(configName);
			UpdateControls();
		}



		private void openConfigDir_Click ( object sender, EventArgs e )
		{
			ShellExecute( Path.GetDirectoryName(configPath) );
		}



		private void openContent_Click ( object sender, EventArgs e )
		{
			var file = (string)game.Invoker.PushAndExecute("contentFile");
			ShellExecute(file);
		}



		private void openContentDir_Click ( object sender, EventArgs e )
		{
			var file = (string)game.Invoker.PushAndExecute("contentFile");
			ShellExecute(Path.GetDirectoryName(file));
		}



		private void buildContent_Click ( object sender, EventArgs e )
		{
			game.Invoker.PushAndExecute("contentBuild");
		}



		private void rebuildContent_Click ( object sender, EventArgs e )
		{
			game.Invoker.PushAndExecute("contentBuild /force");
		}
	}
}

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

	public partial class LaunchBox : Form {

		readonly Game game;

		public static bool Show ( Game game, string config )
		{
			var form = new LaunchBox( game, config );

			var dr = form.ShowDialog();

			if (dr==DialogResult.OK) {
				return true;
			} else {
				return false;
			}
		} 


		string configPath;
		string configName;


		private LaunchBox ( Game game, string config )
		{
			this.game	=	game;
			configName	=	config;
			configPath	=	game.UserStorage.GetFullPath(config);

			InitializeComponent();

			this.Text	=	game.GameTitle;

			UpdateControls();
		}



		void UpdateControls ()
		{

			//	version :
			versionLabel.Text	=	game.GetReleaseInfo();

			//	stereo mode :
			stereoMode.Items.AddRange( Enum.GetValues(typeof(StereoMode)).Cast<object>().ToArray() );
			stereoMode.SelectedItem = game.RenderSystem.Config.StereoMode;

			//	display mode :
			displayWidth.Value	=	game.RenderSystem.Config.Width;
			displayHeight.Value	=	game.RenderSystem.Config.Height;

			//	fullscreen
			fullscreen.Checked	=	game.RenderSystem.Config.Fullscreen;

			//	track objects
			trackObjects.Checked	=	game.TrackObjects;

			//	use debug device :
			debugDevice.Checked	=	game.RenderSystem.Config.UseDebugDevice;
		}



		private void button1_Click ( object sender, EventArgs e )
		{
			// stereo mode :
			game.RenderSystem.Config.StereoMode	=	(StereoMode)stereoMode.SelectedItem;

			//	displya mode :
			game.RenderSystem.Config.Width	=	(int)displayWidth.Value;
			game.RenderSystem.Config.Height	=	(int)displayHeight.Value;

			//	fullscreen
			game.RenderSystem.Config.Fullscreen	=	fullscreen.Checked;

			//	track objects
			game.TrackObjects	=	trackObjects.Checked;

			//	use debug device :
			game.RenderSystem.Config.UseDebugDevice	=	debugDevice.Checked;

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
			game.LoadConfiguration(configName);
			UpdateControls();
		}



		private void button2_Click ( object sender, EventArgs e )
		{
			ShellExecute( Path.GetDirectoryName(configPath) );
		}



		private void openContent_Click ( object sender, EventArgs e )
		{
			var file = (string)game.Invoker.PushAndExecute("contentFile");
			ShellExecute(file);
		}



		private void button4_Click ( object sender, EventArgs e )
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using Fusion.Engine.Common;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;


namespace Fusion.Core.Development {

	public partial class Dashboard : Form {

		
		class GeneralConfiguration {
			
			private readonly Game game;

			public GeneralConfiguration ( Game game )
			{
				this.game	=	game;
			}

			public override string ToString ()
			{
				return "(General)";
			}
			
			[Category("Display")]
			public int DisplayWidth { 
				get { return game.RenderSystem.Width; }
				set { game.RenderSystem.Width = value; }
			}
			
			[Category("Display")]
			public int DisplayHeight { 
				get { return game.RenderSystem.Height; }
				set { game.RenderSystem.Height = value; }
			}
			
			[Category("Display")]
			public bool Fullscreen {
				get { return game.RenderSystem.Fullscreen; }
				set { game.RenderSystem.Fullscreen = value; }
			}
			
			[Category("Graphics")]
			public bool UseDebugDevice {
				get { return game.RenderSystem.UseDebugDevice; }
				set { game.RenderSystem.UseDebugDevice = value; }
			}
			
			[Category("System")]
			public bool TrackObjects {
				get { return game.TrackObjects; }
				set { game.TrackObjects = value; }
			}
			
			[Category("Stereo")]
			public StereoMode StereoMode {
				get { return game.RenderSystem.StereoMode; }
				set { game.RenderSystem.StereoMode = value; }
			}
			
			[Category("Stereo")]
			public InterlacingMode InterlacingMode {
				get { return game.RenderSystem.InterlacingMode; }
				set { game.RenderSystem.InterlacingMode = value; }
			}
		}

		

		void SetupConfigPage ()
		{
			listBoxConfig.Items.Add( new GeneralConfiguration( Game ) );

			listBoxConfig.Items.AddRange( Game.Config.TargetObjects
				.Select( kvp => kvp.Value )
				.OrderBy( val => val.ToString() )
				.ToArray()
				);

			listBoxConfig.SelectedIndexChanged += listBoxConfig_SelectedIndexChanged;

			listBoxConfig.SelectedIndex = 0;
		}



		void listBoxConfig_SelectedIndexChanged ( object sender, EventArgs e )
		{
			propertyGridConfig.SelectedObjects = listBoxConfig.SelectedItems.Cast<object>().ToArray();
		}

	}
}

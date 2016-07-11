using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Fusion;
using Fusion.Build;
using Fusion.Engine.Common;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Core.Development;

namespace GISTest {




	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			Log.AddListener( new ColoredLogListener() );
			Log.AddListener( new LogRecorder() );
			Log.VerbosityLevel	=	LogMessageType.Verbose;

			//
			//	Build content on startup.
			//	Remove this line in release code.
			//
			Builder.Options.InputDirectory	=	@"..\..\..\Content";
			Builder.Options.TempDirectory	=	@"..\..\..\Temp";
			Builder.Options.OutputDirectory	=	@"Content";
			Builder.SafeBuild();
	
			//
			//	Run engine.
			//
			using ( var engine = new Game("TestGame") ) {

				engine.GameServer		=	new CustomGameServer(engine);
				engine.GameClient		=	new CustomGameClient(engine);
				engine.GameInterface	=	new CustomGameInterface(engine);

				//	load configuration:
				engine.LoadConfiguration("Config.ini");

				//	apply configuration here:
				engine.RenderSystem.UseDebugDevice =	false;
				engine.TrackObjects		=	false;
				engine.GameTitle		=	"Graph";

				//	apply command-line options here:
				//	...
				if (!LaunchBox.Show(engine, "Config.ini")) {
					return 0;
				}

				//	run:
				engine.Run();

				//	save configuration:
				engine.SaveConfiguration("Config.ini"); 				
			}

			return 0;
		}
	}
}

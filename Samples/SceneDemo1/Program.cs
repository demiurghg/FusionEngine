using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Build;
using Fusion.Engine.Common;
using Fusion.Core.Shell;
using Fusion.Core.Utils;

namespace SceneDemo1 {

	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			// 	colored console output :
			Log.AddListener( new ColoredLogListener() );

			//	output for in-game console :
			Log.AddListener( new LogRecorder() );

			//	set verbosity :
			Log.VerbosityLevel = LogMessageType.Verbose;


			//
			//	Build content on startup :
			//
			Builder.Options.InputDirectory	=	@"..\..\..\Content";
			Builder.Options.TempDirectory	=	@"..\..\..\Temp";
			Builder.Options.OutputDirectory	=	@"Content";
			Builder.SafeBuild();


			//
			//	Run game :
			//
			using (var game = new Game( "SceneDemo1" )) {

				//	create SV, CL and UI instances :
				game.GameServer = new SceneDemo1GameServer( game );
				game.GameClient = new SceneDemo1GameClient( game );
				game.GameInterface = new SceneDemo1GameInterface( game );

				//	load configuration.
				//	first run will cause warning, 
				//	because configuration file still does not exist.
				game.LoadConfiguration( "Config.ini" );

				//	enable and disable debug direct3d device :
				game.RenderSystem.Config.UseDebugDevice = false;

				//	enable and disable object tracking :
				game.TrackObjects = true;

				//	set game title :
				game.GameTitle = "SceneDemo1";

				//	apply command-line options here:
				//	...

				//	run:
				game.Run();

				//	save configuration :
				game.SaveConfiguration( "Config.ini" );
			}

			return 0;
		}
	}
}

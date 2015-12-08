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
			if (args.Contains("-build")) {
				Builder.SafeBuild( @"..\..\..\Content", @"Content", @"..\..\..\Temp", null, false );
			}


			//
			//	Run game :
			//
			using (var engine = new GameEngine( "SceneDemo1" )) {

				//	create SV, CL and UI instances :
				engine.GameServer = new SceneDemo1GameServer( engine );
				engine.GameClient = new SceneDemo1GameClient( engine );
				engine.GameInterface = new SceneDemo1GameInterface( engine );

				//	load configuration.
				//	first run will cause warning, 
				//	because configuration file still does not exist.
				engine.LoadConfiguration( "Config.ini" );

				//	enable and disable debug direct3d device :
				engine.GraphicsEngine.Config.UseDebugDevice = false;

				//	enable and disable object tracking :
				engine.TrackObjects = true;

				//	set game title :
				engine.GameTitle = "SceneDemo1";

				//	apply command-line options here:
				//	...

				//	run:
				engine.Run();

				//	save configuration :
				engine.SaveConfiguration( "Config.ini" );
			}

			return 0;
		}
	}
}

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

namespace $safeprojectname$ {

	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			// 	colored console output :
			Trace.Listeners.Add( new ColoredTraceListener() );
			
			//	output for in-game console :
			Trace.Listeners.Add( new TraceRecorder() );

			//
			//	Build content on startup :
			//
			try {
				Builder.Build( @"..\..\..\Content", @"Content", @"..\..\..\Temp", false );
			} catch ( Exception e ) {
				Log.Error( e.Message );
				return 1;
			}


			//
			//	Run game :
			//
			using ( var engine = new GameEngine() ) {

				//	create SV, CL and UI instances :
				engine.GameServer		=	new CustomGameServer(engine);
				engine.GameClient		=	new CustomGameClient(engine);
				engine.GameInterface	=	new CustomGameInterface(engine);

				//	load configuration.
				//	first run will cause warning, 
				//	because configuration file still does not exist.
				engine.LoadConfiguration("Config.ini");

				//	apply command-line options here:
				//	...

				//	run:
				engine.Run();
				
				//	save configuration :
				engine.SaveConfiguration("Config.ini"); 				
			}

			return 0;
		}
	}
}

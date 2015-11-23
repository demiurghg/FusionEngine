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
using Fusion.Core;
using Fusion.Core.Mathematics;

namespace TestGame2 {

	enum Blah {
		BlahA,
		BlahB,
	}

	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );
			Trace.Listeners.Add( new TraceRecorder() );

			//
			//	Build content on startup :
			//
			Builder.SafeBuild( @"..\..\..\Content", @"Content", @"..\..\..\Temp", false );
	
			Log.Warning( StringConverter.ToString( Color.Red ) );




			//
			//	Parse command line :
			//
			using ( var engine = new GameEngine() ) {

				engine.GameServer		=	new CustomGameServer(engine);
				engine.GameClient		=	new CustomGameClient(engine);
				engine.GameInterface	=	new CustomGameInterface(engine);

				engine.LoadConfiguration("Config.ini");

				engine.GraphicsEngine.Config.UseDebugDevice = false;
				engine.TrackObjects		=	true;
				engine.GameTitle		=	"Test Game 2";


				//	apply command-line options here:
				//	...

				engine.Run();
				
				engine.SaveConfiguration("Config.ini"); 				
			}

			return 0;
		}
	}
}

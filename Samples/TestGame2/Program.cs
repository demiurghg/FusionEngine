﻿using System;
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

namespace TestGame2 {




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
			Builder.SafeBuild( @"..\..\..\Content", @"Content", @"..\..\..\Temp", null, false );
	
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
				engine.RenderSystem.Config.UseDebugDevice =	false;
				engine.TrackObjects		=	false;
				engine.GameTitle		=	"Test Game 2";

				//	apply command-line options here:
				//	...

				//	run:
				engine.Run();
				
				//	save configuration:
				engine.SaveConfiguration("Config.ini"); 				
			}

			return 0;
		}
	}
}

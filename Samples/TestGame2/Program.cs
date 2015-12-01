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

namespace TestGame2 {

	enum Blah {
		BlahA,
		BlahB,
	}

	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			Log.AddListener( new ColoredLogListener() );
			Log.AddListener( new LogRecorder() );
			Log.VerbosityLevel	=	LogMessageType.Verbose;

			//
			//	Build content on startup :
			//
			Builder.SafeBuild( @"..\..\..\Content", @"Content", @"..\..\..\Temp", false );
	
			//
			//	Parse command line :
			//
			using ( var engine = new GameEngine("TestGame") ) {

				engine.GameServer		=	new CustomGameServer(engine);
				engine.GameClient		=	new CustomGameClient(engine);
				engine.GameInterface	=	new CustomGameInterface(engine);

				engine.LoadConfiguration("Config.ini");

				engine.GraphicsEngine.Config.UseDebugDevice = false;
				engine.TrackObjects		=	true;
				engine.GameTitle		=	"Test Game 2";


				//	apply command-line options here:
				//	...

				/*var mtrl = Material.CreateFromTexture("walls/wall01.tga");

				File.WriteAllText(@"C:\GitHub\Material.ini", mtrl.ToIni());

				var mtrl2 = Material.FromIni( File.ReadAllText(@"C:\GitHub\Material.ini") );
				mtrl2.Options = MaterialOptions.Terrain;
				File.WriteAllText(@"C:\GitHub\Material2.ini", mtrl2.ToIni());*/


				engine.Run();
				
				engine.SaveConfiguration("Config.ini"); 				
			}

			return 0;
		}
	}
}

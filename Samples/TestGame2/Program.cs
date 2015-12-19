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

	/// <summary>
	/// Program parameters.
	/// </summary>
	class Options {
		
		[CommandLineParser.Name("width")]
		public int Width { get; set; }

		[CommandLineParser.Name("height")]
		public int Height { get; set; }

		[CommandLineParser.Name("fullscr")]
		public bool Fullscreen { get; set; }

		[CommandLineParser.Name("stereo")]
		public StereoMode StereoMode { get; set; }

		[CommandLineParser.Name("debug")]
		public bool DebugDevice { get; set; }

		[CommandLineParser.Name("dedicated")]
		public bool Dedicated { get; set; }

		[CommandLineParser.Name("command")]
		public string Command { get; set; }

		public void Apply ( Game game )
		{
			if (Width>0) {
				game.RenderSystem.Config.Width	=	Width;
			}
			if (Height>0) {
				game.RenderSystem.Config.Height	=	Height;
			}
			if (Fullscreen) {
				game.RenderSystem.Config.Fullscreen	=	Fullscreen;
			}
			if (DebugDevice) {
				game.RenderSystem.Config.UseDebugDevice =	DebugDevice;
			}
			if (StereoMode!=StereoMode.Disabled) {
				game.RenderSystem.Config.StereoMode	=	StereoMode;
			}
			if (!string.IsNullOrWhiteSpace(Command)) {
				game.Invoker.Push( Command );
			}
		}
	}



	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			Log.AddListener( new ColoredLogListener() );
			Log.AddListener( new LogRecorder() );
			Log.VerbosityLevel	=	LogMessageType.Verbose;

			//
			//	Parse command line arguments.
			//
			var options	=	new Options();
			var parser	=	new CommandLineParser(options);
			parser.ParseCommandLine(args);

			//
			//	Build content on startup.
			//	Remove this line in release code.
			//
			Builder.SafeBuild( @"..\..\..\Content", @"Content", @"..\..\..\Temp", null, false );
	
			//
			//	Run engine.
			//
			using ( var engine = new Game("TestGame") ) {

				if (options.Dedicated) {
					engine.GameServer		=	new CustomGameServer(engine);
					engine.GameClient		=	null;
					engine.GameInterface	=	null;
				} else {
					engine.GameServer		=	new CustomGameServer(engine);
					engine.GameClient		=	new CustomGameClient(engine);
					engine.GameInterface	=	new CustomGameInterface(engine);
				}

				//	load configuration:
				engine.LoadConfiguration("Config.ini");

				//	apply configuration here:
				engine.RenderSystem.Config.UseDebugDevice =	false;
				engine.TrackObjects		=	true;
				engine.GameTitle		=	"Test Game 2";

				//	apply command-line options here:
				options.Apply( engine );

				//	run:
				engine.Run();
				
				//	save configuration:
				engine.SaveConfiguration("Config.ini"); 				
			}

			return 0;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion;
using Fusion.Core;
using Fusion.Core.Shell;
using Fusion.Core.Content;
using Fusion.Core.IniParser;
using Fusion.Core.Utils;
using Fusion.Core.Extensions;
using Fusion.Build;
using Fusion.Build.Processors;

namespace FMap {
	class FMap {

		class Parameters {

			[CommandLineParser.Name("input", "Input file name")]
			[CommandLineParser.Required()]
			public string InputFile { get; set; }

			[CommandLineParser.Name("output", "Output file name")]
			[CommandLineParser.Required()]
			public string OutputFile { get; set; }
			
		}


		[STAThread]
		static int Main ( string[] args )
		{
			Thread.CurrentThread.CurrentCulture	=	System.Globalization.CultureInfo.InvariantCulture;
			Log.AddListener( new StdLogListener() );

			var p = new Parameters();

			var parser = new CommandLineParser( p );
			
			if (!parser.ParseCommandLine( args )) {
				return 1;
			}


			return 0;
		}
	}
}

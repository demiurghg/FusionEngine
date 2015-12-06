using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;
using Fusion.Core.Utils;

namespace Fusion.Build {

	
	/// <summary>
	/// Clears trace recorder.
	/// </summary>
	[Command("contentBuild", CommandAffinity.Default)]
	public class BuildContentCmd : NoRollbackCommand {

		[CommandLineParser.Required]
		[CommandLineParser.Name("in")]
		public string InputDirectory { get; set; }
			
		[CommandLineParser.Required]
		[CommandLineParser.Name("out")]
		public string OutputDirectory { get; set; }
			
		[CommandLineParser.Name("temp")]
		public string TempDirectory { get; set; }
			
		[CommandLineParser.Name("force")]
		public bool ForceRebuild { get; set; }
			
		[CommandLineParser.Name("clean")]
		public string CleanPattern { get; set; }
			
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public BuildContentCmd ( Invoker invoker ) : base( invoker )
		{		
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Execute ()
		{
			if (string.IsNullOrWhiteSpace(InputDirectory)) {
				throw new ArgumentException("InputDirectory", "input directory must be specified");
			}
			if (string.IsNullOrWhiteSpace(OutputDirectory)) {
				throw new ArgumentException("InputDirectory", "input directory must be specified");
			}

			var task = new Task( BuildTask );
			task.Start();
		}


		void BuildTask ()
		{
			Builder.SafeBuild( InputDirectory, OutputDirectory, TempDirectory, CleanPattern, ForceRebuild );
			Invoker.GameEngine.Reload();
		}
	}
}

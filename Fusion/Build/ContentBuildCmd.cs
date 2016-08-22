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
	public class ContentBuildCmd : NoRollbackCommand {

		[CommandLineParser.Name("force", "Force to rebuild entire content")]
		public bool ForceRebuild { get; set; }
			
		[CommandLineParser.Name("clean", "Clean all files that matches provided pattern")]
		public string CleanPattern { get; set; }
			
		[CommandLineParser.Name("async", "Build content in separate thread")]
		public bool ASync { get; set; }
			
		[CommandLineParser.Name("file", "Files to rebuild")]
		public List<string> Files { get; set; }
			
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public ContentBuildCmd ( Invoker invoker ) : base( invoker )
		{		
			Files = new List<string>();
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Execute ()
		{
			if (ASync) {
				var task = new Task( BuildTask );
				task.Start();
			} else {
				BuildTask();
			}
		}


		void BuildTask ()
		{
			Builder.SafeBuild( ForceRebuild, CleanPattern, Files );
			if (Game.IsInitialized) {
				Invoker.Game.Reload();
			}
		}
	}
}

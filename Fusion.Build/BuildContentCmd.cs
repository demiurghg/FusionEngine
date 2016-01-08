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

		[CommandLineParser.Name("force")]
		public bool ForceRebuild { get; set; }
			
		[CommandLineParser.Name("clean")]
		public string CleanPattern { get; set; }
			
		[CommandLineParser.Name("async")]
		public bool ASync { get; set; }
			
		
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
			if (ASync) {
				var task = new Task( BuildTask );
				task.Start();
			} else {
				BuildTask();
			}
		}


		void BuildTask ()
		{
			Builder.SafeBuild( ForceRebuild, CleanPattern );
			if (Game.IsInitialized) {
				Invoker.Game.Reload();
			}
		}
	}
}

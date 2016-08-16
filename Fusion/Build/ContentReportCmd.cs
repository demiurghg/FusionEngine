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
	[Command("contentReport", CommandAffinity.Default)]
	public class ContentReportCmd : NoRollbackCommand {

		[CommandLineParser.Required()]
		[CommandLineParser.Name("keyPath")]
		public string KeyPath { get; set; }
			
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public ContentReportCmd ( Invoker invoker ) : base( invoker )
		{		
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Execute ()
		{
			Builder.OpenReport( KeyPath );
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;
using Fusion.Core.Utils;

namespace Fusion.Build {

	
	/// <summary>
	/// Returns full path to content file.
	/// </summary>
	[Command("contentFile", CommandAffinity.Default)]
	public class BuildContentFileCmd : NoRollbackCommand {
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public BuildContentFileCmd ( Invoker invoker ) : base( invoker )
		{		
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Execute ()
		{
			Result = Builder.Options.ContentIniFile;
		}
	}
}

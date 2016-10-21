using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;
using Fusion.Core.Utils;

namespace Fusion.Engine.Tools {

	
	/// <summary>
	/// Clears trace recorder.
	/// </summary>
	[Command("clear", CommandAffinity.Default)]
	public class ClearCmd : NoRollbackCommand {
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public ClearCmd ( Invoker invoker ) : base( invoker )
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Execute ()
		{
			LogRecorder.Clear();
		}
	}
}

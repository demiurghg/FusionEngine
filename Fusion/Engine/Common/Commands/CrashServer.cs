using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common.Commands {

	[Command("crashServer", CommandAffinity.Server)]
	public class CrashServer : NoRollbackCommand {

		static public bool crashRequested = false;


		public static void CrashTest ()
		{
			if (crashRequested) {
				crashRequested = false;
				throw new Exception("SERVER CRASHTEST");
			}
		}

			
		public CrashServer ( Invoker invoker ) : base(invoker) 
		{
		}

		public override void Execute ()
		{
			crashRequested	=	true;
		}
	}
}

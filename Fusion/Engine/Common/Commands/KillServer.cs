using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common.Commands {

	[Command("killServer", CommandAffinity.Default)]
	public class KillServerCommand : NoRollbackCommand {
			
		public KillServerCommand ( Invoker invoker ) : base(invoker) 
		{
		}

		public override void Execute ()
		{
			Invoker.Game.KillServer();
		}
	}
}

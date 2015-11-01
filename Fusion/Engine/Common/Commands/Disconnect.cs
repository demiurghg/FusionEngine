using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common.Commands {

	[Command("disconnect", CommandAffinity.Default)]
	public class DisconnectCommand : NoRollbackCommand {

		public DisconnectCommand ( Invoker invoker ) : base(invoker) 
		{
		}

		public override void Execute ()
		{
			Invoker.GameEngine.Disconnect();
		}
	}
}

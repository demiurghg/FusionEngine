using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common.Commands {

	[Command("showModules", CommandAffinity.Default)]
	public class ShowModules : NoRollbackCommand {

		public ShowModules ( Invoker invoker ) : base(invoker) 
		{
		}

		public override void Execute ()
		{
			GameModule.PrintModuleNames();
		}
	}
}

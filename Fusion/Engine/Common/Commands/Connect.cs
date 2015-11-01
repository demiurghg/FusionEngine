using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common.Commands {

	[Command("connect", CommandAffinity.Default)]
	public class ConnectCommand : NoRollbackCommand {

		[CommandLineParser.Required()]
		[CommandLineParser.Name("host")]
		public string Host { get; set; }

		[CommandLineParser.Required()]
		[CommandLineParser.Name("port")]
		public int Port { get; set; }
			
		public ConnectCommand ( Invoker invoker ) : base(invoker) 
		{
		}

		public override void Execute ()
		{
			Invoker.GameEngine.Connect( Host, Port );
		}
	}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Common.Commands {

	[Command("disconnect", CommandAffinity.Default)]
	public class DisconnectCommand : NoRollbackCommand {


		/// <summary>
		/// 
		/// </summary>
		[CommandLineParser.Name("msg")]
		public string Message { get; set; }

		public DisconnectCommand ( Invoker invoker ) : base(invoker) 
		{
			Message = "";
		}

		public override void Execute ()
		{
			Invoker.GameEngine.Disconnect(Message);
		}
	}
}

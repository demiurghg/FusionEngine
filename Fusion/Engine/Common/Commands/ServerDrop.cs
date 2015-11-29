using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;
using Fusion.Engine.Common;

namespace Fusion.Engine.Common.Commands {

	[Command("drop", CommandAffinity.Server)]
	internal class ServerDrop : NoRollbackCommand {


		[CommandLineParser.Required]
		[CommandLineParser.Name("client", "client IP or name")]
		public string Client { get; set; }

		[CommandLineParser.Name("reason", "reason of drop")]
		public string Reason { get; set; }


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="ivoker"></param>
		public ServerDrop ( Invoker invoker ) : base(invoker)
		{
			Reason	=	"";
		}


		/// <summary>
		/// Executes
		/// </summary>
		public override void Execute ()
		{
			if (Invoker.GameEngine.GameServer.IsAlive) {
				//Invoker.GameEngine.GameServer.Drop( Client, Reason );
			} else {
				Log.Warning("Server is not running");
			}
		}
	}
}

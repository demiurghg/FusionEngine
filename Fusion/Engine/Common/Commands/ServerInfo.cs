using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;
using Fusion.Engine.Common;

namespace Fusion.Engine.Common.Commands {

	[Command("serverInfo", CommandAffinity.Server)]
	internal class ServerInfo : NoRollbackCommand {


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="ivoker"></param>
		public ServerInfo ( Invoker invoker ) : base(invoker)
		{
		}


		/// <summary>
		/// Executes
		/// </summary>
		public override void Execute ()
		{
			if (Invoker.GameEngine.GameServer.IsAlive) {
				
				Invoker.GameEngine.GameServer.PrintServerInfo();

			} else {
				Log.Warning("Server is not running");
			}
		}
	}
}

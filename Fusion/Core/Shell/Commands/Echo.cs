using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Shell.Commands {
	
	[Command("echo", CommandAffinity.Default)]
	public class Echo : NoRollbackCommand {

		/// <summary>
		/// 
		/// </summary>
		[CommandLineParser.Required]
		public List<string> Message { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Echo ( Invoker invoker ) : base(invoker)
		{
			Message = new List<string>();
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ()
		{
			Log.Message("{0}", string.Join(" ", Message) );
		}
	}
}

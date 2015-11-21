using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Shell.Commands {
	
	[Command("listVars", CommandAffinity.Default)]
	public class ListVariables : Command {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public ListVariables ( Invoker invoker ) : base(invoker)
		{
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ()
		{
			Log.Message("");
			Log.Message("Variables:");

			var list = Invoker.Variables.ToList()
					.Select( e1 => e1.Value )
					.OrderBy( e => e.Prefix + e.Name )
					.ToList();
			
			foreach ( var variable in list ) {
				Log.Message("  {0,-35} = {1}", variable.Prefix + "." + variable.Name, variable.Get() );
			}
			Log.Message("{0} vars", list.Count );
		}



		/// <summary>
		/// No rollback.
		/// </summary>
		public override void Rollback ()
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// No rollback.
		/// </summary>
		[CommandLineParser.Ignore]
		public override bool NoRollback
		{
			get	{
				return true;
			}
			set	{
			}
		}
		
	}
}

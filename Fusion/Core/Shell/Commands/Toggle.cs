using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Configuration;

namespace Fusion.Core.Shell.Commands {
	
	[Command("toggle", CommandAffinity.Default)]
	public class Toggle : Command {

		/// <summary>
		/// 
		/// </summary>
		[CommandLineParser.Required]
		public string Variable { get; set; }


		string oldValue;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Toggle ( Invoker invoker ) : base(invoker)
		{
			Variable = null;
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ()
		{
			ConfigVariable variable;

			if (!Invoker.Variables.TryGetValue( Variable, out variable )) {
				throw new Exception(string.Format("Variable '{0}' does not exist", Variable) );
			}

			oldValue	= variable.Get();
			var value	= oldValue.ToLowerInvariant();

			if (value=="false") {
				variable.Set("true");
			} else if (value=="true") {
				variable.Set("false");
			} else if (value=="0") {
				variable.Set("1");
			} else {
				variable.Set("0");
			}
		}



		public override void Rollback ()
		{
			ConfigVariable variable;

			if (!Invoker.Variables.TryGetValue( Variable, out variable )) {
				throw new Exception(string.Format("Variable '{0}' does not exist", Variable) );
			}

			variable.Set( oldValue );
		}
	}
}

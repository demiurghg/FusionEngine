using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Input {
	[Command("unbind", CommandAffinity.Default)]
	internal sealed class Unbind : Command {

		[CommandLineParser.Required]
		public Keys Key { get; set; }

		KeyBind	oldBind = null;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public Unbind ( Invoker invoker ) : base(invoker)
		{
		}



		public override void Execute ()
		{
			var kb = Game.Keyboard;

			oldBind	=	kb.Bindings.FirstOrDefault( b => b.Key == Key );

			kb.Unbind( Key );
		}



		public override void Rollback ()
		{
			var kb = Game.Keyboard;

			if (oldBind!=null) {
				kb.Bind( oldBind.Key, oldBind.KeyDownCommand, oldBind.KeyUpCommand );
			} else {
				kb.Unbind( oldBind.Key );
			}
		}
	}
}

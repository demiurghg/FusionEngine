using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Input {

	/// <summary>
	///	Binds key.
	/// </summary>
	[Command("bind", CommandAffinity.Default)]
	internal class Bind : Command {

		
		[CommandLineParser.Required]
		public Keys Key { get; set; }

		[CommandLineParser.Name("up")]
		public string KeyUpCommand { get; set; }

		[CommandLineParser.Name("down")]
		public string KeyDownCommand { get; set; }

		KeyBind	oldBind = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public Bind ( Invoker invoker ) : base(invoker)
		{
		}


		public override void Execute ()
		{
			var kb = GameEngine.Keyboard;

			oldBind	=	kb.Bindings.FirstOrDefault( b => b.Key == Key );

			if (kb.IsBound(Key)) {
				kb.Unbind(Key);
			}

			kb.Bind( Key, KeyDownCommand, KeyUpCommand );
		}



		public override void Rollback ()
		{
			var kb = GameEngine.Keyboard;

			if (oldBind!=null) {
				kb.Bind( oldBind.Key, oldBind.KeyDownCommand, oldBind.KeyUpCommand );
			} else {
				kb.Unbind( oldBind.Key );
			}
		}
		
	}
}

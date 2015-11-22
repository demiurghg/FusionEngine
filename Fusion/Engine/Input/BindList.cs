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
	[Command("listBind", CommandAffinity.Default)]
	internal class BindList : NoRollbackCommand {


		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public BindList ( Invoker invoker ) : base(invoker)
		{
		}


		public override void Execute ()
		{
			var kb = GameEngine.Keyboard;

			Log.Message("");

			foreach ( var bind in kb.Bindings.OrderBy(b=>b.Key) ) {
				Log.Message("{0,-8} = {1} | {2}", bind.Key, bind.KeyDownCommand, bind.KeyUpCommand );
			}

			Log.Message("{0} keys are bound", kb.Bindings.Count() );
		}
		
	}
}

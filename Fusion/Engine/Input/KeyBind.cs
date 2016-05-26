using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Input {
	
	/// <summary>
	/// 
	/// </summary>
	public sealed class KeyBind {

		/// <summary>
		/// Key that associated with the commands.
		/// </summary>
		public Keys Key { get; private set; }

		/// <summary>
		/// Command to execute on key down event.
		/// </summary>
		public string KeyDownCommand { get; private set; }

		/// <summary>
		/// Command to execute on key up event.
		/// </summary>
		public string KeyUpCommand { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="keyDownCommand"></param>
		/// <param name="keyUpCommand"></param>
		public KeyBind ( Keys key, string keyDownCommand, string keyUpCommand )
		{
			Key				=	key;
			KeyDownCommand	=	keyDownCommand;
			KeyUpCommand	=	keyUpCommand;
		}

	}
}

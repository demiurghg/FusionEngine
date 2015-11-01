using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Shell {

	/// <summary>
	/// Shell command attribute
	/// </summary>
	public enum CommandAffinity {

		/// <summary>
		/// Command will be dispatched in UI thread.
		/// Only Default commands are undoable.
		/// </summary>
		Default,

		/// <summary>
		/// Command will be dispatched by server.
		/// </summary>
		Server,

		/// <summary>
		/// Command will be dispatched by client.
		/// </summary>
		Client,
	}
}

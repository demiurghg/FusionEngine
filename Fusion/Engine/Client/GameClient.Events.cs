using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Core.Content;


namespace Fusion.Engine.Client {

	/// <summary>
	/// Provides basic client-server interaction and client-side game logic.
	/// </summary>
	public abstract partial class GameClient : GameComponent {

		public class ClientEventArgs : EventArgs {	
			public ClientState ClientState;
			public string Message;
		}


		public event EventHandler<ClientEventArgs> ClientStateChanged;

	}
}

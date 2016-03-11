using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using System.Net;

namespace Fusion.Engine.Common {

	public abstract partial class UserInterface : GameModule {

		/// <summary>
		/// Creates instance of UserInterface
		/// </summary>
		/// <param name="Game"></param>
		public UserInterface ( Game Game ) : base(Game)
		{
		}


		/// <summary>
		/// Overloaded. Immediately releases the unmanaged resources used by this object. 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				if (client!=null) {
					client.Shutdown("dispose");
					client = null;
				}
			}
			base.Dispose( disposing );
		}


		/// <summary>
		/// Called when the game has determined that UI logic needs to be processed.
		/// </summary>
		/// <param name="gameTime"></param>
		public abstract void Update ( GameTime gameTime );

		/// <summary>
		/// Called when user tries to close program using Alt-F4 or from windows menu.
		/// </summary>
		public abstract void RequestToExit ();

		/// <summary>
		/// This method called each time when discovery responce arrived.
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="serverInfo"></param>
		public abstract void DiscoveryResponse ( IPEndPoint endPoint, string serverInfo );

		/// <summary>
		/// Starts server discovery.
		/// </summary>
		/// <param name="numPorts">Number of ports to scan.</param>
		/// <param name="timeout">Time to scan.</param>
		public void StartDiscovery ( int numPorts, TimeSpan timeout )
		{
			StartDiscoveryInternal(numPorts, timeout);
		}

		/// <summary>
		/// Stops server discovery.
		/// </summary>
		public void StopDiscovery ()
		{
			StopDiscoveryInternal();
		}

		/// <summary>
		/// Indicates that discovery in progress.
		/// </summary>
		/// <returns></returns>
		public bool IsDiscoveryRunning ()
		{
			return IsDiscoveryRunningInternal();
		}
	}
}

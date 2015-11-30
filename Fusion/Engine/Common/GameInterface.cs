using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using System.Net;

namespace Fusion.Engine.Common {

	public abstract partial class GameInterface : GameModule {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		public GameInterface ( GameEngine gameEngine ) : base(gameEngine)
		{
		}

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
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public abstract void Update ( GameTime gameTime );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ShowMessage ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ShowWarning ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ShowError ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ChatMessage ( string message );

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Lidgren.Network;
using System.Net;
using System.Diagnostics;

namespace Fusion.Engine.Common {

	public abstract partial class UserInterface : GameModule {

		NetClient client;

		TimeSpan timeout;

		object lockObj = new object();
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="numPorts"></param>
		/// <param name="timeout"></param>
		void StartDiscoveryInternal ( int numPorts, TimeSpan timeout )
		{
			lock (lockObj) {
				if (client!=null) {
					Log.Warning("Discovery is already started.");
					return;
				}

				this.timeout	=	timeout;

				var netConfig = new NetPeerConfiguration( Game.GameID );
				netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryRequest );
				netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryResponse );

				client	=	new NetClient( netConfig );
				client.Start();

				var svPort	=	Game.Network.Config.Port;

				var ports = Enumerable.Range(svPort, numPorts)
							.Where( p => p <= ushort.MaxValue )
							.ToArray();

				Log.Message("Start discovery on ports: {0}", string.Join(", ", ports) );

				foreach (var port in ports) {
					client.DiscoverLocalPeers( port );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void StopDiscoveryInternal ()
		{
			lock (lockObj) {
				if (client==null) {
					Log.Warning("Discovery is already started.");
					return;
				}

				Log.Message("Discovery is stopped.");

				client.Shutdown("stop discovery");
				client = null;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		bool IsDiscoveryRunningInternal()
		{
			return (client!=null);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		internal void UpdateInternal ( GameTime gameTime )
		{
			lock (lockObj) {
				if (client!=null) {

					DispatchIM( client );

					timeout -= gameTime.Elapsed;

					if (timeout<TimeSpan.Zero) {
						StopDiscoveryInternal();
					}
				}
			}
			
			Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		void DispatchIM ( NetClient client )
		{
			NetIncomingMessage msg;
			while ((msg = client.ReadMessage()) != null)
			{
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.VerboseDebugMessage:Log.Debug	("UI Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.DebugMessage:		Log.Verbose	("UI Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.WarningMessage:		Log.Warning	("UI Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.ErrorMessage:		Log.Error	("UI Net: " + msg.ReadString()); break;

					case NetIncomingMessageType.DiscoveryResponse:
						DiscoveryResponse( msg.SenderEndPoint, msg.ReadString() );
						break;

					//case NetIncomingMessageType.StatusChanged:		

					//	var status	=	(NetConnectionStatus)msg.ReadByte();
					//	var message	=	msg.ReadString();
					//	Log.Message("UI: {0} - {1}", status, message );

					//	break;
					
					//case NetIncomingMessageType.Data:
						
					//	var netCmd	=	(NetCommand)msg.ReadByte();
					//	state.DataReceived( netCmd, msg );

					//	break;
					
					default:
						Log.Warning("CL: Unhandled type: " + msg.MessageType);
						break;
				}
				client.Recycle(msg);
			}			
		}

	}
}

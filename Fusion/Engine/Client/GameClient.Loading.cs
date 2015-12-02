using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Engine.Network;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Engine.Server;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient {

		class Loading : State {

			/// <summary>
			/// if null - no reason to disconnect.
			/// </summary>
			string disconnectReason = null;
			Task loadingTask;
			

			public Loading ( GameClient gameClient, string serverInfo ) : base(gameClient)
			{
				loadingTask = new Task( () => gameClient.LoadContent(serverInfo) );
				loadingTask.Start();
			}


			public override void Connect ( string host, int port )
			{
				Log.Warning("Already connected. Loading in progress.");
			}


			public override void Disconnect ( string reason )
			{
				client.Disconnect( reason );
			}


			public override void Update ( GameTime gameTime )
			{
				if (loadingTask.IsCompleted) {
					if (disconnectReason!=null) {
						gameClient.SetState( new Disconnected(gameClient, disconnectReason) );
					} else {
						gameClient.SetState( new Awaiting(gameClient) );
					}
				}
				//throw new NotImplementedException();
			}


			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
				if (status==NetConnectionStatus.Disconnected) {
					disconnectReason = message;
				}
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
			}
		}
	}
}

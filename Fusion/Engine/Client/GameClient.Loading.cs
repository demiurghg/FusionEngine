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
			GameLoader loader;
			

			public Loading ( GameClient gameClient, string serverInfo ) : base(gameClient)
			{
				loader	=	gameClient.LoadContent( serverInfo );

				if (loader==null) {
					throw new InvalidOperationException("Null GameLoader");
				}
			}


			public override void UserConnect ( string host, int port )
			{
				Log.Warning("Already connected. Loading in progress.");
			}


			public override void UserDisconnect ( string reason )
			{
				client.Disconnect( reason );
			}


			public override void Update ( GameTime gameTime )
			{
				loader.Update(gameTime);

				//	sleep a while to get 
				//	other threads more time.
				Thread.Sleep(1);

				if (loader.IsCompleted) {
					if (disconnectReason!=null) {
						gameClient.SetState( new Disconnected(gameClient, disconnectReason) );
					} else {
						gameClient.FinalizeLoad( loader );
						gameClient.SetState( new Awaiting(gameClient) );
					}
				}
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

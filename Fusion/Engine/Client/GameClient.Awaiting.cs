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

		class Awaiting : State {

			public Awaiting ( GameClient gameClient ) : base(gameClient)
			{
				//	send user command to draw server attention:
				gameClient.SendUserCommand( client, 0, new byte[0] );
			}



			public override void UserConnect ( string host, int port )
			{
				Log.Warning("Already connected. Waiting for snapshot.");
			}



			public override void UserDisconnect ( string reason )
			{
				client.Disconnect( reason );
			}



			public override void Update ( GameTime gameTime )
			{
			}


			
			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
 				if (status==NetConnectionStatus.Disconnected) {
					gameClient.SetState( new Disconnected(gameClient, message) );
				}
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
				if (command==NetCommand.Snapshot) {
					var frame		=	msg.ReadUInt32();
					var prevFrame	=	msg.ReadUInt32();
					var size		=	msg.ReadInt32();

					if (prevFrame!=0) {
						Log.Warning("Bad initial snapshot. Previous frame does not equal zero.");
						return;
					}

					var snapshot	=	NetworkEngine.Decompress( msg.ReadBytes(size) );

					gameClient.SetState( new Active( gameClient, frame, snapshot ) );
				}

				if (command==NetCommand.Notification) {
					gameClient.FeedNotification( msg.ReadString() );
				}
			}
		}
	}
}

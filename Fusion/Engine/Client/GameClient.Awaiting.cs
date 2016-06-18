using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Engine.Server;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient {

		class Awaiting : State {

			public Awaiting ( GameClient gameClient ) : base(gameClient, ClientState.Awaiting)
			{
				//	send user command to draw server attention:
				//	snapshotID and commandID are zero, because we dont have valid snapshot yet.
				gameClient.SendUserCommand( client, 0, 0, new byte[0] );
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
					var ackCmdID	=	msg.ReadUInt32();
					var serverTicks	=	msg.ReadInt64();
					var size		=	msg.ReadInt32();

					//Log.Warning("{0}", offsetTicks );

					if (prevFrame!=0) {
						Log.Warning("Bad initial snapshot. Previous frame does not equal zero.");
						return;
					}
					if (ackCmdID!=0) {
						Log.Warning("Bad command ID {0}. Command ID for initial snapshot must be zero.", ackCmdID);
						return;
					}

					//	read snapshot :
					var snapshot	=	NetDeflate.Decompress( msg.ReadBytes(size) );

					//	initial snapshot contains atom table :
					gameClient.Atoms	=	new AtomCollection( msg );


					gameClient.SetState( new Active( gameClient, frame, snapshot, serverTicks ) );
				}

				if (command==NetCommand.Notification) {
					gameClient.FeedNotification( msg.ReadString() );
				}
			}
		}
	}
}

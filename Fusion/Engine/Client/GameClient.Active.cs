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

		class Active : State {

			//	first commandID must not be zero.
			uint commandCounter = 1;

			SnapshotQueue queue;
			
			uint lastSnapshotFrame;


			public Active ( GameClient gameClient, uint snapshotId, byte[] initialSnapshot ) : base(gameClient, ClientState.Active)
			{
				queue	=	new SnapshotQueue(32);
				queue.Push( new Snapshot( new TimeSpan(0), snapshotId, initialSnapshot) );

				lastSnapshotFrame	=	snapshotId;

				gameClient.FeedSnapshot( initialSnapshot, 0 );
			}



			public override void UserConnect ( string host, int port )
			{
				Log.Warning("Already connected.");
			}



			public override void UserDisconnect ( string reason )
			{
				client.Disconnect( reason );
			}



			public override void Update ( GameTime gameTime )
			{
				var userCmd  = gameClient.Update(gameTime, commandCounter);

				bool showSnapshot = gameClient.Game.Network.Config.ShowSnapshots;

				if (showSnapshot) {
					Log.Message("User cmd: #{0} : {1}", lastSnapshotFrame, userCmd.Length );
				}

				gameClient.SendUserCommand( client, lastSnapshotFrame, commandCounter, userCmd );

				//	increase command counter:
				commandCounter++;
			}



			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
 				if (status==NetConnectionStatus.Disconnected) {
					gameClient.SetState( new Disconnected(gameClient, message) );
				}
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
				bool showSnapshot = gameClient.Game.Network.Config.ShowSnapshots;

				if (command==NetCommand.Snapshot) {
					var index		=	msg.ReadUInt32();
					var prevFrame	=	msg.ReadUInt32();
					var ackCmdID	=	msg.ReadUInt32();
					var size		=	msg.ReadInt32();

					lastSnapshotFrame	=	index;

					var snapshot		=	queue.Decompress( prevFrame, msg.ReadBytes(size) );
					
					if (snapshot!=null) {

						gameClient.FeedSnapshot( snapshot, ackCmdID );
						queue.Push( new Snapshot(new TimeSpan(0), index, snapshot) );

					} else {
						lastSnapshotFrame = 0;
					}
				}

				if (command==NetCommand.Notification) {
					gameClient.FeedNotification( msg.ReadString() );
				}
			}
		}
	}
}

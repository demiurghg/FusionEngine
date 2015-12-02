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

			SnapshotQueue queue;
			
			uint lastSnapshotFrame;


			public Active ( GameClient gameClient, uint snapshotId, byte[] initialSnapshot ) : base(gameClient)
			{
				queue	=	new SnapshotQueue(32);
				queue.Push( new Snapshot(snapshotId, initialSnapshot) );

				lastSnapshotFrame	=	snapshotId;

				gameClient.FeedSnapshot( initialSnapshot, true );
			}



			public override void Connect ( string host, int port )
			{
				Log.Warning("Already connected.");
			}



			public override void Disconnect ( string reason )
			{
				client.Disconnect( reason );
			}



			public override void Update ( GameTime gameTime )
			{
				var userCmd  = gameClient.Update(gameTime);

				bool showSnapshot = gameClient.GameEngine.Network.Config.ShowSnapshots;

				if (showSnapshot) {
					Log.Message("User cmd: #{0} : {1}", lastSnapshotFrame, userCmd.Length );
				}

				gameClient.SendUserCommand( client, lastSnapshotFrame, userCmd );
			}



			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
 				if (status==NetConnectionStatus.Disconnected) {
					gameClient.SetState( new Disconnected(gameClient, message) );
				}
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
				bool showSnapshot = gameClient.GameEngine.Network.Config.ShowSnapshots;

				if (command==NetCommand.Snapshot) {
					var index		=	msg.ReadUInt32();
					var prevFrame	=	msg.ReadUInt32();
					var size		=	msg.ReadInt32();

					lastSnapshotFrame	=	index;

					var snapshot		=	queue.Decompress( prevFrame, msg.ReadBytes(size) );
					
					if (snapshot!=null) {

						gameClient.FeedSnapshot( snapshot, false );
						queue.Push( new Snapshot(index, snapshot) );

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

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
using System.Diagnostics;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient {

		class Active : State {

			//	first commandID must not be zero.
			uint commandCounter = 1;
			uint lastSnapshotFrame;

			SnapshotQueue	queue;
			JitterBuffer	jitter;
			Stopwatch		stopwatch;


			/// <summary>
			/// Creates instance of Active client state.
			/// </summary>
			/// <param name="gameClient"></param>
			/// <param name="snapshotId"></param>
			/// <param name="initialSnapshot"></param>
			public Active ( GameClient gameClient, uint snapshotId, byte[] initialSnapshot, long svTicks ) : base(gameClient, ClientState.Active)
			{
				queue	=	new SnapshotQueue(32);
				queue.Push( new Snapshot( new TimeSpan(0), snapshotId, initialSnapshot) );

				jitter		=	new JitterBuffer( svTicks );
				stopwatch	=	new Stopwatch();
				stopwatch.Start();

				lastSnapshotFrame	=	snapshotId;

				gameClient.FeedSnapshot( initialSnapshot, 0 );
			}



			/// <summary>
			/// Handles user request for connection.
			/// </summary>
			/// <param name="host"></param>
			/// <param name="port"></param>
			public override void UserConnect ( string host, int port )
			{
				Log.Warning("Already connected.");
			}



			/// <summary>
			/// Handles user request for disconnection.
			/// </summary>
			/// <param name="reason"></param>
			public override void UserDisconnect ( string reason )
			{
				client.Disconnect( reason );
			}



			/// <summary>
			/// Called on each client frame
			/// </summary>
			/// <param name="gameTime"></param>
			public override void Update ( GameTime gameTime )
			{
				//
				//	Feed snapshot from jitter buffer :
				//
				uint ackCmdID;
				byte[] snapshot = jitter.Pop( stopwatch.Elapsed.Ticks, out ackCmdID );

				if (snapshot!=null) {

					gameClient.FeedSnapshot( snapshot, ackCmdID );

				}

				//
				//	Update client state and get user command:
				//
				var userCmd  = gameClient.Update(gameTime, commandCounter);

				bool showSnapshot = gameClient.Game.Network.Config.ShowSnapshots;

				if (showSnapshot) {
					Log.Message("User cmd: #{0} : {1}", lastSnapshotFrame, userCmd.Length );
				}

				gameClient.SendUserCommand( client, lastSnapshotFrame, commandCounter, userCmd );

				//	increase command counter:
				commandCounter++;
			}



			/// <summary>
			/// Called when NetClient changed its status
			/// </summary>
			/// <param name="status"></param>
			/// <param name="message"></param>
			/// <param name="connection"></param>
			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
 				if (status==NetConnectionStatus.Disconnected) {
					gameClient.SetState( new Disconnected(gameClient, message) );
				}
			}



			/// <summary>
			/// 
			/// </summary>
			/// <param name="snapshot"></param>
			/// <param name="ackCmdID"></param>
			/// <param name="svTicks"></param>
			void FeedSnapshot ( byte[] snapshot, uint ackCmdID, long svTicks )
			{
				jitter.Push( snapshot, ackCmdID, svTicks );
				gameClient.FeedSnapshot( snapshot, ackCmdID );
			}



			/// <summary>
			/// Called when data arrived.
			/// It could snapshot or notification.
			/// </summary>
			/// <param name="command"></param>
			/// <param name="msg"></param>
			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
				bool showSnapshot = gameClient.Game.Network.Config.ShowSnapshots;

				if (command==NetCommand.Snapshot) {
					var index		=	msg.ReadUInt32();
					var prevFrame	=	msg.ReadUInt32();
					var ackCmdID	=	msg.ReadUInt32();
					var serverTicks	=	msg.ReadInt64();
					var size		=	msg.ReadInt32();

					lastSnapshotFrame	=	index;
					var snapshot		=	queue.Decompress( prevFrame, msg.ReadBytes(size) );
					
					if (snapshot!=null) {

						FeedSnapshot( snapshot, ackCmdID, serverTicks );
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

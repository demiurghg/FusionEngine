﻿//#define USE_DEJITTER
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
using System.Diagnostics;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient {

		class Active : State {

			//	first commandID must not be zero.
			uint commandCounter = 1;
			uint lastSnapshotFrame;

			SnapshotQueue	queue;
			//JitterBuffer	jitter;
			Stopwatch		stopwatch;
			long			clientFrames = 0;
			long			clientTicks;

			long			lastServerTicks;
			uint			lastSnapshotID;


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

				lastServerTicks		=	svTicks;
				lastSnapshotID		=	snapshotId;
				lastSnapshotFrame	=	snapshotId;

				Message				=	"";


				#if USE_DEJITTER
				jitter		=	new JitterBuffer( gameClient.Game, svTicks );
				#endif


				stopwatch	=	new Stopwatch();
				stopwatch.Start();
				clientTicks	=	stopwatch.Elapsed.Ticks;


				gameClient.FeedSnapshot( new GameTime(0,svTicks,0L), initialSnapshot, 0 );
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
				//	Update timing and frame counting :
				//
				long currentTime	=	stopwatch.Elapsed.Ticks;
				long deltaTime		=	currentTime - clientTicks;
				clientTicks			=	currentTime;
				var  clientTime		=	new GameTime(clientFrames, currentTime, deltaTime);

				clientFrames++;

				//
				//	Feed snapshot from jitter buffer :
				//
				#if USE_DEJITTER
				uint ackCmdID;
				byte[] snapshot = jitter.Pop( clientTicks, playoutDelay, out ackCmdID );

				if (snapshot!=null) {
					gameClient.FeedSnapshot( snapshot, ackCmdID );
				}
				#endif


				//
				//	Update client state and get user command:
				//
				var userCmd  = gameClient.Update( clientTime, commandCounter);


				//	show user commands :
				bool showSnapshot = gameClient.Game.Network.ShowUserCommands;
				if (showSnapshot) {
					Log.Message("User cmd: #{0} : {1}", lastSnapshotFrame, userCmd.Length );
				}


				//
				//	Send user command :
				//
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
			void FeedSnapshot ( byte[] snapshot, uint ackCmdID, uint snapshotId, long svTicks, long svFrame )
			{
				uint indexDelta	=	snapshotId - lastSnapshotID;
				lastSnapshotID	=	snapshotId;

				long timeDelta	=	svTicks - lastServerTicks;
				lastServerTicks	=	svTicks;

				if (indexDelta>1) {
					Log.Warning("Snapshot(s) dropped: {2} - [{0}-{1}]", lastSnapshotID - indexDelta, lastSnapshotID, indexDelta-1);
				}

				if (indexDelta==0) {
					Log.Error("Duplicate snapshot #{0}", snapshotId);
				} else {
					timeDelta	/= indexDelta;
				}

				#if USE_DEJITTER
				jitter.Push( snapshot, ackCmdID, svTicks, stopwatch.Elapsed.Ticks );
				#else
				gameClient.FeedSnapshot( new GameTime(svFrame, svTicks,timeDelta), snapshot, ackCmdID );
				#endif
			}



			/// <summary>
			/// Called when data arrived.
			/// It could snapshot or notification.
			/// </summary>
			/// <param name="command"></param>
			/// <param name="msg"></param>
			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
				if (command==NetCommand.Snapshot) {

					//Log.Message("ping:{0} - offset:{1}", msg.SenderConnection.AverageRoundtripTime, msg.SenderConnection.RemoteTimeOffset);

					var index		=	msg.ReadUInt32();
					var prevFrame	=	msg.ReadUInt32();
					var ackCmdID	=	msg.ReadUInt32();
					var serverTicks	=	msg.ReadInt64();
					var size		=	msg.ReadInt32();

					lastSnapshotFrame	=	index;
					var snapshot		=	queue.Decompress( prevFrame, msg.ReadBytes(size) );
					
					if (snapshot!=null) {

						FeedSnapshot( snapshot, ackCmdID, index, serverTicks, index );
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

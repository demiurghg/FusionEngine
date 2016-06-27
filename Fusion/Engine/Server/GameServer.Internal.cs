﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using System.Net;
using Fusion.Core.Shell;
using Fusion.Engine.Common;
using Fusion.Engine.Common.Commands;
using System.Diagnostics;
using Fusion.Core.Content;


namespace Fusion.Engine.Server {
	

	public abstract partial class GameServer : GameModule {

		Task serverTask;
		CancellationTokenSource killToken;

		object lockObj = new object();

		Queue<string> notifications = null;

		public event Action OnServerShutdown;


		/// <summary>
		/// Gets whether server is still alive.
		/// </summary>
		public bool IsAlive {
			get {
				return serverTask != null; 
			}
		}



		/// <summary>
		/// Initiate server thread.
		/// </summary>
		/// <param name="map"></param>
		/// <param name="postCommand"></param>
		internal void StartInternal ( string map, string postCommand )
		{
			lock (lockObj) {
				if (IsAlive) {
					Log.Warning("Can not start server, it is already running");
					return;
				}

				killToken	=	new CancellationTokenSource();
				serverTask	=	new Task( () => ServerTaskFunc(map, postCommand), killToken.Token );
				serverTask.Start();
			}
		}


		
		/// <summary>
		/// Kills server thread.
		/// </summary>
		/// <param name="wait"></param>
		internal void KillInternal ()
		{
			lock (lockObj) {
				if (!IsAlive) {
					Log.Warning("Server is not running");
				}

				if (killToken!=null) {
					killToken.Cancel();
				}
			}
		}



		/// <summary>
		/// Waits for server thread.
		/// </summary>
		internal void Wait ()
		{
			lock (lockObj) {
				if (killToken!=null) {
					killToken.Cancel();
				}

				if (serverTask!=null) {
					Log.Message("Waiting for server task...");
					serverTask.Wait();
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="map"></param>
		void ServerTaskFunc ( string map, string postCommand )
		{
			var netConfig		=	new NetPeerConfiguration(Game.GameID);
			netConfig.Port		=	Game.Network.Port;
			netConfig.MaximumConnections	=	32;
			netConfig.UnreliableSizeBehaviour = NetUnreliableSizeBehaviour.NormalFragmentation;
			
			if (Debugger.IsAttached) {
				netConfig.ConnectionTimeout		=	float.MaxValue;	
				Log.Message("SV: Debugger is attached: ConnectionTimeout = {0} sec", netConfig.ConnectionTimeout);
			}

			netConfig.EnableMessageType( NetIncomingMessageType.ConnectionApproval );
			netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryRequest );
			netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryResponse );
			netConfig.EnableMessageType( NetIncomingMessageType.ConnectionLatencyUpdated );

			var server		=	new NetServer( netConfig );
			notifications	=	new Queue<string>();

			Log.Message("SV: Start: {0} {1}", map, postCommand);

			var snapshotQueue	=	new SnapshotQueue(32);
			var serverFrames	=	0L;

			//var accumulatedElapsedTime = TimeSpan.FromTicks(0);
			//var maxElapsedTime = TimeSpan.FromMilliseconds(1000);
			//var previousTicks = (long)0;

			//
			//	configure & start server :
			//
			try {

				server.Start();

				//
				//	start game specific stuff :
				//
				atoms	=	new AtomCollection();
				LoadContent( map );
				atoms.Lock();

				//
				//	invoke post-start command :
				//
				if (postCommand!=null) {
					Game.Invoker.Push( postCommand );
				}


				//	Timer and fixed timestep stuff :
				//	http://gafferongames.com/game-physics/fix-your-timestep/
				var accumulator	=	TimeSpan.Zero;
				var stopwatch	=	new Stopwatch();
				stopwatch.Start();
				var currentTime	=	stopwatch.Elapsed;
				var time		=	stopwatch.Elapsed;

				//
				//	server loop :
				//	
				while ( !killToken.IsCancellationRequested ) {

				_retryTick:
					var targetDelta	=	TimeSpan.FromTicks( (long)(10000000 / TargetFrameRate) );
					var newTime		=	stopwatch.Elapsed;
					var frameTime	=	newTime - currentTime;
					currentTime		=	newTime;

					accumulator +=	 frameTime;

					if ( accumulator < targetDelta ) {
						Thread.Sleep(1);
						goto _retryTick;
					}

					while ( accumulator > targetDelta ) {

						//var svTime = new GameTime( time, targetDelta );
						var svTime = new GameTime( serverFrames, time, targetDelta );
						serverFrames++;

						//
						//	Do actual server stuff :
						//	
						UpdateNetworkAndLogic( svTime, server, snapshotQueue );

						accumulator	-= targetDelta;
						time		+= targetDelta;
					}
				}

				foreach ( var conn in server.Connections ) {
					conn.Disconnect("Server disconnected");
				}

			} catch ( Exception e ) {
				Log.PrintException( e, "Server error: {0}", e.Message );

				foreach ( var conn in server.Connections ) {
					conn.Disconnect(string.Format("Server error: {0}", e.Message));
				}

			} finally {

				//
				//	kill game specific stuff :
				//	try...catch???
				//
				UnloadContent();

				//
				//	shutdown connection :
				//
				server.Shutdown("Server shutdown");

				while (server.Status != NetPeerStatus.NotRunning) {
					Thread.Sleep(10);
				}
				
				Log.Message("SV: Shutdown");

				notifications	=	null;

				killToken	=	null;
				serverTask	=	null;
				server		=	null;
				pings		=	null;

				if (OnServerShutdown != null) {
					OnServerShutdown();
				}
			}
		}



		/// <summary>
		/// Updates everything related to network and game logic.
		/// </summary>
		void UpdateNetworkAndLogic ( GameTime svTime, NetServer server, SnapshotQueue snapshotQueue )
		{
			#if DEBUG
			server.Configuration.SimulatedLoss				=	Game.Network.SimulatePacketsLoss;
			server.Configuration.SimulatedMinimumLatency	=	Game.Network.SimulateMinLatency;
			server.Configuration.SimulatedRandomLatency		=	Game.Network.SimulateRandomLatency;
			#endif

			//	read input messages :
			DispatchIM( svTime, snapshotQueue, server );

			//	update pings :
			UpdatePings( server );

			//	update frame and get snapshot :
			var snapshot = Update( svTime );

			//	push snapshot to queue :
			snapshotQueue.Push( svTime.Total, snapshot );

			//	send snapshot to clients :
			SendSnapshot( server, snapshotQueue, svTime.Total.Ticks );

			//	send notifications to clients :
			SendNotifications( server );

			//	execute server's command queue :
			Game.Invoker.ExecuteQueue( svTime, CommandAffinity.Server );

			//	crash test for server :
			CrashServer.CrashTest();
			FreezeServer.FreezeTest();
			SlowdownServer.SlowTest();
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Client-server stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		Dictionary<Guid,float> pings = null;

		void UpdatePings ( NetServer server )
		{
			pings	=	server.Connections.ToDictionary( conn => conn.GetHailGuid(), conn => conn.AverageRoundtripTime );
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		void DispatchIM ( GameTime gameTime, SnapshotQueue queue, NetServer server )
		{
			NetIncomingMessage msg;
			while ((msg = server.ReadMessage()) != null)
			{
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.VerboseDebugMessage:Log.Debug	("SV Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.DebugMessage:		Log.Verbose	("SV Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.WarningMessage:		Log.Warning	("SV Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.ErrorMessage:		Log.Error	("SV Net: " + msg.ReadString()); break;

					case NetIncomingMessageType.ConnectionLatencyUpdated:
						if (Game.Network.ShowLatency) {
							float latency = msg.ReadFloat();
							Log.Verbose("...SV ping - {0} {1,6:0.00} ms", msg.SenderEndPoint, (latency*1000) );
						}

						break;

					case NetIncomingMessageType.DiscoveryRequest:
						Log.Message("Discovery request from {0}", msg.SenderEndPoint.ToString() );
						var response = server.CreateMessage( ServerInfo() );
						server.SendDiscoveryResponse( response, msg.SenderEndPoint );

						break;

					case NetIncomingMessageType.ConnectionApproval:
						
						var userGuid	=	msg.SenderConnection.GetHailGuid();
						var userInfo	=	msg.SenderConnection.GetHailUserInfo();
						var reason		=	"";
						var approve		=	ApproveClient( userGuid, userInfo, out reason );

						if (approve) {	
							msg.SenderConnection.Approve( server.CreateMessage( ServerInfo() ) );
						} else {
							msg.SenderConnection.Deny( reason );
						}

						break;

					case NetIncomingMessageType.StatusChanged:		
						DispatchStatusChange( msg );
						break;
					
					case NetIncomingMessageType.Data:
						DispatchDataIM( gameTime, queue, msg );
						break;
					
					default:
						Log.Warning("SV: Unhandled type: " + msg.MessageType);
						break;
				}
				server.Recycle(msg);
			}		
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void DispatchStatusChange ( NetIncomingMessage msg )
		{
			var connStatus	=	(NetConnectionStatus)msg.ReadByte();
			var senderEP	=	msg.SenderEndPoint;
			var text		=	msg.ReadString();

			Log.Message	("SV: {0}: {1}: {2}", connStatus, senderEP.ToString(), text);
			
			switch (connStatus) {
				case NetConnectionStatus.Connected :
					msg.SenderConnection.InitClientState(); 
					ClientConnected( msg.SenderConnection.GetHailGuid(), msg.SenderConnection.GetHailUserInfo() );
					break;

				case NetConnectionStatus.Disconnected :
					ClientDeactivated( msg.SenderConnection.GetHailGuid() );
					ClientDisconnected( msg.SenderConnection.GetHailGuid() );
					break;

				default:
					break;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="server"></param>
		void SendSnapshot ( NetServer server, SnapshotQueue queue, long serverTicks )
		{
			//	snapshot request is stored in connection's tag.s
			var debug	=	Game.Network.ShowSnapshots;
			var conns	=	server.Connections.Where ( c => c.IsSnapshotRequested() );

			var sw		=	new Stopwatch();

			foreach ( var conn in conns ) {

				sw.Reset();
				sw.Start();
					
				var frame		=	queue.LastFrame;
				var prevFrame	=	conn.GetRequestedSnapshotID();
				int size		=	0;
				var commandID	=	conn.GetLastCommandID();
				var snapshot	=	queue.Compress( ref prevFrame, out size);

				//	reset snapshot request :
				conn.ResetRequestSnapshot();

				var msg = server.CreateMessage( snapshot.Length + 4 * 4 + 8 + 1 );
			
				msg.Write( (byte)NetCommand.Snapshot );
				msg.Write( frame );
				msg.Write( prevFrame );
				msg.Write( commandID );
				msg.Write( serverTicks );
				msg.Write( snapshot.Length );
				msg.Write( snapshot ); 

				//	append atom table to first snapshot :
				if (commandID==0) {
					atoms.Write( msg );
				}

				//	Zero snapshot frame index means that client is waiting for first snapshot.
				//	and snapshot should reach the client.
				var delivery	=	prevFrame == 0 ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced;

				if (prevFrame==0) {
					Log.Message("SV: Sending initial snapshot to {0}", conn.GetHailGuid().ToString() );
				}

				sw.Stop();

				server.SendMessage( msg, conn, delivery, 0 );

				if (debug) {
					Log.Message("Snapshot: #{0} - #{1} : {2} / {3} to {4} at {5} msec", 
						frame, prevFrame, snapshot.Length, size, conn.RemoteEndPoint.ToString(), sw.Elapsed.TotalMilliseconds );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="server"></param>
		void SendNotifications ( NetServer server )
		{
			List<string> messages;
			lock (notifications) {
				messages = notifications.ToList();
				notifications.Clear();
			}

			var conns = server.Connections;

			if (!conns.Any()) {
				return;
			}

			foreach ( var message in messages ) {
				var msg = server.CreateMessage( message.Length + 1 );
				msg.Write( (byte)NetCommand.Notification );
				msg.Write( message );
				server.SendMessage( msg, conns, NetDeliveryMethod.ReliableSequenced, 0 );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void DispatchDataIM ( GameTime gameTime, SnapshotQueue queue, NetIncomingMessage msg )
		{
			var netCmd = (NetCommand)msg.ReadByte();

			switch (netCmd) {
				case NetCommand.UserCommand : 
					DispatchUserCommand( gameTime, queue, msg );
					break;

				case NetCommand.Notification :
					FeedNotification( msg.SenderConnection.GetHailGuid(), msg.ReadString() );
					break;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void DispatchUserCommand ( GameTime gameTime, SnapshotQueue queue, NetIncomingMessage msg )
		{	
			var snapshotID	=	msg.ReadUInt32();
			var commandID	=	msg.ReadUInt32();
			var size		=	msg.ReadInt32();

			var data		=	msg.ReadBytes( size );

			//	we got user command and (command count=1)
			//	this means that client receives snapshot:
			if (msg.SenderConnection.GetCommandCount()==1) {
				ClientActivated( msg.SenderConnection.GetHailGuid() );
			}

			//	do not feed server with empty command.
			if (data.Length>0) {
				FeedCommand( msg.SenderConnection.GetHailGuid(), data, commandID, queue.GetLag(snapshotID, gameTime) );
			}

			//	set snapshot request when command get.
			msg.SenderConnection.SetRequestSnapshot( snapshotID, commandID );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		void NotifyClientsInternal ( string message )
		{
			if (notifications!=null) {
				lock (notifications) {
					notifications.Enqueue(message);
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		float GetPingInternal ( Guid clientGuid )
		{
			if (pings==null) {
				return float.MaxValue;
			}

			float ping;

			if (pings.TryGetValue( clientGuid, out ping )) {
				return ping;
			} else {
				return float.MaxValue;
			}
		}
	}
}

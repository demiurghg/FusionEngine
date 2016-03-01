using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fusion.Engine.Network;
using System.Net;
using Fusion.Core.Shell;
using Fusion.Engine.Common;
using Lidgren.Network;
using Fusion.Engine.Server;
using Fusion.Engine.Common.Commands;
using System.Diagnostics;
using Fusion.Core.Content;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient : GameModule {

		NetClient	client;
		State		state;


		/// <summary>
		/// Sets state
		/// </summary>
		/// <param name="newState"></param>
		void SetState ( State newState )
		{						
			this.state = newState;
			Log.Message("CL: State: {0}", newState.GetType().Name );
		}
							


		/// <summary>
		/// Inits internal stuff
		/// </summary>
		void InitInternal ()
		{
			SetState( new StandBy(this) );

			var netConfig	=	new NetPeerConfiguration(Game.GameID);

			netConfig.AutoFlushSendQueue	=	true;
			netConfig.EnableMessageType( NetIncomingMessageType.ConnectionApproval );
			//netConfig.EnableMessageType( NetIncomingMessageType.ConnectionLatencyUpdated );
			netConfig.EnableMessageType( NetIncomingMessageType.DiscoveryRequest );
			netConfig.UnreliableSizeBehaviour = NetUnreliableSizeBehaviour.NormalFragmentation;

			if (Debugger.IsAttached) {
				netConfig.ConnectionTimeout		=	float.MaxValue;	
				Log.Message("CL: Debugger is attached: ConnectionTimeout = {0} sec", netConfig.ConnectionTimeout);
			}

			client	=	new NetClient( netConfig );
		}



		/// <summary>
		/// Wait for client completion.
		/// </summary>
		internal void Wait ()
		{	
			if ( !(state is StandBy) && !(state is Disconnected) ) {
				DisconnectInternal("quit");
			}


			while ( !(state is StandBy) ) {
				Thread.Sleep(50);
				UpdateInternal( new GameTime() );
			}
		}



		/// <summary>
		/// Request connection. Result depends on current client state.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void ConnectInternal ( string host, int port )
		{
			state.UserConnect( host, port );
		}



		/// <summary>
		/// Request diconnect. Result depends on current client state.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void DisconnectInternal (string message)
		{
			state.UserDisconnect(message);
		}



		/// <summary>
		/// Updates client.
		/// </summary>
		/// <param name="gameTime"></param>
		internal void UpdateInternal ( GameTime gameTime )
		{
			//
			//	Read messages :
			//	
			DispatchIM( client );

			//
			//	Update client-side game :
			//
			state.Update( gameTime );

			//
			//	Crash test :
			//
			CrashClient.CrashTest();
			FreezeClient.FreezeTest();

			//
			//	Execute command :
			//	Should command be executed in Active state only?
			//	
			try {
				Game.Invoker.ExecuteQueue( gameTime, CommandAffinity.Client );
			} catch ( Exception e ) {
				Log.Error( e.Message );
			}
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
					case NetIncomingMessageType.VerboseDebugMessage:Log.Debug	("CL Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.DebugMessage:		Log.Verbose	("CL Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.WarningMessage:		Log.Warning	("CL Net: " + msg.ReadString()); break;
					case NetIncomingMessageType.ErrorMessage:		Log.Error	("CL Net: " + msg.ReadString()); break;

					case NetIncomingMessageType.ConnectionLatencyUpdated:
						float latency = msg.ReadFloat();
						Log.Verbose("CL ping: {0} - {1} ms", msg.SenderConnection.RemoteEndPoint, latency * 1000 );
						break;

					case NetIncomingMessageType.StatusChanged:		

						var status	=	(NetConnectionStatus)msg.ReadByte();
						var message	=	msg.ReadString();
						Log.Message("CL: {0} - {1}", status, message );

						state.StatusChanged( status, message, msg.SenderConnection );

						break;
					
					case NetIncomingMessageType.Data:
						
						var netCmd	=	(NetCommand)msg.ReadByte();
						state.DataReceived( netCmd, msg );

						break;
					
					default:
						Log.Warning("CL: Unhandled type: " + msg.MessageType);
						break;
				}
				client.Recycle(msg);
			}			
		}



		/// <summary>
		/// 
		/// </summary>
		internal void SendDiscoveryRequest ()
		{
			client.DiscoverLocalPeers( Game.Network.Config.Port );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		void NotifyInternal ( string message )
		{
			var msg = client.CreateMessage( message.Length + 1 );

			msg.Write( (byte)NetCommand.Notification );
			msg.Write( message );

			client.SendMessage( msg, NetDeliveryMethod.ReliableSequenced );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="userCmd"></param>
		void SendUserCommand ( NetClient client, uint recvSnapshotFrame, uint cmdCounter, byte[] userCmd )
		{
			var msg = client.CreateMessage( userCmd.Length + 4 * 3 + 1 );

			msg.Write( (byte)NetCommand.UserCommand );
			msg.Write( recvSnapshotFrame );
			msg.Write( cmdCounter );
			msg.Write( userCmd.Length );
			msg.Write( userCmd );

			//	Zero snapshot frame index means that we are waiting for first snapshot.
			//	and command shoud reach the server.
			var delivery	=	recvSnapshotFrame == 0 ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced;

			client.SendMessage( msg, delivery );
		}


		#if false
		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void DispatchSnapshot ( NetIncomingMessage msg )
		{
			var counter = msg.ReadUInt32();
			var size	= msg.ReadInt32();

			var data	= msg.ReadBytes( size );

			FeedSnapshot( data );
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

			Log.Message	("CL: {0}: {1}: {2}", connStatus, senderEP.ToString(), text);
			
			switch (connStatus) {
				case NetConnectionStatus.Connected :
					//clientState	=	ClientState.Loading;
					break;

				case NetConnectionStatus.Disconnected :
					DisconnectInternal(false);
					break;

				default:
					break;
			}
		}



		void InitiateLoadLevel ( string serverInfo )
		{
			var task = new Task( 
				()=> {
					LoadContent( serverInfo );
					//clientState = ClientState.Awaiting;
				}
			);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		void DispatchDataIM ( NetIncomingMessage msg )
		{
			var netCmd = (NetCommand)msg.ReadByte();

			switch (netCmd) {
				case NetCommand.Snapshot : 
					DispatchSnapshot( msg );
					break;

				case NetCommand.UserCommand : 
					Log.Warning("User command is received by client");
					break;

				case NetCommand.Notification :
					Log.Message("CL: Notification: {0}", msg.ReadString() );
					break;

				case NetCommand.ChatMessage :
					//CharM
					break;
			}
		}
		#endif
	}
}

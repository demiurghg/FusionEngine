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


namespace Fusion.Engine.Client {
	public abstract partial class GameClient : GameModule {

		ClientState state;
		NetClient	client;


		/// <summary>
		/// Request connection. Result depends on current client state.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void ConnectInternal ( string host, int port )
		{
			switch (state) {
				case ClientState.StandBy :
					DoConnect( host, port );
					break;
				case ClientState.Connecting :
				case ClientState.Loading :
				case ClientState.Awaiting :
					Log.Warning("{0} in progress.", state);
					break;
				case ClientState.Active :
					Log.Warning("Already connected");
					break;
			}
		}



		/// <summary>
		/// Request diconnect. Result depends on current client state.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void DisconnectInternal (bool disconnect)
		{
			switch (state) {
				case ClientState.Active :
					DoDisconnect();
					break;
				case ClientState.StandBy :
					Log.Warning("Not connected.");
					break;
				case ClientState.Loading:
				case ClientState.Awaiting:
				case ClientState.Connecting:
					Log.Warning("{0} can not be interrupted.", state);
					break;
			}
		}



		/// <summary>
		/// Updates client.
		/// </summary>
		/// <param name="gameTime"></param>
		internal void UpdateInternal ( GameTime gameTime )
		{
			if (client!=null) {
				DispatchIM(client);
			}
			
			Dispatch();
		}





		
		void DoConnect ( string host, int port )
		{
			var netConfig	=	new NetPeerConfiguration(GameEngine.GameTitle);
			netConfig.AutoFlushSendQueue	=	true;

			client	=	new NetClient( netConfig );

			client.Start();

			var userInfo	=	UserInfo();
			var hail		=	client.CreateMessage( userInfo );

			var serverEP	=	new IPEndPoint( IPAddress.Parse(host), port );

			var conn		=	client.Connect( serverEP, hail );

			state		=	ClientState.Connecting;
		}



		void DoDisconnect ()
		{
			if (client!=null) {
				client.Shutdown("Client disconnected");
				client = null;
			}

			//client.SendUnconnectedToSelf
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Client-server stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

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
					case NetIncomingMessageType.VerboseDebugMessage:Log.Verbose	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.DebugMessage:		Log.Debug	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.WarningMessage:		Log.Warning	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.ErrorMessage:		Log.Error	("CL: " + msg.ReadString()); break;

					case NetIncomingMessageType.StatusChanged:		
						DispatchStatusChange( msg );
						break;
					
					case NetIncomingMessageType.Data:
						DispatchDataIM( msg );
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
		/// <param name="client"></param>
		/// <param name="userCmd"></param>
		void SendUserCommand ( NetClient client, byte[] userCmd )
		{
			if (client==null) {
				return;
			}

			var msg = client.CreateMessage( userCmd.Length + 4 + 4 + 1 );

			msg.Write( (byte)NetCommand.UserCommand );
			msg.Write( (uint)0 );
			msg.Write( userCmd.Length );
			msg.Write( userCmd );

			client.SendMessage( msg, NetDeliveryMethod.UnreliableSequenced );
		}



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
					LoadLevel( serverInfo );
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
	}
}

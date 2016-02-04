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

		abstract class State {

			/// <summary>
			/// game client
			/// </summary>
			protected readonly GameClient	gameClient;

			/// <summary>
			///	net client
			/// </summary>
			protected NetClient client {
				get {
					return gameClient.client;
				}
			}


			public readonly ClientState ClientState;
			
			
			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="gameClient"></param>
			public State ( GameClient gameClient, ClientState clientState )
			{
				this.ClientState	=	clientState;
				this.gameClient	=	gameClient;
			}


			public abstract void UserConnect ( string host, int port );
			public abstract void UserDisconnect ( string reason );
			public abstract void Update ( GameTime gameTime );
			public abstract void StatusChanged ( NetConnectionStatus status, string message, NetConnection connection );
			public abstract void DataReceived ( NetCommand command, NetIncomingMessage msg );

		
			///// <summary>
			///// Dispatches common messages.
			///// </summary>
			///// <param name="msg"></param>
			//protected void DispatchDefaultIM( NetIncomingMessage msg )
			//{
			//	switch (msg.MessageType) {
			//		case NetIncomingMessageType.VerboseDebugMessage:Log.Verbose	("CL: " + msg.ReadString()); return true;
			//		case NetIncomingMessageType.DebugMessage:		Log.Debug	("CL: " + msg.ReadString()); return true;
			//		case NetIncomingMessageType.WarningMessage:		Log.Warning	("CL: " + msg.ReadString()); return true;
			//		case NetIncomingMessageType.ErrorMessage:		Log.Error	("CL: " + msg.ReadString()); return true;
			//		default: 
			//			Log.Warning("CL: Unhandled: {0}", msg.MessageType );
			//	}
			//}


			///// <summary>
			///// Parses status-change message.
			///// </summary>
			///// <param name="msg"></param>
			///// <param name="status"></param>
			///// <param name="message"></param>
			//protected NetConnectionStatus ParseStatusChange ( NetIncomingMessage msg )
			//{
			//	var status	=	(NetConnectionStatus)msg.ReadByte();
			//	var message	=	(NetConnectionStatus)msg.ReadString();
			//	Log.Message("CL: {0} - {1}", status, message );
			//	return status;
			//}
		}


		
	}
}

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

		NetClient client;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void ConnectInternal ( string host, int port )
		{
			var netConfig	=	new NetPeerConfiguration(GameEngine.GameTitle);
			netConfig.AutoFlushSendQueue	=	true;

			client	=	new NetClient( netConfig );

			client.Start();

			client.Connect( new IPEndPoint( IPAddress.Parse(host), port ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void DisconnectInternal ()
		{
			client.Disconnect("Client disconnected");
			client.Shutdown("Client shutdown");
			client = null;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		internal void UpdateInternal ( GameTime gameTime )
		{
			if (client!=null) {
				DispatchIM( client );
				Update( gameTime );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		void DispatchIM ( NetClient server )
		{
			NetIncomingMessage msg;
			while ((msg = server.ReadMessage()) != null)
			{
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.VerboseDebugMessage:Log.Verbose	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.DebugMessage:		Log.Debug	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.WarningMessage:		Log.Warning	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.ErrorMessage:		Log.Error	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.StatusChanged:		Log.Message	("CL: Status changed: {0}", (NetConnectionStatus)msg.ReadByte());	break;
					
					case NetIncomingMessageType.ConnectionLatencyUpdated:
						Log.Message("CL: Connection latencty - {0}", msg.ReadSingle() );
						break;

					case NetIncomingMessageType.Data:
						DispatchDataIM( msg );
						break;
					
					default:
						Log.Warning("CL: Unhandled type: " + msg.MessageType);
						break;
				}
				server.Recycle(msg);
			}			
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
					Log.Warning("Snapshot is received"); 
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

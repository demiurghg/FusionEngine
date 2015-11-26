using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using System.Net;
using Fusion.Core.Shell;
using Fusion.Engine.Network;
using Fusion.Engine.Common;


namespace Fusion.Engine.Server {

	public abstract partial class GameServer : GameModule {

		Task serverTask;
		CancellationTokenSource killToken;


		object lockObj = new object();


		/// <summary>
		/// Gets whether server is still alive.
		/// </summary>
		internal bool IsAlive {
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
			var netConfig		=	new NetPeerConfiguration( GameEngine.GameTitle );
			netConfig.Port		=	GameEngine.Network.Config.Port;
			netConfig.MaximumConnections	=	8;

			NetServer server	=	new NetServer( netConfig );



			//
			//	configure & start server :
			//
			try {

				server.Start();

				//
				//	start game specific stuff :
				//
				LoadLevel( map );

				//
				//	invoke post-start command :
				//
				if (postCommand!=null) {
					GameEngine.Invoker.Push( postCommand );
				}


				var svTime = new GameTime();

				//
				//	server loop :
				//	
				while ( !killToken.IsCancellationRequested ) {

					svTime.Update();

					DispatchIM( server );

					GameEngine.Invoker.ExecuteQueue( svTime, CommandAffinity.Server );

				}

			} catch ( Exception e ) {
				Log.PrintException( e, "Server error: {0}", e.Message );

			} finally {

				//
				//	kill game specific stuff :
				//	try...catch???
				//
				UnloadLevel();

				//
				//	shutdown connection :
				//
				server.Shutdown("Server shutdown");

				killToken	=	null;
				serverTask	=	null;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		void DispatchIM ( NetServer server )
		{
			NetIncomingMessage msg;
			while ((msg = server.ReadMessage()) != null)
			{
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.VerboseDebugMessage:Log.Verbose	("SV: " + msg.ReadString()); break;
					case NetIncomingMessageType.DebugMessage:		Log.Debug	("SV: " + msg.ReadString()); break;
					case NetIncomingMessageType.WarningMessage:		Log.Warning	("SV: " + msg.ReadString()); break;
					case NetIncomingMessageType.ErrorMessage:		Log.Error	("SV: " + msg.ReadString()); break;
					case NetIncomingMessageType.StatusChanged:		Log.Message	("SV: Status changed: {0}", (NetConnectionStatus)msg.ReadByte());	break;
					
					case NetIncomingMessageType.ConnectionLatencyUpdated:
						Log.Message("SV: Connection latencty - {0}", msg.ReadSingle() );
						break;

					case NetIncomingMessageType.Data:
						DispatchDataIM( msg );
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
		void DispatchDataIM ( NetIncomingMessage msg )
		{
			var netCmd = (NetCommand)msg.ReadByte();

			switch (netCmd) {
				case NetCommand.Snapshot : 
					Log.Warning("Snapshot is received by server"); 
					break;
				case NetCommand.UserCommand : 
					int size = msg.ReadInt32();
					FeedCommand( msg.SenderEndPoint.ToString(), msg.ReadBytes(size) );
					break;
				case NetCommand.Notification :
					Log.Warning("Notification is received by server");
					break;
				case NetCommand.ChatMessage :
					//CharM
					break;
			}
		}
	}
}

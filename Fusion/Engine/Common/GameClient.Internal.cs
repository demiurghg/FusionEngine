using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fusion.Engine.Network;
using System.Net;
using Fusion.Core.Shell;


namespace Fusion.Engine.Common {
	public abstract partial class GameClient : GameModule {

		Task	clientTask;
		CancellationTokenSource	disconnectToken;

		object lockObj = new object();



		/// <summary>
		/// Gets whether server is still alive.
		/// </summary>
		internal bool IsAlive {
			get {
				return clientTask != null; 
			}
		}

		
		/// <summary>
		/// Connects client.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void ConnectInternal ( string host, int port )
		{		
			lock (lockObj) {

				disconnectToken	=	new CancellationTokenSource();

				clientTask	=	new Task( () => ClientTaskFunc( host, port ), disconnectToken.Token );
				clientTask.Start();
			}
		}



		/// <summary>
		/// Internal disconnect.
		/// </summary>
		internal void DisconnectInternal (bool wait)
		{
			lock (lockObj) {
				if (!IsAlive) {
					Log.Warning("Not connected.");
					return;
				}
				if (disconnectToken!=null) {
					disconnectToken.Cancel();
				}
			}
		}
		


		/// <summary>
		/// Waits for client thread.
		/// </summary>
		internal void Wait ()
		{
			lock (lockObj) {
				if (disconnectToken!=null) {
					disconnectToken.Cancel();
				}

				if (clientTask!=null) {
					Log.Message("Waiting for client task...");
					clientTask.Wait();
				}
			}
		}


		NetChan chatNetChan = null;
		IPEndPoint serverEP;

		/// <summary>
		/// 
		/// </summary>
		void ClientTaskFunc ( string host, int port )
		{
			NetChan netChan = null;

			try {
				netChan	=	new NetChan( GameEngine, GameEngine.Network.ClientSocket, "CL" );

				chatNetChan = netChan;

				serverEP = new IPEndPoint( IPAddress.Parse(host), port );

				Connect( host, port );

				var clTime	=	new GameTime();


				while (!disconnectToken.IsCancellationRequested) {

					clTime.Update();

					Thread.Sleep(500);

					DispatchClientIM(netChan);
					
					Update( clTime );
				}


			} catch ( Exception e ) {
				Log.Error("Client error: {0}", e.ToString());

			} finally {

				//	try...catch???
				Disconnect();

				chatNetChan = null;

				SafeDispose( ref netChan );
				clientTask	=	null;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		internal void Chat ( string message )
		{
			if (chatNetChan!=null) {
				chatNetChan.OutOfBand( serverEP, NetChanMsgType.OOB_StringData, Encoding.ASCII.GetBytes(message) );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="netChan"></param>
		void DispatchClientIM ( NetChan netChan )
		{
			Datagram datagram = null;

			while ( netChan.Dispatch( out datagram ) ) {

				//datagram.Print();
				
				if (datagram.Header.MsgType==NetChanMsgType.OOB_StringData) {
					Log.Warning("chat: {0}", datagram.ASCII);
				}

			}
		}



		[Command("chat", CommandAffinity.Default)]
		public class ChatCmd : NoRollbackCommand {

			[CommandLineParser.Required]
			public string Message { get; set; }

			public ChatCmd ( Invoker invoker ) : base(invoker)
			{
			}

			public override void Execute ()
			{
				Invoker.GameEngine.GameClient.Chat(Message);
			}
		}
	}
}

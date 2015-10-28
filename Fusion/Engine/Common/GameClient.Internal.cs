using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fusion.Engine.Network;


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


		/// <summary>
		/// 
		/// </summary>
		void ClientTaskFunc ( string host, int port )
		{
			NetClient client = null;

			try {
				client	=	new NetClient(host, port);

				Connect( host, port );

				var clTime	=	new GameTime();


				while (!disconnectToken.IsCancellationRequested) {

					clTime.Update();

					Thread.Sleep(500);

					client.SendMessage("OLOLO!");
					
					Update( clTime );
				}


			} catch ( Exception e ) {
				Log.Error("Client error: {0}", e.ToString());

			} finally {

				//	try...catch???
				Disconnect();

				SafeDispose( ref client );
				clientTask	=	null;
			}
		}



	}
}

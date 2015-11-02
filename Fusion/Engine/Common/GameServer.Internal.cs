using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Lidgren.Network;
using System.Threading;
using System.Net;
using Fusion.Core.Shell;
using Fusion.Engine.Network;

namespace Fusion.Engine.Common {

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
			//
			//	configure & start server :
			//
			try {

				NetStart();

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

					NetDispatchIM(svTime);

					Update( svTime );

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
				NetShutdown();

				killToken	=	null;
				serverTask	=	null;
			}
		}
	}
}

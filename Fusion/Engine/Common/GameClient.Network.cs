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

		const int ReconnectPeriod	=	3000;
		const int ReconnectAttempts	=	10;

		NetChan	netChan;
		IPEndPoint serverEP;

		ConnState	connState	=	ConnState.Standby;
		int			connTime	=	0;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		void NetConnect ( string host, int port )
		{
			Log.Message("Connecting to {0}:{1}...", host, port );

			netChan		=	new NetChan( GameEngine, GameEngine.Network.ClientSocket, "CL" );

			serverEP	=	new IPEndPoint( IPAddress.Parse(host), port );

			ConnectToServer(10);
		}



		/// <summary>
		/// 
		/// </summary>
		void NetDisconnect ()
		{
			netChan.OutOfBand( serverEP, Protocol.ClientDisconnect);

			SafeDispose( ref netChan );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns>Challange ID</returns>
		int ConnectToServer (int attemptCount)
		{
			for (int i=0; i<attemptCount; i++) {
				Log.Message("  sending connect ({0})...", i);

				netChan.OutOfBand( serverEP, Protocol.ClientConnect + " \"" + UserInfo() + "\"");
				var message	=	netChan.Wait( (d) => d.GetString().StartsWith( Protocol.ServerConnectAck ), 10, 100 );

				if (message!=null) {
					Log.Message("  connection established.");
					return 0;
				}
			}
			
			throw new GameException("Server does not respond.");
		}


		/// <summary>
		/// 
		/// </summary>
		void NetDispatchIM ( GameTime gameTime )
		{
			
		}
	}
}

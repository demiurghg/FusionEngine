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


namespace Fusion.Engine.Client {
	public abstract partial class GameClient : GameModule {

		/// <summary>
		/// Client tries to connect to server.
		/// </summary>
		class STConnecting : STState {

			int maxAttempts = 10;
			int maxResendTimeout;
			int attemptCount = 0;
			int resendTimeout = 0;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public STConnecting ( GameClient client, string host, int port ) : base(client)
			{
				maxAttempts			=	client.GameEngine.Network.Config.ResendMaxCount;
				maxResendTimeout	=	client.GameEngine.Network.Config.ResendTimeout;

				IPAddress ip;

				if (!IPAddress.TryParse( host, out ip)) {
					Log.Error("Failed to parse IP: '{0}'", host);
					client.SetState( new STStandBy(client) );
					return;
				}

				Log.Message("Connecting to {0} {1}...", ip.ToString(), port );

				client.serverEP	=	new IPEndPoint( ip, port );

				SendConnect();
			}



			/// <summary>
			/// Sends connect request
			/// </summary>
			void SendConnect ()
			{
				if (attemptCount>=maxAttempts) {
					client.SetState( new STStandBy(client) );
					Log.Message("Server does not respond.");
					client.GameEngine.GameInterface.ShowError( "Server does not respond." );
				}

				Log.Message("Sending connect request {0}/{1}...", attemptCount++, maxAttempts);
				netChan.OutOfBand( client.serverEP, NetCommand.Connect, client.UserInfo() ); 
				resendTimeout = maxResendTimeout;
			}



			/// <summary>
			/// 
			/// </summary>
			/// <param name="host"></param>
			/// <param name="port"></param>
			public override void Connect ( string host, int port )
			{
				Log.Warning("Already connecting.");
			}


			/// <summary>
			/// 
			/// </summary>
			public override void Disconnect ()
			{
				Log.Message("Connection interrupted by user.");
				client.SetState( new STStandBy(client) );

				//	send disconnect to server, if we already registered on it.
				//	send several time to be sure, that packet sent :
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			public override void DispatchIM ( NetMessage msg )
			{
				if (msg.Command==NetCommand.Accepted) {

					Log.Message("Connection established.");

					netChan.Add( msg.SenderEP );
					client.SetState( new STConnected(client, msg.Text) );
					return;
				}

				if (msg.Command==NetCommand.Refused) {

					Log.Message("Connection refused: {0}", msg.Text);

					client.SetState( new STStandBy(client) );
					client.GameEngine.GameInterface.ShowError( msg.Text );
					netChan.Clear();
					return;
				}
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="gameTime"></param>
			public override void Update ( GameTime gameTime )
			{
				resendTimeout -= (int)gameTime.Elapsed.TotalMilliseconds;

				if (resendTimeout<0) {
					SendConnect();
				}
			}
		}
	}
}

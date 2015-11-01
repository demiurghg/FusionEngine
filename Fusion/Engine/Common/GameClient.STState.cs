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

		abstract class State {

			protected readonly GameClient client;
			protected readonly NetChan netChan;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public State( GameClient client ) 
			{
				this.client		=	client;
				this.netChan	=	client.netChan;
			}

			public abstract void Connect ( string host, int port );
			public abstract void Disconnect ();
			public abstract void DispatchIM ( NetMessage msg );
			public abstract void Update( GameTime gameTime );
		}



		/// <summary>
		/// StandBy client state - no connection, no game.
		/// </summary>
		class StandBy : State {

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public StandBy ( GameClient	client ) : base(client)
			{
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="host"></param>
			/// <param name="port"></param>
			public override void Connect ( string host, int port )
			{
				client.SetState( new Connecting(client, host, port) );
			}



			/// <summary>
			/// 
			/// </summary>
			public override void Disconnect ()
			{
				Log.Warning("Not connected.");
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			public override void DispatchIM ( NetMessage msg )
			{
				//	do nothing.
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="gameTime"></param>
			public override void Update ( GameTime gameTime )
			{
				//	do nothing.
			}
		}



		/// <summary>
		/// Client tries to connect to server.
		/// </summary>
		class Connecting : State {

			int maxAttempts = 10;
			int maxResendTimeout;
			int attemptCount = 0;
			int resendTimeout = 0;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public Connecting ( GameClient client, string host, int port ) : base(client)
			{
				maxAttempts			=	client.GameEngine.Network.Config.ResendMaxCount;
				maxResendTimeout	=	client.GameEngine.Network.Config.ResendTimeout;

				IPAddress ip;

				if (!IPAddress.TryParse( host, out ip)) {
					Log.Error("Failed to parse IP: '{0}'", host);
					client.SetState( new StandBy(client) );
					return;
				}

				ip	=	ip.MapToIPv6();

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
					client.SetState( new StandBy(client) );
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
				Log.Warning("Connection interrupted by user.");
				client.SetState( new StandBy(client) );
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			public override void DispatchIM ( NetMessage msg )
			{
				if (msg.Command==NetCommand.Accepted) {
					Log.Message("Connection established.");
					client.SetState( new Connected(client, msg.Text) );
					return;
				}

				if (msg.Command==NetCommand.Refused) {
					Log.Message("Connection refused: {0}", msg.Text);

					client.SetState( new StandBy(client) );
					client.GameEngine.GameInterface.ShowError( msg.Text );
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



		/// <summary>
		/// Client has connected, we need load game stuff.
		/// </summary>
		class Connected : State {

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public Connected ( GameClient client, string serverInfo ) : base(client)
			{
				Log.Message("Load level: {0}", serverInfo );
				client.LoadLevel( serverInfo );
			}



			/// <summary>
			/// 
			/// </summary>
			/// <param name="host"></param>
			/// <param name="port"></param>
			public override void Connect ( string host, int port )
			{
				Log.Warning("Already connected.");
			}


			/// <summary>
			/// 
			/// </summary>
			public override void Disconnect ()
			{
				Log.Warning("Can not interrupt game loading.");
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			public override void DispatchIM ( NetMessage msg )
			{
				if ( msg.Command==NetCommand.Dropped ) {
					Log.Message("Dropped.");
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
					client.SetState( new StandBy(client) );
				}

				if ( msg.Command==NetCommand.ServerDisconnected ) {
					Log.Message("Server disconnected.");
					client.GameEngine.GameInterface.ShowMessage("Server disconnected.");
					client.SetState( new StandBy(client) );
				}
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="gameTime"></param>
			public override void Update ( GameTime gameTime )
			{
				client.SetState( new Active(client) );
				//	do nothing
			}
		}



		/// <summary>
		/// Client is active.
		/// </summary>
		class Active : State {

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public Active ( GameClient	client ) : base(client)
			{
			}



			/// <summary>
			/// 
			/// </summary>
			/// <param name="host"></param>
			/// <param name="port"></param>
			public override void Connect ( string host, int port )
			{
				Log.Warning("Already connected.");
			}


			/// <summary>
			/// 
			/// </summary>
			public override void Disconnect ()
			{
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				Log.Message("Disconnected.");

				client.UnloadLevel();

				client.SetState( new StandBy(client) );
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			public override void DispatchIM ( NetMessage msg )
			{
				if ( msg.Command==NetCommand.Dropped ) {
					Log.Message("Dropped: {0}", msg.Text);
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
					client.SetState( new StandBy(client) );
				}

				if ( msg.Command==NetCommand.ServerDisconnected ) {
					Log.Message("Server disconnected.");
					client.GameEngine.GameInterface.ShowMessage("Server disconnected.");
					client.SetState( new StandBy(client) );
				}

				if ( msg.Command==NetCommand.Notification ) {
					Log.Message( msg.Text );
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
				}

				if ( msg.Command==NetCommand.ChatMessage ) {
					Log.Message( "Chat: " + msg.Text );
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
				}
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="gameTime"></param>
			public override void Update ( GameTime gameTime )
			{
				//	do nothing
			}
		}
	}
}

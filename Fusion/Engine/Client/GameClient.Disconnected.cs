using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Engine.Server;
using System.Net;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient {

		class Disconnected : State {

			public Disconnected ( GameClient gameClient, string reason ) : base(gameClient, ClientState.Disconnected)
			{
				Message	=	reason;

				//	Notify client that game ended?
				gameClient.UnloadContent();

				client.Shutdown( reason );
			}



			public override void UserConnect ( string host, int port )
			{
				Log.Warning("Wait stand by.");
			}



			public override void UserDisconnect ( string reason )
			{
				Log.Warning("Already disconnected.");
			}



			public override void Update ( GameTime gameTime )
			{
				//	fall immediatly to stand-by mode:
				gameClient.SetState( new StandBy( gameClient ) );
			}



			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{							
				
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
			}
		}
	}
}

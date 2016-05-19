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

		class StandBy : State {

			public StandBy ( GameClient gameClient ) : base(gameClient, ClientState.StandBy)
			{
				Message	=	"";
			}


			public override void UserConnect ( string host, int port )
			{
				var endPoint	=	new IPEndPoint( IPAddress.Parse(host), port );
				
				gameClient.SetState( new Connecting( gameClient, endPoint ) );	
			}



			public override void UserDisconnect ( string reason )
			{
				Log.Warning("Not connected.");
			}



			public override void Update ( GameTime gameTime )
			{
			}


			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
 				Log.Warning("Status chnaged while stand by: {0}", status);
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
			}
		}
	}
}

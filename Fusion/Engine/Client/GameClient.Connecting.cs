using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Engine.Network;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Engine.Server;
using System.Net;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient {

		class Connecting : State {

			public Connecting ( GameClient gameClient, IPEndPoint endPoint ) : base(gameClient, ClientState.Connecting)
			{
				client.Start();

				var hail	=	client.CreateMessage();
				hail.Write( gameClient.Guid.ToByteArray() );
				hail.Write( Encoding.UTF8.GetBytes(gameClient.UserInfo()) );

				client.Connect( endPoint, hail );
			}



			public override void UserConnect ( string host, int port )
			{
				Log.Warning("Connecting in progress");
			}



			public override void UserDisconnect ( string reason )
			{
				client.Disconnect( reason );
			}



			public override void Update ( GameTime gameTime )
			{
			}



			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
 				if (status==NetConnectionStatus.Connected) {
					gameClient.SetState( new Loading( gameClient, connection.RemoteHailMessage.PeekString() ) );
				}
 				if (status==NetConnectionStatus.Disconnected) {
					gameClient.SetState( new Disconnected( gameClient, message ) );
				}
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
			}
		}
	}
}

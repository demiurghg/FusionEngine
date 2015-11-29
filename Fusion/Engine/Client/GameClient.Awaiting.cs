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


namespace Fusion.Engine.Client {
	public abstract partial class GameClient {

		class Awaiting : State {

			public Awaiting ( GameClient gameClient ) : base(gameClient)
			{
				//	send user command to draw server attention:
				gameClient.SendUserCommand( client, new byte[0] );
			}



			public override void Connect ( string host, int port )
			{
				Log.Warning("Already connected. Waiting for snapshot.");
			}



			public override void Disconnect ( string reason )
			{
				client.Disconnect( reason );
			}



			public override void Update ( GameTime gameTime )
			{
			}


			
			public override void StatusChanged(NetConnectionStatus status, string message, NetConnection connection)
			{
 				if (status==NetConnectionStatus.Disconnected) {
					gameClient.SetState( new Disconnected(gameClient, message) );
				}
			}


			public override void DataReceived ( NetCommand command, NetIncomingMessage msg )
			{
				if (command==NetCommand.Snapshot) {
					var index		=	msg.ReadUInt32();
					var size		=	msg.ReadInt32();
					var snapshot	=	NetworkEngine.Decompress( msg.ReadBytes(size) );

					gameClient.SetState( new Active( gameClient, index, snapshot ) );
				}

				if (command==NetCommand.Notification) {
					gameClient.FeedNotification( msg.ReadString() );
				}
			}
		}
	}
}

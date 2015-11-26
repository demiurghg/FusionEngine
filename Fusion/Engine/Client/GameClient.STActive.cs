using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fusion.Engine.Network;
using System.Net;
using Fusion.Core.Shell;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Engine.Server;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient : GameModule {

		/// <summary>
		/// Client is active.
		/// </summary>
		class STActive : STState {

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public STActive ( GameClient client ) : base(client)
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
				//	send several time to be sure, that packet sent :
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				Log.Message("Disconnected.");

				client.UnloadLevel();

				client.SetState( new STStandBy(client) );
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
					netChan.Clear();
					client.SetState( new STStandBy(client) );
				}

				if ( msg.Command==NetCommand.ServerDisconnected ) {
					
					Log.Message("Server disconnected.");

					client.GameEngine.GameInterface.ShowMessage("Server disconnected.");
					netChan.Clear();
					client.SetState( new STStandBy(client) );
				}

				if ( msg.Command==NetCommand.Notification ) {
					Log.Message( msg.Text );
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
				}

				if ( msg.Command==NetCommand.ChatMessage ) {
					Log.Message( "Chat: " + msg.Text );
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
				}

				if ( msg.Command==NetCommand.Snapshot ) {
					//	assemble snapshot 
					//	and feed it to game when ready
					AssembleSnapshot( msg );
				}
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="gameTime"></param>
			public override void Update ( GameTime gameTime )
			{
				var userCmd	=	client.Update( gameTime );

				if (userCmd.Length>1024) {
					Log.Warning("User command > 1024 bytes. Ignored.");
					return;
				}

				var buffer = new byte[userCmd.Length + 8];

				using ( var stream = new MemoryStream(buffer) ) {
					using ( var writer = new BinaryWriter(stream) ) {

						writer.Write( commandCounter );
						writer.Write( userCmd.Length );
						writer.Write( userCmd );
					}

				}

				netChan.Transmit( client.serverEP, NetCommand.UserCommand, buffer, buffer.Length );
			}



			/*-------------------------------------------------------------------------------------
			 * 
			 *	Snapshot stuff
			 * 
			-------------------------------------------------------------------------------------*/

			int commandCounter	=	0;
			int lastSnapshotId	=	0;


			/// <summary>
			/// Snapshot which is ready to feed client-side game.
			/// </summary>
			byte[] builtSnapshot	=	new byte[ GameServer.SnapshotSize ];

			/// <summary>
			/// Currently assembling snapshot
			/// </summary>
			byte[] recvingSnapshot	=	new byte[ GameServer.SnapshotSize ];


			/// <summary>
			/// 
			/// </summary>
			/// <param name="message"></param>
			void AssembleSnapshot ( NetMessage message )
			{	
				client.FeedSnapshot( NetworkEngine.Decompress(message.Data) );
			}
		}
	}
}

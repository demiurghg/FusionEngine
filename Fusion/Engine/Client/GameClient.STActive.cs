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
				netChan.OutOfBand( client.serverEP, NetCommand.Disconnect );
				Log.Message("Disconnected.");

				client.UnloadLevel();

				client.SetState( new STStandBy(client) );
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			public override void DispatchIM ( NetIMessage msg )
			{
				if ( msg.Command==NetCommand.Dropped ) {
					Log.Message("Dropped: {0}", msg.Text);
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
					client.SetState( new STStandBy(client) );
				}

				if ( msg.Command==NetCommand.ServerDisconnected ) {
					Log.Message("Server disconnected.");
					client.GameEngine.GameInterface.ShowMessage("Server disconnected.");
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
				NetOMessage message = new NetOMessage(1024 + 16);

				using ( var stream = message.OpenWrite() ) {
					using ( var writer = new BinaryWriter(stream) ) {

						writer.Write( commandCounter );

						client.Update( gameTime, stream );
					}
				}
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
			void AssembleSnapshot ( NetIMessage message )
			{	
				
			}
		}
	}
}

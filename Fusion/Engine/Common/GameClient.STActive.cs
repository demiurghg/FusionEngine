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

		/// <summary>
		/// Client is active.
		/// </summary>
		class STActive : STState {

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public STActive ( GameClient	client ) : base(client)
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
			public override void DispatchIM ( NetMessage msg )
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

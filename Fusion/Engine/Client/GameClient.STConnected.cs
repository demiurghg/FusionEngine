using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fusion.Engine.Network;
using System.Net;
using Fusion.Engine.Common;
using Fusion.Core.Shell;


namespace Fusion.Engine.Client {
	public abstract partial class GameClient : GameModule {

		/// <summary>
		/// Client has connected, we need load game stuff.
		/// </summary>
		class STConnected : STState {

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public STConnected ( GameClient client, string serverInfo ) : base(client)
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
			public override void DispatchIM ( NetIMessage msg )
			{
				if ( msg.Command==NetCommand.Dropped ) {
					Log.Message("Dropped.");
					client.GameEngine.GameInterface.ShowMessage( msg.Text );
					client.SetState( new STStandBy(client) );
				}

				if ( msg.Command==NetCommand.ServerDisconnected ) {
					Log.Message("Server disconnected.");
					client.GameEngine.GameInterface.ShowMessage("Server disconnected.");
					client.SetState( new STStandBy(client) );
				}
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="gameTime"></param>
			public override void Update ( GameTime gameTime )
			{
				client.SetState( new STActive(client) );
				//	do nothing
			}
		}
	}
}

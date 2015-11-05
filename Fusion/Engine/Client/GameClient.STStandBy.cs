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
		/// StandBy client state - no connection, no game.
		/// </summary>
		class STStandBy : STState {

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public STStandBy ( GameClient	client ) : base(client)
			{
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="host"></param>
			/// <param name="port"></param>
			public override void Connect ( string host, int port )
			{
				client.SetState( new STConnecting(client, host, port) );
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
			public override void DispatchIM ( NetIMessage msg )
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
	}
}

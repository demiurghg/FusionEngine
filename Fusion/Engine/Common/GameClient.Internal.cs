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

		NetChan	netChan		=	null;
		STState	state;

		IPEndPoint	serverEP	=	null;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="newState"></param>
		void SetState ( STState newState )
		{
			state	=	newState;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void ConnectInternal ( string host, int port )
		{
			//	recreate NetChan to reset counters and internal state.
			SafeDispose( ref netChan );
			netChan	=	new NetChan( GameEngine, GameEngine.Network.ClientSocket, "CL" );

			state.Connect( host, port );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		internal void DisconnectInternal ()
		{
			state.Disconnect(); 
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		internal void UpdateInternal ( GameTime gameTime )
		{
			NetMessage msg;

			while ( netChan!=null && netChan.Dispatch(out msg) ) {

				//	dispatch messages only from server :
				if (NetUtils.IsIPsEqual( msg.SenderEP, serverEP ) ) {
					state.DispatchIM( msg );
				} else {
					Log.Warning("{0} sends commands. Expected from: {1}", msg.SenderEP, serverEP );
				}
			}

			state.Update( gameTime );
		}

	}
}

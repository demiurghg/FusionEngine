using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Core.Shell;
using System.Net;
using Fusion.Engine.Network;


namespace Fusion.Engine.Common {

	public abstract partial class GameServer : GameModule {

		class ClientState {
			
			/// <summary>
			/// Client's end point.
			/// </summary>
			public IPEndPoint EndPoint {
				get; private set;
			}

			/// <summary>
			/// Clients user info.
			/// </summary>
			public string UserInfo {
				get; set;
			}


			/// <summary>
			/// Gets unique string client ID.
			/// </summary>
			public string ID {
				get {
					return EndPoint.ToString();
				}
			}


			/// <summary>
			/// 
			/// </summary>
			/// <param name="ep"></param>
			/// <param name="userInfo"></param>
			public ClientState ( IPEndPoint ep, string userInfo )
			{
				EndPoint	= ep;
				UserInfo	= userInfo;
			}

		}


	}
}

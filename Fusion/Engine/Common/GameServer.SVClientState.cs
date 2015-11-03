using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Core.Shell;
using System.Net;
using System.IO;
using Fusion.Engine.Network;


namespace Fusion.Engine.Common {

	public abstract partial class GameServer : GameModule {

		class SVClientState {
			
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


			readonly GameServer	server;


			/// <summary>
			///	Indicates that client have sent command and awaiting for snapshot;
			///	Intial value is TRUE to tell server that client just connected and need snapshot.
			/// </summary>
			public bool NeedSnapshot = true;


			/// <summary>
			/// 
			/// </summary>
			/// <param name="ep"></param>
			/// <param name="userInfo"></param>
			public SVClientState ( GameServer server, IPEndPoint ep, string userInfo )
			{
				this.server	=	server;
				EndPoint	=	ep;
				UserInfo	=	userInfo;
			}



			/// <summary>
			/// 
			/// </summary>
			public void SendSnapshot ( Stream stream )
			{
				if (!NeedSnapshot) {
					return;
				}

				
			}



		}


	}
}

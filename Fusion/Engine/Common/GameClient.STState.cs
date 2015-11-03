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

		abstract class STState {

			protected readonly GameClient client;
			protected readonly NetChan netChan;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="client"></param>
			public STState( GameClient client ) 
			{
				this.client		=	client;
				this.netChan	=	client.netChan;
			}

			public abstract void Connect ( string host, int port );
			public abstract void Disconnect ();
			public abstract void DispatchIM ( NetIMessage msg );
			public abstract void Update( GameTime gameTime );
		}

	}
}

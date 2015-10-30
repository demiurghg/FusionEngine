using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Shell;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using System.Net;
using System.Net.Sockets;
using Lidgren.Network;

namespace Fusion.Engine.Network {

	public class NetworkEngine : GameModule {

		[Config]
		public NetworkConfig Config { get; set; }


		/// <summary>
		/// Server socket
		/// </summary>
		public Socket ServerSocket { 
			get {
				return serverSocket;
			}
		}


		/// <summary>
		/// Client socket
		/// </summary>
		public Socket ClientSocket { 
			get { 
				return clientSocket;
			}
		}


		Socket serverSocket;
		Socket clientSocket;



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		internal NetworkEngine ( GameEngine gameEngine ) : base( gameEngine )
		{
			Config	=	new NetworkConfig();
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			Log.Message("  local IP : {0}", GetLocalIPAddress());

			Log.Message("  network  : {0}", System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() );

			//	create server socket :
			serverSocket			=	new Socket( SocketType.Dgram, ProtocolType.Udp );
			serverSocket.Blocking	=	false;

			serverSocket.Bind( new IPEndPoint( IPAddress.Any, Config.Port ) );
			Log.Message("  server   : {0}", serverSocket.LocalEndPoint );

			//	create client socket :
			clientSocket			=	new Socket( SocketType.Dgram, ProtocolType.Udp );
			clientSocket.Blocking	=	false;

			clientSocket.Bind( new IPEndPoint(IPAddress.Any, 0) );
			Log.Message("  client   : {0}", clientSocket.LocalEndPoint );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref clientSocket );
				SafeDispose( ref serverSocket );
			}
			base.Dispose( disposing );
		}

	
		
		/// <summary>
		/// http://stackoverflow.com/questions/6803073/get-local-ip-address
		/// </summary>
		public string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (var ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					return ip.ToString();
				}
			}

			throw new Exception("Local IP Address Not Found!");
		}
	}

}

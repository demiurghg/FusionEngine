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
using System.IO.Compression;
using System.IO;

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
			serverSocket				=	new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
			serverSocket.Blocking		=	false;
			serverSocket.DontFragment	=	true;
			Log.Message("  recv buf : {0}", serverSocket.ReceiveBufferSize );

			int port = Config.Port;

			for (int i=0; i<10; i++) {
				try {			   
					Log.Message("  try port : {0}", port );
					serverSocket.Bind( new IPEndPoint( IPAddress.Any, port ) );
					Log.Message("  server   : {0}", serverSocket.LocalEndPoint );
					break;

				} catch ( SocketException ne ) {
					if (ne.SocketErrorCode==SocketError.AddressAlreadyInUse) {
						port++;
					}
					if (i>=9) {
						throw;
					}
				}
			}

			//	create client socket :
			clientSocket			=	new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
			clientSocket.Blocking	=	false;
			serverSocket.DontFragment	= true;

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




		/// <summary>
		/// Compresses byte array.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Compress(byte[] data)
		{
			using (var compressedStream = new MemoryStream()) {

				using (var zipStream = new DeflateStream(compressedStream, CompressionLevel.Optimal)) {

					zipStream.Write(data, 0, data.Length);
					zipStream.Close();
					return compressedStream.ToArray();
				}
			}
		}



		/// <summary>
		/// Decompresses byte array
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Decompress(byte[] data)
		{
			using (var compressedStream = new MemoryStream(data)) {

				using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress)) {

					using (var resultStream = new MemoryStream()) {

						zipStream.CopyTo(resultStream);
						return resultStream.ToArray();
					}
				}
			}
		}
	}

}

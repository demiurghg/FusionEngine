using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;


namespace Fusion.Engine.Network {

	/// <summary>
	/// 
	/// Net Channel reponces for:
	/// 
	///		1. Reliability
	///		2. Fragmentation
	///		3. Encryption
	///		
	/// 
	/// Net Channel header structure :
	/// 
	///		[RELIABLE (1)] [SEQUENCE NUMBER (31)          ]	- 0xFFFFFFFF means out of band message
	///		[ACKNOLEDGMENT                                ]
	///		[QPORT (16)  ] [FRAGMENT COUNT|TOTAL FRAGMENTS]	- QPort is Quake legacy. Fragment ID???
	///		[PAYLOAD......................................]
	///		
	/// </summary>
	public class NetChan {

		/// <summary>
		/// Port
		/// </summary>
		public readonly ushort QPort;

		/// <summary>
		/// Maximal transmition unit.
		/// </summary>
		public const int MTU	=	1400;


		/// <summary>
		/// Creates instance of NetChan with given name and bound to particular socket.
		/// </summary>
		/// <param name="socket">Socket which NetChan bound to.</param>
		/// <param name="name">NetChan name</param>
		public NetChan ( Socket socket, string name )
		{
			QPort	=	(ushort)(socket.LocalEndPoint as IPEndPoint).Port;
			Log.Message("Netchan {0} : {1}", name, QPort );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="target"></param>
		/// <param name="data"></param>
		public void OutOfBand ( Socket socket, IPEndPoint remoteEP, byte[] data )
		{		
			var header = new NetChanHeader( QPort );
			socket.SendTo( header.MakeDatagram(data), remoteEP );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void Transmit ( Socket socket, IPEndPoint to, byte[] data, bool reliable )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public Datagram Recv ()
		{
			throw new NotImplementedException();
		}



		/// <summary>
		/// 
		/// raise exception, if discovery in progress.
		/// </summary>
		/// <param name="timeoutMSec"></param>
		public void RequestDiscovery ( int timeoutMSec )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string DiscoveryResponce ()
		{
			throw new NotImplementedException();
		}
	}
}

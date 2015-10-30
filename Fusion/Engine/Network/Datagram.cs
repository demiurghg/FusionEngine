using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace Fusion.Engine.Network {

	public class Datagram {

		/// <summary>
		/// 
		/// </summary>
		public IPEndPoint From {
			get; private set;
		}


		/// <summary>
		/// Indicates, hat this datagram contains reliable data.
		/// </summary>
		public bool Reliable {
			get; private set;
		}

		
		/// <summary>
		/// Sequence number.
		/// </summary>
		public int Sequence {
			get; private set;
		}


		/// <summary>
		/// Datagram's data
		/// </summary>
		public byte[] Data {
			get; private set;
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="from"></param>
		/// <param name="reliable"></param>
		/// <param name="sequenceNumber"></param>
		/// <param name="data"></param>
		internal Datagram ( IPEndPoint from, bool reliable, int sequence, byte[] data )
		{		
			From		=	from;

			Reliable	=	reliable;
			Sequence	=	sequence;

			Data		=	data;
		}
	}
}

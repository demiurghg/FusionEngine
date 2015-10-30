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
		/// NetChan header.
		/// </summary>
		public NetChanHeader Header { get; private set; }


		/// <summary>
		/// Sender.
		/// </summary>
		public IPEndPoint Sender { get; private set; }


		/// <summary>
		/// Message data.
		/// </summary>
		public byte[] Data { get; private set; }

		

		/// <summary>
		/// Converts Data to ASCII string.
		/// </summary>
		public string ASCII { 
			get {
				return Encoding.ASCII.GetString( Data );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="from"></param>
		/// <param name="reliable"></param>
		/// <param name="sequenceNumber"></param>
		/// <param name="data"></param>
		internal Datagram ( NetChanHeader header, IPEndPoint sender, byte[] recievedData, int receivedSize )
		{		
			Header		=	header;
			Sender		=	sender;

			int	length	=	receivedSize - NetChanHeader.SizeInBytes;

			Data		=	new byte[length];

			Buffer.BlockCopy( recievedData, NetChanHeader.SizeInBytes, Data, 0, length );
		}	   


		internal void Print ()
		{
			Log.Message("Datagram:");
			Log.Message("  sequence   = {0}", Header.Sequence		);
			Log.Message("  ack        = {0}", Header.AckSequence	);
			Log.Message("  frag       = {0}", Header.Fragment		);
			Log.Message("  frag count = {0}", Header.FragmentCount	);
			Log.Message("  msg type   = {0}", Header.MsgType		);
			Log.Message("  length     = {0}", Data.Length );

		}
	}
}

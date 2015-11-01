using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace Fusion.Engine.Network {

	public class NetMessage {

		/// <summary>
		/// NetChan header.
		/// </summary>
		public NetChanHeader Header { get; private set; }


		/// <summary>
		/// Sender.
		/// </summary>
		public IPEndPoint SenderEP { get; private set; }


		/// <summary>
		/// Message data.
		/// </summary>
		public byte[] Data { get; private set; }


		/// <summary>
		/// Gets sender's endpoint as string.
		/// </summary>
		public string Address {
			get {
				return SenderEP.ToString();
			}
		}


		/// <summary>
		/// Gets header command.
		/// </summary>
		public NetCommand Command {
			get {
				return Header.Command;
			}
		}
		

		/// <summary>
		/// Gets data as a text (e.g. converts byte array to string).
		/// </summary>
		public string Text {
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
		internal NetMessage ( NetChanHeader header, IPEndPoint sender, byte[] recievedData, int receivedSize )
		{		
			Header		=	header;
			SenderEP	=	sender;

			int	length	=	receivedSize - NetChanHeader.SizeInBytes;

			Data		=	new byte[length];

			Buffer.BlockCopy( recievedData, NetChanHeader.SizeInBytes, Data, 0, length );
		}	   


		/// <summary>
		/// 
		/// </summary>
		internal void Print ()
		{
			Log.Message("Datagram:");
			Log.Message("  sequence    = {0}", Header.Sequence		);
			Log.Message("  ack         = {0}", Header.AckSequence	);
			Log.Message("  frag        = {0}", Header.Fragment		);
			Log.Message("  frag count  = {0}", Header.FragmentCount	);
			Log.Message("  out-of-band = {0}", Header.IsOutOfBand	);
			Log.Message("  command     = {0}", Header.Command		);
			Log.Message("  length      = {0}", Data.Length );

		}
	}
}

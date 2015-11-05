using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace Fusion.Engine.Network {

	public class NetMessage {

		/// <summary>
		/// NetChan header.
		/// </summary>
		public NetChanHeader Header { get; private set; }


		/// <summary>
		/// Sender's end point.
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
		internal NetMessage ( IPEndPoint sender, byte[] recievedData, int receivedSize )
		{		
			int	length	=	receivedSize - NetChanHeader.SizeInBytes;

			SenderEP	=	sender;
			Header		=	new NetChanHeader();
			Data		=	new byte[ length ];

			using ( var stream = new MemoryStream(recievedData) ) {
				using ( var reader = new BinaryReader(stream) ) {
					Header.Read( reader );
				}
			}
			Buffer.BlockCopy( recievedData, NetChanHeader.SizeInBytes, Data, 0, length );
		}	   
	}
}

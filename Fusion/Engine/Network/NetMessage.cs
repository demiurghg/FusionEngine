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
		/// Indicates that message is fragmented.
		/// </summary>
		public bool IsFragmented {
			get {
				return Header.FragmentCount > 1;
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
		

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="header"></param>
		/// <param name="data"></param>
		NetMessage ( NetChanHeader header, IPEndPoint sender, byte[] data )
		{
			this.Header		=	header;
			this.SenderEP	=	sender;
			this.Data		=	data;
		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fragments"></param>
		/// <returns></returns>
		static internal NetMessage Compose ( IEnumerable<NetMessage> fragments )
		{
			if (fragments==null) {
				throw new ArgumentNullException("fragments");
			}
			if (!fragments.Any()) {
				return null;
			}

			NetChanHeader	header	=	fragments.First().Header;

			int totalSize	=	fragments.Sum( f => f.Data.Length );
			var buffer		=	new byte[ totalSize ];

			int offset		=	0;
			var fragCount	=	fragments.First().Header.FragmentCount;

			foreach ( var fragment in fragments ) {
				if (fragCount!=fragment.Header.FragmentCount) {
					Log.Warning("Bad fragmented packet sequence");
					return null;
				}
				Buffer.BlockCopy( fragment.Data, 0, buffer, offset, fragment.Data.Length );
				offset += fragment.Data.Length;
			}

			
			header.FragmentCount	=	1;
			header.FragmentID		=	0;

			return new NetMessage( header, fragments.First().SenderEP, buffer );
		}
		  
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace Fusion.Engine.Network {

	public class NetOMessage {

		readonly byte[] buffer;
		
		/// <summary>
		/// Gets capacity of outgoing message.
		/// </summary>
		public int Capacity {
			get;
			private set;
		}



		/// <summary>
		/// Gets buffer with raw data (including header).
		/// </summary>
		internal byte[] Bytes {
			get {
				return buffer;
			}
		}



		const int MaxMsgSize = NetChan.MTU - NetChanHeader.SizeInBytes;



		/// <summary>
		/// Creates instance of NetOMessage
		/// </summary>
		/// <param name="capacity"></param>
		public NetOMessage ( int capacity )
		{
			if (capacity<0) {
				throw new ArgumentOutOfRangeException("capacity", "capacity must be > 0");
			}
			if (capacity>MaxMsgSize) {
				throw new ArgumentOutOfRangeException("capacity", "capacity must be < " + MaxMsgSize.ToString() );
			}

			buffer		=	new byte [ NetChanHeader.SizeInBytes + capacity ];
			Capacity	=	capacity;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public void SetData<T>( T[] data, int offset, int count )
		{
			Buffer.BlockCopy( data, offset, buffer, NetChanHeader.SizeInBytes, count );
		}



		/// <summary>
		/// Opens memory stream for writing.
		/// This method leave space in front of buffer for header.
		/// </summary>
		/// <returns></returns>
		public Stream OpenWrite ()
		{
			return new MemoryStream( buffer, NetChanHeader.SizeInBytes, Capacity, true );
		}


		/// <summary>
		/// Opens binary writer. 
		/// </summary>
		/// <returns></returns>
		public BinaryWriter OpenWriter ()
		{
			return new BinaryWriter( OpenWrite() );
		}



		/// <summary>
		/// Writes header to outgoing message.
		/// </summary>
		/// <param name="header"></param>
		internal void WriteHeader ( NetChanHeader header )
		{
			using ( var stream = new MemoryStream( buffer, 0, NetChanHeader.SizeInBytes, true ) ) {
				using ( var writer = new BinaryWriter( stream ) ) {
					header.Write( writer );
				}
			}
		}

	}
}

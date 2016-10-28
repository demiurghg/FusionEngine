using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Mathematics;
using System.Runtime.InteropServices;
using SharpDX;

namespace Fusion.Core.Extensions {
	public static class StreamExtensions {


		/// <summary>
		/// Writes FourCC to stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="fourCC"></param>
		public static void WriteFourCC ( this Stream stream, string fourCC )
		{
			if (fourCC==null) {
				throw new ArgumentNullException("fourCC");
			}
			if (fourCC.Length!=4) {
				throw new ArgumentException("fourCC must contain exactly four characters");
			}

			var data = Encoding.ASCII.GetBytes( fourCC ).Take(4).ToArray();

			stream.Write( data, 0, 4 );
		}



		/// <summary>
		/// Reads FourCC from string.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static string ReadFourCC ( this Stream stream )
		{
			var fourCC = new byte[4];

			stream.Read( fourCC, 0, 4 );

			return Encoding.ASCII.GetString( fourCC );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream"></param>
		/// <param name="array"></param>
		/// <param name="count"></param>
		public static void ReadToStructureArray<T> ( this Stream stream, T[] array, int count ) where T: struct
		{
			var dataSize		=	count * Marshal.SizeOf(typeof(T));
			var buffer			=	new byte[dataSize];
			
			stream.Read( buffer, 0, dataSize );

			var handle			= GCHandle.Alloc( buffer, GCHandleType.Pinned );
			var dataStream		= new DataStream( handle.AddrOfPinnedObject(), buffer.Length, true, false );
			
			dataStream.ReadRange<T>( array, 0, count );

			dataStream.Dispose();
			handle.Free();
		}



		/// <summary>
		/// Reads the contents of the stream into a byte array.
		/// data is returned as a byte array. An IOException is
		/// thrown if any of the underlying IO calls fail.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <returns>A byte array containing the contents of the stream.</returns>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
		/// <exception cref="System.IO.IOException">Anor occurs.</exception>
		public static byte[] ReadAllBytes( this Stream source )
		{
			byte[] readBuffer = new byte[4096];
 
			int totalBytesRead = 0;
			int bytesRead;
 
			while ((bytesRead = source.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
			{
				totalBytesRead += bytesRead;
 
				if (totalBytesRead == readBuffer.Length)
				{
					int nextByte = source.ReadByte();
					if (nextByte != -1)
					{
						byte[] temp = new byte[readBuffer.Length * 2];
						Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
						Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
						readBuffer = temp;
						totalBytesRead++;
					}
				}
			}
 
			byte[] buffer = readBuffer;
			if (readBuffer.Length != totalBytesRead)
			{
				buffer = new byte[totalBytesRead];
				Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
			}
			return buffer;
		}


	}
}

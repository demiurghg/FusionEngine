using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using Fusion.Core.Content;


namespace Fusion.Core.Extensions {
	public static class BinaryReaderExtensions {

		public static void ExpectFourCC ( this BinaryReader reader, string fourCC, string what )
		{
			var readFourCC = reader.ReadFourCC();

			if (readFourCC!=fourCC) {
				throw new IOException(string.Format("Bad {2}: expected {0}, got {1}", fourCC, readFourCC, what)); 
			}
		}


		public static string ReadFourCC ( this BinaryReader reader )
		{
			return ContentUtils.MakeFourCC( reader.ReadUInt32() );
		}


		public static void Read<T> ( this BinaryReader reader, T[] array, int count ) where T: struct
		{
			var dataSize		=	count * Marshal.SizeOf(typeof(T));
			var buffer			=	new byte[dataSize];
			
			reader.Read( buffer, 0, dataSize );

			var handle			= GCHandle.Alloc( buffer, GCHandleType.Pinned );
			var dataStream		= new DataStream( handle.AddrOfPinnedObject(), buffer.Length, true, false );
			
			dataStream.ReadRange<T>( array, 0, count );

			dataStream.Dispose();
			handle.Free();
		}




		public static T[] Read<T> ( this BinaryReader reader, int count ) where T : struct
		{
			var buffer			= reader.ReadBytes( count * Marshal.SizeOf(typeof(T)) );
			var elementCount	= count;
			var handle			= GCHandle.Alloc( buffer, GCHandleType.Pinned );
			var dataStream		= new DataStream( handle.AddrOfPinnedObject(), buffer.Length, true, false );
			
			var range		= dataStream.ReadRange<T>( elementCount );

			dataStream.Dispose();
			handle.Free();

			return range;
		}



		public static T Read<T> ( this BinaryReader reader ) where T : struct
		{
			var size	=	Marshal.SizeOf( typeof( T ) );
			var bytes	=	reader.ReadBytes( size ); 

			var handle	=	GCHandle.Alloc( bytes, GCHandleType.Pinned );

			T structure	=	(T)Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof(T) );

			handle.Free();
			
			return structure;
		}
	}
}

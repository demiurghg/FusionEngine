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
	public static class BinaryWriterExtensions {

		public static void WriteFourCC ( this BinaryWriter writer, string fourCC )
		{
			writer.Write( ContentUtils.MakeFourCC(fourCC) );
		}



		static void Write<T>( BinaryWriter writer, object src, int elementCount )
		{
			var size = elementCount * Marshal.SizeOf( typeof( T ) );
			var buffer = new byte[ size ];
			var handle = GCHandle.Alloc( src, GCHandleType.Pinned );
			var ds = new DataStream( handle.AddrOfPinnedObject(), size, true, false );
			ds.ReadRange( buffer, 0, size );
			writer.Write( buffer );
			ds.Dispose();
			handle.Free();
		}



		public static void Write<T>( this BinaryWriter writer, T structure ) where T : struct
		{
			Write<T>( writer, structure, 1 );
		}



		public static void Write<T>( this BinaryWriter writer, T[] array ) where T : struct
		{
			Write<T>( writer, array, array.Length );
		}
	}
}

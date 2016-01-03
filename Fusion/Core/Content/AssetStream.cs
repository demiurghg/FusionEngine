using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Fusion.Core.Content {
	public class AssetStream : Stream {

		const string AssetSignature	=	"AST1";
		const string DataSignature	=	"DATA";


		enum Mode {
			Read,
			Write,
		}


		readonly Mode mode;


		/// <summary>
		/// Opens asset file for reading.
		/// Immidiatly reads asset attributes.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static AssetStream OpenRead ( string path )
		{
			return new AssetStream( path );
		}



		/// <summary>
		/// Opens asset file for writing.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static AssetStream OpenWrite ( string fullTargetPath, string buildParameters, string[] dependencies )
		{
			return new AssetStream( fullTargetPath, buildParameters, dependencies );
		}


		/// <summary>
		//  Gets a value indicating whether the current stream supports reading.
		/// </summary>
		public override bool CanRead {
			get { return (mode == Mode.Read); }
		}


		/// <summary>
		//  Gets a value indicating whether the current stream supports writing.
		/// </summary>
		public override bool CanWrite {
			get { return (mode == Mode.Write); }
		}


		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		public override bool CanSeek {
			get { return false; }
		}


		/// <summary>
		/// Set and get position is not supported.
		/// </summary>
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}


		/// <summary>
		/// Length is not supported.
		/// </summary>
		public override long Length	{
			get { throw new NotSupportedException(); }
		}



		/// <summary>
		/// Gets asset's build parameters.
		/// </summary>
		public string BuildParameters {
			get; private set;
		}


		/// <summary>
		/// Gets list of dependencies.
		/// </summary>
		public string[] Dependencies {
			get; private set;
		}


		Stream zipStream;
		Stream fileStream;


		/// <summary>
		/// Creates asset file for writing.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="buildParameters"></param>
		/// <param name="dependencies"></param>
		private AssetStream ( string fullTargetPath, string buildParameters, string[] dependencies )
		{
			mode	=	Mode.Write;
			
			if (fullTargetPath==null) {
				throw new ArgumentNullException("fullTargetPath");
			}
			if (buildParameters==null) {
				throw new ArgumentNullException("buildParameters");
			}
			if (dependencies==null) {
				throw new ArgumentNullException("dependencies");
			}

			if (!dependencies.Any() ) {
				throw new ArgumentException("Dependencies must contain at least one entry");
			}

			if (dependencies.Any( p => Path.IsPathRooted(p) ) ) {
				throw new ArgumentException("Dependencies contains rooted path");
			}

			BuildParameters	=	buildParameters;
			Dependencies	=	dependencies
								.Select( d => ContentUtils.BackslashesToSlashes(d) )
								.ToArray();


			fileStream	=	File.Open( fullTargetPath, FileMode.Create, FileAccess.Write );

			using (var writer = new BinaryWriter(fileStream, Encoding.UTF8, true) ) {
				writer.Write( ContentUtils.MakeFourCC( AssetSignature ) );
				writer.Write( BuildParameters );
				writer.Write( Dependencies.Length );
				foreach ( var dep in Dependencies ) {
					writer.Write(dep);
				}

				writer.Write( ContentUtils.MakeFourCC( DataSignature ) );
			}

			zipStream	=	new DeflateStream( fileStream, CompressionLevel.Optimal, true );
		}


		/// <summary>
		/// Opens asset stream for reading.
		/// </summary>
		/// <param name="path"></param>
		private AssetStream ( string path )
		{
			mode	=	Mode.Read;

			fileStream	=	File.Open( path, FileMode.Open, FileAccess.Read );

			using (var reader = new BinaryReader(fileStream, Encoding.UTF8, true) ) {

				if ( reader.ReadUInt32() != ContentUtils.MakeFourCC( AssetSignature ) ) {
					throw new IOException("Bad asset file signature. " + AssetSignature + " is expected. Rebuild content.");
				}

				BuildParameters	=	reader.ReadString();

				int depsCount	=	reader.ReadInt32();
				Dependencies	=	new string[ depsCount ];

				for (int i=0; i<depsCount; i++) {
					Dependencies[i] = reader.ReadString();
				}

				if ( reader.ReadUInt32() != ContentUtils.MakeFourCC( DataSignature ) ) {
					throw new IOException("Bad data section signature. " + DataSignature + " is expected. Rebuild content.");
				}
			}

			zipStream	=	new DeflateStream( fileStream, CompressionMode.Decompress, true );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				DisposableBase.SafeDispose( ref zipStream );
				DisposableBase.SafeDispose( ref fileStream );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Overriden. Seek is not supported.
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		public override long Seek ( long offset, SeekOrigin origin ) 
		{
			throw new NotSupportedException();
		}


		/// <summary>
		/// Overriden. SetLength is not supported.
		/// </summary>
		/// <param name="value"></param>
		public override void SetLength ( long value )
		{
			throw new NotSupportedException();
		}


		/// <summary>
		/// Writes byte array to stream.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public override void Write ( byte[] buffer, int offset, int count )
		{
			zipStream.Write( buffer, offset, count );
		}


		/// <summary>
		/// Reads byte array from stream.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public override int Read ( byte[] buffer, int offset, int count )
		{
			return zipStream.Read( buffer, offset, count );
		}


		/// <summary>
		/// Flushes underlaying streams.
		/// </summary>
		public override void Flush () 
		{
			zipStream.Flush();
			fileStream.Flush();
		}


	}
}

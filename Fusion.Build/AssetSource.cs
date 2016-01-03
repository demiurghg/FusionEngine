using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Fusion;
using Fusion.Core.Content;
using Fusion.Build.Processors;

namespace Fusion.Build {

	public class AssetSource {
			
		string fullPath;
		string keyPath;
		string baseDir;
		string outputDir;

		/// <summary>
		/// Logical relativee path to file to be built.
		/// </summary>
		public string KeyPath { 
			get { 
				return keyPath; 
			} 
		}

		/// <summary>
		/// Full actual path to asset file
		/// </summary>
		public string FullSourcePath { 
			get { 
				return fullPath; 
			} 
		}

		/// <summary>
		/// Base directory for this file
		/// </summary>
		public string BaseDirectory { 
			get { return baseDir; } 
		}



		/// <summary>
		/// Gets target file name.
		/// </summary>
		public string TargetName {
			get {
				return ContentUtils.GetHashedFileName( KeyPath, ".asset" );
			}
		}



		/// <summary>
		/// Gets filename hash.
		/// </summary>
		public string Hash {
			get {
				return ContentUtils.GetHashedFileName( KeyPath, "" );
			}
		}


		/// <summary>
		/// Full target file path
		/// </summary>
		public string FullTargetPath {
			get {
				return Path.Combine( outputDir, TargetName );
			}
		}


		/// <summary>
		/// Is asset up-to-date.
		/// </summary>
		public bool IsUpToDate {
			get {
				if ( !File.Exists( FullTargetPath ) ) {
					return false;
				}

				var targetTime	=	File.GetLastWriteTime( FullTargetPath );
				var sourceTime	=	File.GetLastWriteTime( FullSourcePath );

				if (targetTime < sourceTime) {
					return false;
				}

				return true;
			}
		}


		/// <summary>
		/// Gets build parameters.
		/// </summary>
		public string[] BuildArguments {
			get; private set;
		}


		readonly Type assetProcessorType;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyPath">Key path</param>
		/// <param name="baseDir">Base directory</param>
		/// <param name="buildParameters"></param>
		/// <param name="context"></param>
		public AssetSource ( string keyPath, string baseDir, Type assetProcessorType, string[] buildArgs, BuildContext context )
		{
			this.assetProcessorType	=	assetProcessorType;
			this.outputDir			=	context.Options.FullOutputDirectory;
			this.fullPath			=	Path.Combine( baseDir, keyPath );
			this.baseDir			=	baseDir;
			this.keyPath			=	keyPath;
			this.BuildArguments		=	buildArgs;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public AssetProcessor CreateProcessor()
		{
			return (AssetProcessor)Activator.CreateInstance( assetProcessorType );
		} 



		/// <summary>
		/// 
		/// </summary>
		/// <param name="dependencies"></param>
		/// <returns></returns>
		public Stream OpenTargetStream ( IEnumerable<string> dependencies )
		{
			return AssetStream.OpenWrite( FullTargetPath, "", dependencies.Concat( new[]{ KeyPath } ).Distinct().ToArray() );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="dependencies"></param>
		/// <returns></returns>
		public Stream OpenTargetStream ()
		{
			return OpenTargetStream( new string[0] );
		}


		/// <summary>
		/// Opens source stream file
		/// </summary>
		/// <returns></returns>
		public Stream OpenSourceStream ()
		{	
			return File.OpenRead( FullSourcePath );
		}
	}
}

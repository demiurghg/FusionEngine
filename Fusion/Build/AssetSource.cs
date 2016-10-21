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
		readonly BuildContext context;

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
		/// 
		/// </summary>
		public bool TargetFileExists {
			get {
				return File.Exists( FullTargetPath );
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
				var buildArgs	=	string.Join(" ", BuildArguments);

				using ( var assetStream = AssetStream.OpenRead( FullTargetPath ) ) {
					
					if (assetStream.BuildParameters!=buildArgs) {
						return false;
					}

					foreach ( var dependency in assetStream.Dependencies ) {
						if ( context.ContentFileExists(dependency) ) {
							var fullDependencyPath = context.ResolveContentPath(dependency);

							var sourceTime	=	File.GetLastWriteTime(fullDependencyPath);

							if (targetTime < sourceTime) {
								return false;
							}
						}
					}
				}

				return true;
			}
		}



		public IEnumerable<string> GetAllDependencies()
		{
			var removedDeps = new List<string>();

			using ( var assetStream = AssetStream.OpenRead( FullTargetPath ) ) {
				return assetStream.Dependencies;
			}
		}



		/// <summary>
		/// Return list of key path to changed content file.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetRemovedDependencies ()
		{
			var removedDeps = new List<string>();

			using ( var assetStream = AssetStream.OpenRead( FullTargetPath ) ) {
					
				foreach ( var dependency in assetStream.Dependencies ) {

					if ( !context.ContentFileExists(dependency) ) {
						removedDeps.Add( dependency );
					}
				}
			}

			return removedDeps;
		}



		/// <summary>
		/// Return list of key path to changed content file.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetChangedDependencies ()
		{
			var changedDeps = new List<string>();

			var targetTime	=	File.GetLastWriteTime( FullTargetPath );

			using ( var assetStream = AssetStream.OpenRead( FullTargetPath ) ) {
					
				foreach ( var dependency in assetStream.Dependencies ) {

					if ( context.ContentFileExists(dependency) ) {

						var fullDependencyPath = context.ResolveContentPath(dependency);

						var sourceTime	=	File.GetLastWriteTime(fullDependencyPath);

						if (targetTime < sourceTime) {
							changedDeps.Add( dependency );
						}
					}
				}
			}

			return changedDeps;
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
			this.context			=	context;
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
			var args	=	string.Join(" ", BuildArguments);
			return AssetStream.OpenWrite( FullTargetPath, args, dependencies.Concat( new[]{ KeyPath } ).Distinct().ToArray() );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ZipArchive OpenAssetArchive ()
		{
			return ZipFile.Open( Path.ChangeExtension(FullTargetPath, ".zip"), ZipArchiveMode.Update );
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

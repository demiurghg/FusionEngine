using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Fusion;
using Fusion.Core.Content;

namespace Fusion.Build {

	public class AssetSource {
			
		string fullPath;
		string keyPath;
		string outputDir;
		string basePath;
		bool processed;

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
		public string BasePath { 
			get { 
				return basePath; 
			} 
		}

		/// <summary>
		/// Is file have been proceessed.
		/// Could be assigned to TRUE only.
		/// </summary>
		public bool IsProcessed { 
			get { return processed; }
			set {
				if (value==false) {
					throw new BuildException("BuildFile.IsProcessed may be assigned to True only");
				}
				processed = value;
			}
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
		/// 
		/// </summary>
		/// <param name="fullPath">Full path to this asset file</param>
		/// <param name="contentDir">Directory where file had been found.</param>
		/// <param name="context">Build context</param>
		public AssetSource ( string fullPath, string contentDir, BuildContext context )
		{
			this.outputDir		=	context.Options.FullOutputDirectory;
			this.fullPath		=	fullPath;
			this.basePath		=	contentDir;
			this.keyPath		=	ContentUtils.BackslashesToSlashes( ContentUtils.MakeRelativePath( contentDir + "\\", fullPath ) );

			this.processed		=	false;
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

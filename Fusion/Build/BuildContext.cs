using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion.Build.Processors;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using Fusion;
using Fusion.Engine.Storage;
using Fusion.Core.Content;
using Microsoft.Win32;

namespace Fusion.Build {

	public class BuildContext {

		/// <summary>
		/// 
		/// </summary>
		public BuildOptions Options {
			get; private set;
		}


		List<string> contentPaths	;
		List<string> binaryPaths	;

		/// <summary>
		/// All content directories
		/// </summary>
		public IEnumerable<string> ContentDirectories {
			get {
				return contentPaths;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		public BuildContext ( BuildOptions options, IniData iniData )
		{
			this.Options	=	options;

			Log.Message("Source directories:");

				contentPaths	=	new List<string>();
				contentPaths.Add( options.FullInputDirectory );
				contentPaths.AddRange( GetAllKeysFromSection( iniData, "ContentDirectories" ).Select( p => ResolveDirectory( p )).Where( p1 => p1!=null ) );

				foreach ( var dir in contentPaths ) {
					Log.Message("  {0}", dir );
				}

			Log.Message("");


			Log.Message("Binary directories:");
				binaryPaths	=	new List<string>();
				binaryPaths.AddRange( GetAllKeysFromSection( iniData, "BinaryDirectories" ).Select( p => ResolveDirectory( p )).Where( p1 => p1!=null ) );

				foreach ( var dir in binaryPaths ) {
					Log.Message("  {0}", dir );
				}
			Log.Message("");


			
			Log.Message("Target directory:");
			Log.Message("  {0}", options.FullOutputDirectory );
			Log.Message("");

																	  
			Log.Message("Temp directory:");
			Log.Message("  {0}", options.FullTempDirectory );

			Log.Message("");
			 
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string GetFullVTOutputPath ( AssetSource assetSource )
		{
			return Path.Combine( Options.FullOutputDirectory, "VTPages_" + assetSource.Hash );
		}



		/// <summary>
		/// Gets storage associated with given asset.
		/// </summary>
		/// <param name="assetSource"></param>
		/// <returns></returns>
		public IStorage GetAssetStorage ( AssetSource assetSource )
		{
			var path = Path.Combine( Options.FullOutputDirectory, assetSource.Hash + ".storage" );
			return new DirectoryStorage(path);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="iniData"></param>
		/// <param name="sectionName"></param>
		/// <returns></returns>
		string[] GetAllKeysFromSection ( IniData iniData, string sectionName )
		{
			if (!iniData.Sections.ContainsSection(sectionName)) {
				return new string[0];
			}

			return iniData.Sections[ sectionName ]
					.Select( key => key.KeyName )
					.ToArray();
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		string ResolveDirectory ( string dir )
		{
			if ( dir.StartsWith("%") && dir.EndsWith("%") ) {
				var envVar = Environment.GetEnvironmentVariable( dir.Substring(1, dir.Length-2) );
				if (envVar==null) {
					Log.Warning("  {0} : environment variable not found", dir);
					return null;
				}
				if (!Directory.Exists( envVar )) {
					Log.Warning("  {0} = {1} : path not found", dir, envVar );
					return null;
				}
				return envVar;
			}

			if ( dir.StartsWith("HKEY_") ) {

				var keyValue	=	dir.Split(new[]{'@'}, 2);
				var key			=	keyValue[0];
				var value		=	keyValue.Length == 2 ? keyValue[1] : "";

				var regValue	=	Registry.GetValue(key, value, null);

				if (regValue==null) {
					Log.Warning("  {0} : registry variable not found", dir);
					return null;
				}
				if (!(regValue is string)) {
					Log.Warning("  {0} : registry variable must be string", dir);
					return null;
				}
				
				if (!Directory.Exists( (string)regValue )) {
					Log.Warning("  {0} = {1} : path not found", dir, (string)regValue );
					return null;
				}

				return (string)regValue;
			}

			if (Path.IsPathRooted( dir )) {
				if (Directory.Exists( dir )) {
					return dir;
				}
			} else {
				var fullDir = Path.GetFullPath( dir );
				if (Directory.Exists( fullDir )) {
					return fullDir;
				}
			}

			Log.Warning("  {0} : not resolved", dir);
			return null;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string ResolveContentPath ( string path )
		{
			string dummy;
			return ResolveContentPath( path, out dummy );
		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string ResolveContentPath ( string path, out string baseDirectory )
		{
			var resolvedPath = ResolvePath( path, ContentDirectories, out baseDirectory );

			if (resolvedPath==null) {
				throw new BuildException(string.Format("Path '{0}' not resolved", path));
			}

			return resolvedPath;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="dirs"></param>
		/// <returns></returns>
		string ResolvePath ( string path, IEnumerable<string> dirs )
		{
			string dummy;
			return ResolvePath( path, dirs, out dummy );
		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="dirs"></param>
		/// <returns></returns>
		string ResolvePath ( string path, IEnumerable<string> dirs, out string baseDirectory )
		{
			baseDirectory	=	null;

			if (path==null) {
				throw new ArgumentNullException("path");
			}

			if ( Path.IsPathRooted( path ) ) {
				if (File.Exists( path )) {
					return path;
				} else {
					return null;
				}
			}

			//
			//	make search list :
			//
			foreach ( var dir in dirs ) {
				//Log.Message("...{0}", dir );
				var fullPath = Path.GetFullPath( Path.Combine( dir, path ) );
				if ( File.Exists( fullPath ) ) {
					baseDirectory = dir;
					return fullPath;
				}
			}

			return null;
		}



		/// <summary>
		/// Try to resolve source file path.
		/// If succeded returns true. False otherwice.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool ContentFileExists ( string path )
		{
			string dummy;
			return ResolvePath( path, ContentDirectories, out dummy ) != null;
		}



		/// <summary>
		/// Generates temporary file name for given key with given extension
		/// and return full path to this file.
		/// </summary>
		/// <param name="key">unique key string value</param>
		/// <param name="ext">Desired extension with leading dot</param>
		/// <returns>Full path for generated file name.</returns>
		public string GetTempFileName ( string key, string ext )
		{
			var fileName	=	ContentUtils.GetHashedFileName( key, ext );

			return Path.Combine( Options.FullTempDirectory, fileName );
		}



		/// <summary>
		/// Generates temporary file name for given key with given extension
		/// and return full path to this file.
		/// </summary>
		/// <param name="key">unique key string value</param>
		/// <param name="ext">Desired extension with leading dot</param>
		/// <returns>Full path for generated file name.</returns>
		public string GetTempFileName ( string key, string subfile, string ext )
		{
			var fileName	=	ContentUtils.GetHashedFileName( key + subfile, ext );

			return Path.Combine( Options.FullTempDirectory, fileName );
		}



		/// <summary>
		/// Copies file to stream.
		/// </summary>
		/// <param name="fullSourceFileName"></param>
		/// <param name="targetStream"></param>
		public void CopyFileTo( string fullSourceFileName, Stream targetStream )
		{
			using ( var source = File.OpenRead( fullSourceFileName ) ) { 
				source.CopyTo( targetStream );
			}
		}



		/// <summary>
		/// Copies file file to stream.
		/// </summary>
		/// <param name="fullSourceFileName">Resolved source file name</param>
		/// <param name="targetStream"></param>
		public void CopyFileTo( string fullSourceFileName, BinaryWriter writer )
		{
			var data = File.ReadAllBytes( fullSourceFileName ); 
			writer.Write( data );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <param name="targetFile"></param>
		/// <returns></returns>
		public bool IsUpToDate ( string sourceFile, string targetFile )
		{
			if (string.IsNullOrEmpty(sourceFile)) {
				throw new ArgumentNullException("sourceFile");
			}

			if (string.IsNullOrEmpty(targetFile)) {
				throw new ArgumentNullException("targetFile");
			}

			if (!Path.IsPathRooted(sourceFile)) {
				throw new ArgumentException("sourceFile", "Path must be rooted");
			}

			if (!Path.IsPathRooted(targetFile)) {
				throw new ArgumentException("sourceFile", "Path must be rooted");
			}


			if (!File.Exists(targetFile)) {
				return false;
			}
			
			var targetTime	=	File.GetLastWriteTime( targetFile );
			var sourceTime	=	File.GetLastWriteTime( sourceFile );

			if (targetTime < sourceTime) {
				return false;
			}

			return true;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="exePath"></param>
		/// <param name="commandLine"></param>
		public void RunTool ( string exePath, string commandLine )
		{
			//commandLine = FixEncoding(commandLine);
			Log.Debug("...exec: {0} {1}", exePath, commandLine );

			ProcessStartInfo psi = new ProcessStartInfo();
			psi.RedirectStandardInput	=	true;
			psi.RedirectStandardOutput	=	true;
			psi.RedirectStandardError	=	true;
			psi.FileName				=	ResolvePath( exePath, binaryPaths );
			psi.Arguments				=	commandLine;
			psi.UseShellExecute			=	false;
			psi.CreateNoWindow			=	true;

			int exitCode = 0;

			string stdout = "";
			string stderr = "";


			using ( Process proc = Process.Start( psi ) ) {
				stdout = proc.StandardOutput.ReadToEnd().Trim(new[]{'\r', '\n'});
				stderr = proc.StandardError.ReadToEnd().Trim(new[]{'\r', '\n'});
				proc.WaitForExit();
				exitCode = proc.ExitCode;
			}

			Log.Verbose( "{0}", stdout ); //*/
				
			if ( exitCode != 0 ) {
				//File.WriteAllText( @"C:\GITHUB\stderr.txt", commandLine);
				throw new ToolException( string.Format("Failed to launch tool:\r\n{0} {1}\r\n{2}", exePath, commandLine, stderr ) );
			} else {
				if (!string.IsNullOrWhiteSpace(stderr)) {				
					Log.Warning( "{0}", stderr.Trim(new[]{'\r', '\n'}) );
				} 
			}
		}
	}
}

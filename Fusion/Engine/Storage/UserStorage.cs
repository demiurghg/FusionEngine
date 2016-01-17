using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Fusion.Drivers.Graphics;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.IniParser;
using Fusion.Engine.Common;
using Fusion.Engine.Server;

namespace Fusion.Engine.Storage {

	/// <summary>
	/// User storage refers to the read-write storage supported by the game platform 
	/// for saving information from the game at runtime. The data can be associated 
	/// with a particular player's profile, or available to all players.
	/// </summary>
	public class UserStorage : DisposableBase {

		DirectoryInfo storageDir;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Game"></param>
		internal UserStorage ( Game Game )
		{
			string myDocs	=	Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string appName	=	Path.GetFileNameWithoutExtension( AppDomain.CurrentDomain.FriendlyName.Replace(".vshost","") );

			storageDir		=	Directory.CreateDirectory( Path.Combine( myDocs, appName ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="relative"></param>
		/// <returns></returns>
		internal string GetFullPath ( string relative )
		{
			return Path.Combine(storageDir.FullName, relative);
		}



		/// <summary>
		/// Creates a new directory in the UserStorage scope.
		/// </summary>
		public void CreateDirectory ( string directory )
		{	
			Directory.CreateDirectory( GetFullPath(directory) );
		}



		/// <summary>
		/// Removes directory and all subdirectories and files in UserStorage scope 
		/// </summary>
		/// <param name="directory"></param>
		public void DeleteDirectory ( string directory )
		{
			Directory.Delete( GetFullPath(directory) );
		}



		/// <summary>
		/// Creates a file with read/write access at a specified path in the UserStorage.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public Stream CreateFile ( string file )
		{
			return File.Create( GetFullPath(file) );
		}



		/// <summary>
		/// Creates a file with read/write access at a specified path in the UserStorage.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public Stream OpenFile ( string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare  )
		{
			return File.Open( GetFullPath(file), fileMode, fileAccess, fileShare );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="fileMode"></param>
		/// <param name="fileAccess"></param>
		/// <returns></returns>
		public Stream OpenFile ( string file, FileMode fileMode, FileAccess fileAccess  )
		{
			return File.Open( GetFullPath(file), fileMode, fileAccess );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="fileMode"></param>
		/// <returns></returns>
		public Stream OpenFile ( string file, FileMode fileMode )
		{
			return File.Open( GetFullPath(file), fileMode );
		}



		/// <summary>
		/// Creates a file with read/write access at a specified path in the UserStorage.
		/// </summary>
		/// <param name="file"></param>
		public void DeleteFile ( string file )
		{	
			File.Delete( GetFullPath(file) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public bool DirectoryExists ( string directory )
		{
			return Directory.Exists( GetFullPath(directory) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public bool FileExists ( string directory )
		{
			return File.Exists( GetFullPath(directory) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="searchPattern"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		public string[] GetFiles ( string directory, string searchPattern, bool recursive )
		{
			return Directory.GetFiles( GetFullPath(directory), searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}
	}
}

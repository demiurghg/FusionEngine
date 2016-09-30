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
	public class DirectoryStorage : DisposableBase, IStorage  {

		DirectoryInfo storageDir;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Game"></param>
		public DirectoryStorage ( string storageDirPath )
		{
			storageDir		=	Directory.CreateDirectory( storageDirPath );
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
		/// Gets full system path to user storage directory.
		/// </summary>
		/// <param name="relative"></param>
		/// <returns></returns>
		public virtual string GetFullPath ( string relative )
		{
			return Path.Combine(storageDir.FullName, relative);
		}



		/// <summary>
		/// Creates a new directory in the UserStorage scope.
		/// </summary>
		public virtual void CreateDirectory ( string directory )
		{	
			Directory.CreateDirectory( GetFullPath(directory) );
		}



		/// <summary>
		/// Removes directory and all subdirectories and files in UserStorage scope 
		/// </summary>
		/// <param name="directory"></param>
		public virtual void DeleteDirectory ( string directory )
		{
			Directory.Delete( GetFullPath(directory) );
		}



		/// <summary>
		/// Creates a file with read/write access at a specified path in the UserStorage.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public virtual Stream CreateFile ( string file )
		{
			return File.Create( GetFullPath(file) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <param name="fileMode"></param>
		/// <returns></returns>
		public virtual Stream OpenFile ( string file, FileMode fileMode, FileAccess fileAccess )
		{
			return File.Open( GetFullPath(file), fileMode, fileAccess );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public virtual Stream OpenRead ( string fileName )
		{
			return OpenFile( fileName, FileMode.Open, FileAccess.Read );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public virtual Stream OpenWrite ( string fileName )
		{
			return OpenFile( fileName, FileMode.Create, FileAccess.Write );
		}



		/// <summary>
		/// Creates a file with read/write access at a specified path in the UserStorage.
		/// </summary>
		/// <param name="file"></param>
		public virtual void DeleteFile ( string file )
		{	
			File.Delete( GetFullPath(file) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public virtual bool DirectoryExists ( string directory )
		{
			return Directory.Exists( GetFullPath(directory) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public virtual bool FileExists ( string directory )
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
		public virtual string[] GetFiles ( string directory, string searchPattern, bool recursive )
		{
			return Directory.GetFiles( GetFullPath(directory), searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}
	}
}

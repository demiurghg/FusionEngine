using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fusion.Engine.Storage {

	/// <summary>
	/// Represents any kind of storage
	/// </summary>
	public interface IStorage {

		/// <summary>
		/// Creates directory.
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		void CreateDirectory ( string directoryName );

		/// <summary>
		/// Deletes directory
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		void DeleteDirectory ( string directoryName );

		/// <summary>
		/// Determines whether the specified directory exists
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		bool DirectoryExists ( string directoryName );

		/// <summary>
		/// Opens file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="fileMode"></param>
		/// <returns></returns>
		Stream OpenFile ( string fileName, FileMode fileMode, FileAccess fileAccess );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		Stream OpenRead ( string fileName );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		Stream OpenWrite ( string fileName );

		/// <summary>
		/// Deletes file
		/// </summary>
		/// <param name="fileName"></param>
		void DeleteFile ( string fileName );

		/// <summary>
		/// Determines whether the specified file exists
		/// </summary>
		/// <param name="fileName"></param>
		bool FileExists ( string fileName );

		/// <summary>
		/// Gets full disc path. This method may not be supported.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		string GetFullPath ( string fileName );


		/// <summary>
		/// Gets files from storage within given directory
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="searchPattern"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		string[] GetFiles ( string directory, string searchPattern, bool recursive );
	}
}

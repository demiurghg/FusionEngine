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
	public class UserStorage : DirectoryStorage {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Game"></param>
		internal UserStorage ( Game Game ) : base( Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Game.GameID ) )
		{
		}
	}
}

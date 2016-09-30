using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Storage;

namespace Fusion.Engine.Mapping {

	/// <summary>
	/// Represents entire map in game, including:
	///		Virtual textures
	///		All models
	/// </summary>
	public class Map {

		/// <summary>
		/// Gets map's storage
		/// </summary>
		internal IStorage MapStorage {
			get; private set;
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Map ( Game game, Stream stream, IStorage storage )
		{
			MapStorage = storage;
		}
	}
}

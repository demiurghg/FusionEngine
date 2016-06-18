using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Core;


namespace Fusion.Engine.Client {
	
	/// <summary>
	/// Performs loading of client-side game content.
	/// The derived class should create task where it loads all content.
	/// When task completed all visual and aural objects should be arranged in FinalizeLoad.
	/// </summary>
	public abstract class GameLoader : DisposableBase {

		/// <summary>
		/// Creates instance of GameLoader.
		/// </summary>
		public GameLoader()
		{
		}

		/// <summary>
		/// Called on each frame until GameLoader returns IsCompleted..
		/// </summary>
		/// <param name="gameTime"></param>
		abstract public void Update ( GameTime gameTime );
	
		/// <summary>
		/// Indicates that loader has completed loading process.
		/// </summary>
		/// <returns></returns>
		abstract public bool IsCompleted {
			get;
		}
	}

}

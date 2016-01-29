using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using Fusion.Engine.Network;
using System.IO;
using Fusion.Engine.Common;
using Fusion.Core;


namespace Fusion.Engine.Client {
	
	/// <summary>
	/// 
	/// </summary>
	public abstract class GameLoader : DisposableBase {

		/// <summary>
		/// 
		/// </summary>
		public GameLoader()
		{
		}



		/// <summary>
		/// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace ShooterDemo {
	class GameWorld {

		/// <summary>
		/// Creates world
		/// </summary>
		/// <param name="map"></param>
		public GameWorld ()
		{
		}



		/// <summary>
		/// Initializes world from map. Server side.
		/// </summary>
		/// <param name="map"></param>
		public void InitializeFromMap ( string map )
		{
		}


		/// <summary>
		/// Initialized world from server info and snapshot. Client side.
		/// </summary>
		/// <param name="serverInfo"></param>
		/// <param name="snapshot"></param>
		public void InitializeFromSnapshot ( string serverInfo, byte[] snapshot )
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public byte[] UpdateServer ( GameTime gameTime )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void UpdateClient ( GameTime gameTime )
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="snapshot"></param>
		public void FeedSnapshot ( byte[] snapshot )
		{
		} 
	}
}

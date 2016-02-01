using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Server;
using Fusion.Engine.Client;

namespace ShooterDemo {
	partial class GameWorld {

		/// <summary>
		/// Gets entities.
		/// </summary>
		public GameEntityCollection Entities {
			get { return entities; }
		}
		GameEntityCollection entities;


		/// <summary>
		/// Creates world
		/// </summary>
		/// <param name="map"></param>
		public GameWorld ()
		{
			entities	=	new GameEntityCollection();
		}



		/// <summary>
		/// Initializes world from map. Server side.
		/// </summary>
		/// <param name="map"></param>
		public void InitializeFromMap ( GameServer server, string map )
		{
			var scene = server.Content.Load<Scene>( map );

			InitStaticPhysWorld( scene );
		}



		/// <summary>
		/// Initialized world from server info and snapshot. Client side.
		/// </summary>
		/// <param name="serverInfo"></param>
		/// <param name="snapshot"></param>
		public void InitializeFromServerInfo ( GameClient client, string serverInfo )
		{
			//	load scene :
			var scene	=	client.Content.Load<Scene>( serverInfo );

			InitStaticModels( client, scene );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public byte[] UpdateServer ( GameTime gameTime )
		{
			var entityArray = new GameEntity[ Entities.Count ];
			Entities.CopyTo( entityArray, 0 );
			
			foreach ( var entity in entityArray ) {
				entity.Update( gameTime );
			}
							
			return new byte[0];
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
		public void FeedSnapshot ( GameClient client, byte[] snapshot, bool initial )
		{
			//if (
		} 



	}
}

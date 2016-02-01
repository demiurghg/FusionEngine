using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Fusion.Engine.Graphics;
using Fusion.Engine.Audio;
using Fusion.Engine.Input;
using System.IO;


namespace ShooterDemo.Entities {
	class Player : GameEntity {


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		public Player ( SpawnParameters parameters )
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public override void Load ( BinaryReader reader )
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public override void Save ( BinaryWriter writer )
		{
			throw new NotImplementedException();
		}

	}
}

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

namespace ShooterDemo {
	public abstract class GameEntity {

		public GameEntity ()
		{
		}


		abstract public void Update ( GameTime gameTime );
		abstract public void Load ( BinaryReader reader );
		abstract public void Save ( BinaryWriter writer );

	}
}

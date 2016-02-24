using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Fusion.Engine.Graphics;


namespace $safeprojectname$ {
	class $safeprojectname$Loader : GameLoader {

		Task loadingTask;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public $safeprojectname$Loader ( $safeprojectname$Client client, string serverInfo )
		{
			loadingTask	=	new Task( ()=>LoadingTask(serverInfo) );

			loadingTask.Start();
		}


		/// <summary>
		/// Called on each frame until IsCompleted returns true.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			//	do nothing.
		}



		/// <summary>
		/// Returns true when loading complets.
		/// </summary>
		public override bool IsCompleted {
			get { 
				return loadingTask.IsCompleted; 
			}
		}


		/// <summary>
		/// 
		/// </summary>
		void LoadingTask ( string serverInfo )
		{
			//	Load something here.
			//	Do not add objects to RenderWorld and SoundWorld here.
			//	Complete this operations in $safeprojectname$Client.FinalizeLoad.
		}
	}
}

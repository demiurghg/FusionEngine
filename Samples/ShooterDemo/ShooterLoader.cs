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


namespace ShooterDemo.Client {
	class ShooterLoader : Fusion.Engine.Client.GameLoader {

		Task loadingTask;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="serverInfo"></param>
		public ShooterLoader ( ShooterClient client, string serverInfo )
		{
			loadingTask	=	new Task( ()=>LoadingTask(client, serverInfo) );
			loadingTask.Start();
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			//	do nothing.
		}



		/// <summary>
		/// 
		/// </summary>
		public override bool IsCompleted {
			get { 
				return loadingTask.IsCompleted; 
			}
		}


		/// <summary>
		/// 
		/// </summary>
		void LoadingTask ( ShooterClient client, string serverInfo )
		{
			string[] messages = new[] {
				"Locating hamsters"								,
				"Greasing wheels"								,
				"Filling foodbowls"								,
				"Spinning up the warp drive"					,
				"Waiter... there's a pixel in my soup"			,
				"Don't touch that cable!"						,
				"What made that noise?"							,
				"Tomorrow's lottery numbers are..."				,
				"Searching for marbles"							,
				"Tightening loose screws"						,
				"Brushing your hair"							,
				"Engaging warp drive"							,
				"Paying your parking tickets"					,
				"Putting the square peg in a round hole"		,
				"Pixelating the pixels"							,
				"Watering the plants"							,
				"Scanning for schadenfreude"					,
				"Polishing the servers"							,
				"Shooing away rain clouds"						,
				"Tuning up air guitars"							,
				"Anti gravity zone; remain seated"				,
				"Waterproofing swimming pools"					,
				"Weighing pound cakes"							,
				"Watering sidewalks"							,
				"Planetary alignment complete"					,
				"Painting the lawn"								,
			};
			

			foreach ( var msg in messages ) {
				Log.Message("...{0}", msg);
				Thread.Sleep(100);
			}
		}
	}
}

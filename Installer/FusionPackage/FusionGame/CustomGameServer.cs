﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Engine.Client;
using Fusion.Engine.Common;
using Fusion.Engine.Server;

namespace $safeprojectname$ {
	class $safeprojectname$GameServer : GameServer {

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
			
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public $safeprojectname$GameServer ( GameEngine gameEngine ) : base(gameEngine)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Starts server with given map/level.
		/// </summary>
		/// <param name="map"></param>
		public override void LoadLevel ( string map )
		{
			var rand = new Random();

			foreach ( var msg in messages ) {
				Log.Message("INIT: {0}...", msg);
				Thread.Sleep( rand.Next(10,10) );
			}


			Log.Message("[SERVER STARTED]");
		}


		/// <summary>
		/// Kills server
		/// </summary>
		public override void UnloadLevel ()
		{
			var rand = new Random();

			Log.Message("SV: [SERVER IS GONNA DIE]");

			foreach ( var msg in messages ) {
				Log.Message("KILL: {0}...", msg);
				Thread.Sleep( rand.Next(10,10) );
			}


			Log.Message("SV: [SERVER WAS KILLED]");
		}


		/// <summary>
		/// Runs one step of server-side world simulation.
		/// Do not close the stream.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime, Stream outputSnapshot )
		{
			//Thread.Sleep(20);

			using (var writer = new BinaryWriter(outputSnapshot, Encoding.UTF8, true)) {

				for (int i=0; i<3000; i++) {
					writer.Write(i);
				}
				/*var str = string.Join( " | ", state.Select(s1=>s1.Value) );

				for (int i = 0; i<2050; i++) {
					writer.Write(string.Format("SV: {1}", gameTime.ElapsedSec, str ));
				} */
			}
		}


		Dictionary<string, string> state = new Dictionary<string,string>();

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public override void FeedCommand ( string clientIP, Stream inputCommand )
		{
			using ( var reader = new BinaryReader(inputCommand) ) {
				var s = reader.ReadString();
				state[clientIP] = s;
				//Log.Message("FeedCommand: {0} -> {1}", clientIP, s );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ServerInfo ()
		{
			return "Alice";
		}


		public override void ClientConnected ( string clientIP, string userInfo )
		{
			NotifyClients("CONNECTED: {0} - {1}", clientIP, userInfo);
			state.Add( clientIP, " --- " );
		}

		public override void ClientDisconnected ( string clientIP, string userInfo )
		{
			NotifyClients("DISCONNECTED: {0} - {1}", clientIP, userInfo );
			state.Remove( clientIP );
		}
	}
}

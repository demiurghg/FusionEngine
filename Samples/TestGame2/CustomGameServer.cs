using System;
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

namespace TestGame2 {
	class CustomGameServer : GameServer {

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
		public CustomGameServer ( Game game ) : base(game)
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
		public override void LoadContent ( string map )
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
		public override void UnloadContent ()
		{
			var rand = new Random();

			Log.Message("SV: [SERVER IS GONNA DIE]");

			foreach ( var msg in messages ) {
				Log.Message("KILL: {0}...", msg);
				Thread.Sleep( rand.Next(10,10) );
			}


			Log.Message("SV: [SERVER WAS KILLED]");
		}


		static Random rand = new Random();

		int[] buffer	=	Enumerable.Range(0,10000).Select( i => rand.Next(0,5000) ).ToArray();

		/// <summary>
		/// Runs one step of server-side world simulation.
		/// Do not close the stream.
		/// </summary>
		/// <param name="gameTime"></param>
		public override byte[] Update ( GameTime gameTime )
		{
			Thread.Sleep(10);

			for (int i=0; i<5000; i++) {
				buffer[ rand.Next(0, buffer.Length-1) ] = rand.Next(0,5000);
			}

			var extrs = string.Join(" ", buffer.Select( i => i.ToString("x5") ) );
			return Encoding.UTF8.GetBytes( "World: [" + string.Join( " | ", state.Select(s1=>s1.Value) ) + "]" + extrs);
		}


		Dictionary<Guid, string> state = new Dictionary<Guid,string>();

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public override void FeedCommand ( Guid clientGuid, byte[] userCommand )
		{
			state[clientGuid] = Encoding.UTF8.GetString( userCommand );
		}


		public override void FeedNotification ( Guid clientGuid, string message )
		{
			Log.Message("NOTIFICATION {0}: {1}", clientGuid, message );
			NotifyClients("{0} says: {1}", clientGuid, message );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ServerInfo ()
		{
			return "Alice";
		}


		public override void ClientConnected ( Guid clientGuid, string userInfo )
		{
			NotifyClients("CONNECTED: {0} - {1}", clientGuid, userInfo);
			Log.Message("CONNECTED: {0} - {1}", clientGuid, userInfo);
			state.Add( clientGuid, " --- " );
		}

		public override void ClientActivated ( Guid guid )
		{
			Log.Message( "ACTIVATED: {0}", guid );
		}


		public override void ClientDeactivated ( Guid guid )
		{
			Log.Message( "DEACTIVATED: {0}", guid );
		}

		public override void ClientDisconnected ( Guid clientGuid )
		{
			NotifyClients("DISCONNECTED: {0}", clientGuid );
			Log.Message("DISCONNECTED: {0}", clientGuid );
			state.Remove( clientGuid );
		}


		public override bool ApproveClient( Guid clientGuid, string userInfo, out string reason )
		{
			Log.Message("APPROVE: {0} {1}", clientGuid, userInfo );
			reason = ".";
			return true;
		}
	}
}

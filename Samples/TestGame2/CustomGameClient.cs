using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;


namespace TestGame2 {
	class CustomGameClient : GameClient {

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameClient ( GameEngine gameEngine )	: base(gameEngine)
		{
		}


		/// <summary>
		/// Initializes game
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Called when connection request accepted by server.
		/// Client could start loading models, textures, models etc.
		/// </summary>
		/// <param name="map"></param>
		public override void LoadLevel ( string serverInfo )
		{
		}

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		///	Reason???
		/// </summary>
		public override void UnloadLevel ()
		{
		}

		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// Do not close the stream.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime, Stream outputCommand )
		{
			var mouse = GameEngine.Mouse;
			
			using ( var writer = new BinaryWriter(outputCommand, Encoding.UTF8, true) ) {
				writer.Write(string.Format("[{0} {1}]", mouse.Position.X, mouse.Position.Y ));
			}
		}


		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshot"></param>
		public override void FeedSnapshot ( Stream inputSnapshot ) 
		{
			var bb = new byte[1500];
			
			/*using ( var reader = new BinaryReader(inputSnapshot, Encoding.UTF8, true) ) {
				reader.Read(bb, 0, 1500);	
			} */


			using ( var reader = new BinaryReader(inputSnapshot, Encoding.UTF8, true) ) {

				int last = -1;
				for (int i=0; i<3000; i++) {
					var curr = reader.ReadInt32();
					if (curr!=last+1) {
						Log.Warning("{0} {1}", curr, last);
						break;
					}
					last = curr;
				}

				/*for (int i=0; i<250; i++) {
					reader.ReadString();
				}

				string s = reader.ReadString();
				Log.Message("SNAPSHOT: {1} : {0}", s, inputSnapshot.Length );*/
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string UserInfo ()
		{
			return "Bob";
		}
	}
}

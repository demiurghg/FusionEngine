using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Lidgren.Network;
using System.Net;

namespace Fusion.Engine.Client {

	abstract class State {

		protected readonly GameClient	gameClient;

		public State ( GameClient gameClient )
		{
			this.gameClient	=	gameClient;
		}

		public abstract void Connect ( string host, int port );
		public abstract void Disconnect ();
		public abstract void Update ( GameTime gameTime );


		public bool DispatchInternalMessages( NetIncomingMessage msg )
		{
			switch (msg.MessageType) {
				case NetIncomingMessageType.VerboseDebugMessage:Log.Verbose	("CL: " + msg.ReadString()); return true;
				case NetIncomingMessageType.DebugMessage:		Log.Debug	("CL: " + msg.ReadString()); return true;
				case NetIncomingMessageType.WarningMessage:		Log.Warning	("CL: " + msg.ReadString()); return true;
				case NetIncomingMessageType.ErrorMessage:		Log.Error	("CL: " + msg.ReadString()); return true;
				default: return false;
			}
		}
	}



	/// <summary>
	/// StansBy state.
	/// </summary>
	class StandBy : State {
		
		public StandBy ( GameClient gameClient ) : base(gameClient) 
		{
		}


		public override void Connect ( string host, int port )
		{
			var ep = new IPEndPoint( IPAddress.Parse(host), port );
			gameClient.State	=	new Connecting( gameClient, ep );
		}


		public override void Disconnect ()
		{
			Log.Warning("Not connected.");
		}


		public override void Update ( GameTime gameTime )
		{
		}
	}



	/// <summary>
	/// Connecting state.
	/// </summary>
	class Connecting : State {

		NetClient client;
		
		public Connecting ( GameClient gameClient, IPEndPoint endpoint  ) : base(gameClient) 
		{
			var netConfig	=	new NetPeerConfiguration( gameClient.GameEngine.GameTitle );
			netConfig.AutoFlushSendQueue	=	true;

			client			=	new NetClient( netConfig );

			client.Start();

			var userInfo	=	UserInfo();
			var hail		=	client.CreateMessage( userInfo );

			serverEP		=	new IPEndPoint( IPAddress.Parse(host), port );

			var conn		=	client.Connect( serverEP, hail );
		}


		public override void Connect ( string host, int port )
		{
			Log.Warning("Connection in progress.");
		}


		public override void Disconnect ()
		{
			Log.Warning("Connection in progress.");
		}


		public override void Update ( GameTime gameTime )
		{
			NetIncomingMessage msg;
			while ((msg = client.ReadMessage()) != null) {
				switch (msg.MessageType)
				{
					case NetIncomingMessageType.VerboseDebugMessage:Log.Verbose	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.DebugMessage:		Log.Debug	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.WarningMessage:		Log.Warning	("CL: " + msg.ReadString()); break;
					case NetIncomingMessageType.ErrorMessage:		Log.Error	("CL: " + msg.ReadString()); break;

					case NetIncomingMessageType.StatusChanged:		
						DispatchStatusChange( msg );
						break;
					
					case NetIncomingMessageType.ConnectionLatencyUpdated:
						Log.Message("CL: Connection latencty - {0}", msg.ReadSingle() );
						break;

					case NetIncomingMessageType.Data:
						DispatchDataIM( msg );
						break;
					
					default:
						Log.Warning("CL: Unhandled type: " + msg.MessageType);
						break;
				}
				client.Recycle(msg);
			}			
		}
	}


	/// <summary>
	/// Loading state.
	/// </summary>
	class Loading : State {
		
		public Loading ( GameClient gameClient ) : base(gameClient) 
		{
		}


		public override void Connect ( string host, int port )
		{
			throw new NotImplementedException();
		}


		public override void Disconnect ()
		{
			throw new NotImplementedException();
		}


		public override void DispatchIM ( NetIncomingMessage msg )
		{
			throw new NotImplementedException();
		}


		public override void Update ( GameTime gameTime )
		{
			throw new NotImplementedException();
		}
	}


	/// <summary>
	/// Awaiting state.
	/// </summary>
	class Awaiting : State {
		
		public Awaiting ( GameClient gameClient ) : base(gameClient) 
		{
		}


		public override void Connect ( string host, int port )
		{
			throw new NotImplementedException();
		}


		public override void Disconnect ()
		{
			throw new NotImplementedException();
		}


		public override void DispatchIM ( NetIncomingMessage msg )
		{
			throw new NotImplementedException();
		}


		public override void Update ( GameTime gameTime )
		{
			throw new NotImplementedException();
		}
	}


	/// <summary>
	/// Awaiting state.
	/// </summary>
	class Active : State {
		
		public Active ( GameClient gameClient ) : base(gameClient) 
		{
		}


		public override ClientState CurrentState {
			get { return ClientState.Loading; }
		}


		public override void Connect ( string host, int port )
		{
			throw new NotImplementedException();
		}


		public override void Disconnect ()
		{
			throw new NotImplementedException();
		}


		public override void DispatchIM ( NetIncomingMessage msg )
		{
			throw new NotImplementedException();
		}


		public override void Update ( GameTime gameTime )
		{
			throw new NotImplementedException();
		}
	}
}

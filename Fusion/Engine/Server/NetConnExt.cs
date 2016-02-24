using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Fusion.Engine.Server {

	public static class NetConnExt {

		class ClientState {

			public readonly Guid ClientGuid;
			public readonly string UserInfo;
			public bool RequestSnapshot;
			public uint SnapshotID;
			public uint CommandID;
			public uint CommandCounter;

			public ClientState ( Guid clientGuid, string userInfo ) 
			{
				ClientGuid		=	clientGuid;
				UserInfo		=	userInfo;
				SnapshotID		=	0;
				CommandID		=	0;
				CommandCounter	=	0;
			}
		}



		/// <summary>
		/// Reads client GUID from connection hail-message.
		/// </summary>
		/// <param name="conn"></param>
		/// <returns></returns>
		public static Guid GetHailGuid ( this NetConnection conn )
		{
			return new Guid( conn.RemoteHailMessage.PeekBytes(16) );
		}



		/// <summary>
		/// Reads user info from connection hail-message.
		/// </summary>
		/// <param name="conn"></param>
		/// <returns></returns>
		public static string GetHailUserInfo ( this NetConnection conn )
		{
			var bytes = conn.RemoteHailMessage.PeekDataBuffer();
			return Encoding.UTF8.GetString( bytes, 16, bytes.Length-16);
		}



		/// <summary>
		/// Initializes client state for given connection.
		/// </summary>
		/// <param name="conn"></param>
		public static void InitClientState ( this NetConnection conn )
		{
			conn.Tag	=	new ClientState( GetHailGuid(conn), GetHailUserInfo(conn) );
		}



		static ClientState GetState ( this NetConnection conn )
		{
			return conn.Tag as ClientState;
		}



		public static bool IsSnapshotRequested ( this NetConnection conn )
		{
			return (conn.GetState()!=null) && (conn.GetState().RequestSnapshot);
		}


		public static uint GetRequestedSnapshotID ( this NetConnection conn )
		{
			return conn.GetState().SnapshotID;
		}


		public static uint GetLastCommandID ( this NetConnection conn )
		{
			return conn.GetState().CommandID;
		}


		public static void SetRequestSnapshot ( this NetConnection conn, uint snapshotID, uint commandID )
		{
			var state = conn.GetState();
			state.RequestSnapshot	=	true;
			state.SnapshotID		=	snapshotID;
			state.CommandID			=	commandID;
			state.CommandCounter++;
		}


		public static void ResetRequestSnapshot ( this NetConnection conn )
		{
			conn.GetState().RequestSnapshot	=	false;
		}
		


		public static uint GetCommandCount ( this NetConnection conn )
		{
			return conn.GetState().CommandCounter;
		}
	}
}

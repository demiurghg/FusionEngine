using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Core;



namespace Fusion.Engine.Network {

	/// <summary>
	/// 
	/// Net Channel reponces for:
	/// 
	///		1. Reliability
	///		2. Fragmentation
	///		3. Encryption
	///		
	/// </summary>
	internal partial class NetChan : DisposableBase {

		Random rand = new Random();

		readonly GameEngine GameEngine;

		readonly Socket Socket;

		readonly string Name;

		object lockObject = new object();

		Dictionary<IPEndPoint, State> channels = new Dictionary<IPEndPoint,State>();


		/// <summary>
		/// Port
		/// </summary>
		public readonly ushort QPort;

		/// <summary>
		/// Maximal transmition unit.
		/// </summary>
		public const int MTU	=	1400;



		/// <summary>
		/// Creates instance of NetChan with given name and bound to particular socket.
		/// </summary>
		/// <param name="socket">Socket which NetChan bound to.</param>
		/// <param name="name">NetChan name</param>
		public NetChan ( GameEngine engine, Socket socket, string name )
		{
			Name		=	name;
			GameEngine	=	engine;
			Socket		=	socket;
			QPort		=	(ushort)(socket.LocalEndPoint as IPEndPoint).Port;
			Log.Message("Netchan {0} : {1}", name, QPort );
		}



		/// <summary>
		/// Disposes stuff
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
			}
			base.Dispose( disposing );
		}


		/*-----------------------------------------------------------------------------------------
		 *	Channel stuff :
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Adds channel to given endpoint, 
		/// so NetChan can process in-band messages from given endpuint.
		/// 
		/// 
		/// </summary>
		/// <param name="endPoint"></param>
		public void Add ( IPEndPoint endPoint )
		{
			lock (lockObject) {
				channels.Add( endPoint, new State(this, QPort, Socket, endPoint) );
				Log.Message("Netchan {0} : new channel to {1}", Name, endPoint );
			}
		}



		/// <summary>
		/// Removes channel to given ednpoint endpoint.
		/// </summary>
		/// <param name="endPoint"></param>
		public void Remove ( IPEndPoint endPoint )
		{
			lock (lockObject) {
				if (channels.ContainsKey(endPoint)) {
					channels.Remove( endPoint );
					Log.Message("Netchan {0} : channel to {1} removed.", Name, endPoint );
				} else {
					Log.Warning("Netchan {0} : no channel to {1}.", Name, endPoint );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		State GetChannel ( IPEndPoint endPoint )
		{
			lock (lockObject) {
				State state;
				if (!channels.TryGetValue(endPoint, out state)) {
					throw new NetChanException(string.Format("{0} : no channel to {1}.", Name, endPoint));
				}
				return state;				
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public void Clear ()
		{
			channels.Clear();
		}



		/*-----------------------------------------------------------------------------------------
		 *	Out-of-band stuff
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Prints packet info.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="dataSize"></param>
		void ShowPacket ( string action, int dataSize, byte[] data, int seq, int ack )
		{
			if (GameEngine.Network.Config.ShowPackets) {
				var dataStr = string.Join( " ", data.Skip(16).Take(16).Select( b=> b.ToString("X2") ) );
				Log.Message("  {0} {1} [{2,5}] {4} {5}", Name, action, dataSize, dataStr, seq, ack );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="target"></param>
		/// <param name="data"></param>
		public void OutOfBand ( IPEndPoint remoteEP, NetCommand cmd, byte[] data )
		{		
			lock (lockObject) {
				
				var length	=	data.Length;

				if (length>MTU) {	
					throw new NetChanException(string.Format("Message length > {0}", MTU));
				}

				var header	=	new NetChanHeader( QPort, cmd );
				var buffer	=	new byte[ length + NetChanHeader.SizeInBytes ];

				using ( var stream = new MemoryStream(buffer) ) {
					using ( var writer = new BinaryWriter(stream) ) {
						header.Write( writer );
						writer.Write( data );
					}
				}

				Socket.SendTo( buffer, remoteEP );

				ShowPacket("send", buffer.Length, buffer,-1,-1 );
			}
		}



		/// <summary>
		/// Sends OOB string.
		/// </summary>
		/// <param name="remoteEP"></param>
		/// <param name="text"></param>
		public void OutOfBand ( IPEndPoint remoteEP, NetCommand cmd, string text )
		{		
			OutOfBand( remoteEP, cmd, Encoding.ASCII.GetBytes(text) );
		}



		/// <summary>
		/// Sends OOB string.
		/// </summary>
		/// <param name="remoteEP"></param>
		/// <param name="text"></param>
		public void OutOfBand ( IPEndPoint remoteEP, NetCommand cmd )
		{		
			OutOfBand( remoteEP, cmd, new byte[0] );
		}



		/*-----------------------------------------------------------------------------------------
		 *	In-band stuff :
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void Transmit ( IPEndPoint remoteEP, NetCommand command, byte[] data, int length )
		{
			lock (lockObject) {
				var state	=	GetChannel( remoteEP );
				state.Transmit( command, data, length );
			}
		}



		/// <summary>
		/// Dispatches incoming NetChan packets.
		/// Call this method until it return False.
		/// </summary>
		/// <param name="message">Return null if bad packet was received.</param>
		/// <returns>True if packet (even bad one) received. Otherwice returns False.</returns>
		public bool Dispatch ( out NetMessage message )
		{
			lock (lockObject) {

				message	= null;

				var buffer = new byte[MTU+100];

				EndPoint	remoteEP = new IPEndPoint( IPAddress.Any, 0 );

				if (Socket.Available<=0) {
					return false;
				}

				try {
					int size = Socket.ReceiveFrom( buffer, ref remoteEP );

					if (rand.NextFloat(0,1)<GameEngine.Network.Config.SimulatePacketsLoss) {
						message = null;
						return true;
					}

					if (size<NetChanHeader.SizeInBytes) {
						Log.Warning("Bad packet from {0}: size < NetChan header size", remoteEP);
						return true;
					}


					message	=	new NetMessage( (IPEndPoint)remoteEP, buffer, size );

					ShowPacket("recv", size, buffer, (int)message.Header.Sequence, (int)message.Header.AckSequence); 


					//
					//	out of band - do nothing:
					//
					if (message.Header.IsOutOfBand) {
						return true;
					} else {
						var state	=	GetChannel( (IPEndPoint)remoteEP );
						message		=	state.Dispatch( message );
						return true;
					}


				} catch ( SocketException se ) {

					if (se.SocketErrorCode==SocketError.WouldBlock) {
						//	that's OK - no incoming messages, return false.
						return false;
					}

					throw new NetChanException(string.Format("{0}", se.SocketErrorCode));
				}
			}
		}
	}
}

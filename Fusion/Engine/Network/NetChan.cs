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
	/// 
	/// Net Channel header structure :
	/// 
	///		[RELIABLE (1)] [SEQUENCE NUMBER (31)          ]	- 0xFFFFFFFF means out of band message
	///		[ACKNOLEDGMENT                                ]
	///		[QPORT (16)  ] [FRAGMENT COUNT|TOTAL FRAGMENTS]	- QPort is Quake legacy. Fragment ID???
	///		[PAYLOAD......................................]
	///		
	/// </summary>
	public class NetChan : DisposableBase {

		readonly GameEngine GameEngine;

		readonly Socket Socket;

		readonly string Name;

		object lockObject = new object();

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



		void ShowPacket ( string action, int dataSize )
		{
			if (GameEngine.Network.Config.ShowPackets) {
				Log.Message("  {0} {1} [{2,5}]", Name, action, dataSize );
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
				var header		=	new NetChanHeader( QPort, cmd );
				var datagram	=	header.MakeDatagram(data);

				Socket.SendTo( datagram, remoteEP );

				ShowPacket("send", datagram.Length );
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



		uint sequenceCounter = 0;
		uint receivedSequence = 0;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void Transmit ( IPEndPoint remoteEP, NetCommand cmd, byte[] data )
		{
			lock (lockObject) {

				var header		=	new NetChanHeader( QPort, cmd, sequenceCounter++, 0, false );
				var datagram	=	header.MakeDatagram( data );

				Socket.SendTo( datagram, remoteEP );

				ShowPacket("send", datagram.Length );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void Transmit ( IPEndPoint remoteEP, NetCommand cmd, Stream stream )
		{
			var count	=	(int)stream.Length;
			var data	=	new byte[ count ];
			stream.Position = 0;
			stream.Read( data, 0, count );

			Transmit( remoteEP, cmd,  data );
		}



		Random rand = new Random();


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

				var buffer = new byte[MTU];

				EndPoint	remoteEP = new IPEndPoint( IPAddress.Any, 0 );

				if (Socket.Available<=0) {
					return false;
				}

				try {
					int size = Socket.ReceiveFrom( buffer, ref remoteEP );

					ShowPacket("recv", size); 

					//	simulate packet loss :
					if (rand.NextFloat(0,1) < GameEngine.Network.Config.SimulatePacketsLoss) {
						message = null;
						return true;
					}

					if (size<NetChanHeader.SizeInBytes) {
						throw new NetChanException("Bad packet: size < NetChan header size");
					}

					var header = NetChanHeader.ReadFrom( buffer );

					//
					//	out of band - do nothing:
					//
					if (header.IsOutOfBand) {
						message = new NetMessage( header, (IPEndPoint)remoteEP, buffer, size );
						return true;
					}


					//
					//	non reliable message
					//
					if (receivedSequence>=header.Sequence) {	
						message = null;
						return true;
					}
					if (header.Sequence - receivedSequence > 1) {
						Log.Warning("Lost packet: {0} - {1}", receivedSequence, header.Sequence );
					}
					receivedSequence = header.Sequence;

					message	=	new NetMessage( header, (IPEndPoint)remoteEP, buffer, size );
					return true;

					//
					//	reliable message
					//

				} catch ( NetChanException ne ) {
					Log.Warning( "NetChan.Dispatch() : {0}", ne.Message );

					message = null;
					return true;

				} catch ( SocketException se ) {
					if (se.SocketErrorCode==SocketError.WouldBlock) {
						//	that's OK - no incoming messages, return false.
						return false;
					}
					Log.Warning( "NetChan.Dispatch() : {0}", se.ToString() );
					return false;
				}


				Log.Warning("NetChan.Dispatch() : Something is wrong. This code should not be reached.");
				return false;
			}
		}



		/// <summary>
		/// Waits for message which meet given criteria.
		/// If there are no datagrams, NetChan will sleep for given time.
		/// If attemptCount is hit - function will return null;
		/// </summary>
		/// <param name="criteria">Criteria function</param>
		/// <param name="sleepTime">Sleep time between attemptrs (10 msec, is ok)</param>
		/// <param name="attemptCount">Attempts count</param>
		/// <returns></returns>
		public NetMessage Wait ( Func<NetMessage,bool> criteria, int sleepTime = 10, int attemptCount = 100 )
		{
			NetMessage message;

			for (int i=0; i<attemptCount; i++) {

				while ( Dispatch( out message ) ) {
					
					if (criteria(message)) {
						return message;
					}
				}

				Thread.Sleep(sleepTime);
			}

			return null;
		}
	}
}

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

	internal partial class NetChan : DisposableBase {

		class State {

			readonly public NetChan NetChan;
			readonly public IPEndPoint EndPoint;
			readonly public Socket Socket;
			readonly public ushort QPort;

			uint outgoingSequence = 1;
			uint incomingSequence = 0;

			List<NetMessage>	fragments	=	null;


			/// <summary>
			/// 
			/// </summary>
			/// <param name="ep"></param>
			public State ( NetChan netChan, ushort qport, Socket socket, IPEndPoint ep )
			{
				this.NetChan	=	netChan;
				this.QPort		=	qport;
				this.EndPoint	=	ep;
				this.Socket		=	socket;
			}



			/// <summary>
			/// 
			/// </summary>
			/// <param name="command"></param>
			/// <param name="data"></param>
			/// <param name="length"></param>
			public void Transmit ( NetCommand command, byte[] data, int length )
			{
				if (length>MTU) {	
					
					int fragCount		=	MathUtil.IntDivRoundUp( length, MTU );
					int restOfBuffer	=	length;

					if (fragCount>255) {	
						throw new NetChanException( string.Format("Message length {0} > max length {1}", length, MTU * 255) );
					}

					for (int i=0; i<fragCount; i++) {
						Transmit( command, data, MTU * i, Math.Min(MTU, restOfBuffer), (byte)fragCount, (byte)i );
						restOfBuffer -= MTU;
					}

				} else {

					Transmit( command, data, 0, length, 1, 0 );
				}
			}

								   

			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			void Transmit ( NetCommand command, byte[] data, int offset, int length, byte fragCount, byte fragId )
			{
				var header = new NetChanHeader( QPort, command, outgoingSequence++, incomingSequence, false );
				header.FragmentCount	=	fragCount;
				header.FragmentID		=	fragId;

				if (fragId>=fragCount) {
					throw new ArgumentException("fragId >= fragCount");
				}

				var buffer = new byte[ length + NetChanHeader.SizeInBytes ];

				using ( var stream = new MemoryStream(buffer) ) {
					using ( var writer = new BinaryWriter(stream) ) {
						header.Write( writer );
						writer.Write( data, offset, length );
					}
				}

				Socket.SendTo( buffer, EndPoint );

				NetChan.ShowPacket("send", buffer.Length, buffer, (int)header.Sequence, (int)header.AckSequence );
			}



			/// <summary>
			/// Dispatches messages.
			/// Returns dispatched message or NULL.
			/// </summary>
			/// <param name="message"></param>
			/// <returns></returns>
			public NetMessage Dispatch ( NetMessage message )
			{
				var sequence	=	message.Header.Sequence;
				var ackSequence	=	message.Header.AckSequence;

				//	discard duplicate or stale packet:
				if ( sequence <= incomingSequence ) {

					Log.Warning("{0}: out of order pakcet from: {1} at {2}", message.SenderEP, sequence, incomingSequence );
					return null;
				}


				//	lost packet:
				uint dropped	=	sequence - (incomingSequence+1);

				if ( dropped>0 ) {
					Log.Warning("{0}: dropped {1} packets at {2}", message.SenderEP, dropped, incomingSequence );
				}

				incomingSequence	=	sequence;

				
				if (message.IsFragmented) {
					return DispatchFragmented( message, dropped > 0 );
				} else {
					return message;
				}
			}



			/// <summary>
			/// 
			/// </summary>
			/// <param name="message"></param>
			NetMessage DispatchFragmented ( NetMessage message, bool drop )
			{
				///Log.Message("FRAG: {0} {1}", message.Header.FragmentID, message.Header.FragmentCount );
				
				//	fragment packet was dropped.
				//	stop receiving framents.
				if ( message.Header.FragmentID > 0 && drop ) {
					Log.Warning("Entire fragment discarded.");
					fragments = null;
					return null;
				}

				//	got fragment -> add if receiving:
				if ( message.Header.FragmentID>0) {
					if (fragments!=null) {
						fragments.Add( message );

						//	we'd got all fragments -> compose single message:
						if ( fragments.Count==message.Header.FragmentCount ) {
							var newMessage = NetMessage.Compose( fragments );
							fragments = null;
							return newMessage;
						} else {
							return null;
						}

					} else {
						//	perevious fragments are lost.
						//	do nothing
						return null;
					}
				}

				//	got fragment with id = 0, start fragment receiving:
				if ( message.Header.FragmentID==0) {
					fragments = new List<NetMessage>();
					fragments.Add( message );
					return null;
				}

				//	something bad...
				Log.Warning("Something bad with fragmented message");
				fragments = null;

				return null;
			}
		}
	}
}

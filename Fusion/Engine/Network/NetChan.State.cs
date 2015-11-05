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

			uint sentSequence = 1;
			uint recvSequence = 0;


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
			/// <param name="msg"></param>
			public void Transmit ( NetCommand command, byte[] data, int length )
			{
				if (length>MTU) {	
					throw new NetChanException(string.Format("Message length > {0}", MTU));
				}

				var header = new NetChanHeader( QPort, command, sentSequence++, 0, false );

				var buffer = new byte[ length + NetChanHeader.SizeInBytes ];

				using ( var stream = new MemoryStream(buffer) ) {
					using ( var writer = new BinaryWriter(stream) ) {
						header.Write( writer );
						writer.Write( data, 0, length );
					}
				}

				Socket.SendTo( buffer, EndPoint );


				NetChan.ShowPacket("send", buffer.Length, buffer );
			}



			/// <summary>
			/// 
			/// </summary>
			/// <param name="msg"></param>
			public void Dispatch ( ref NetMessage message )
			{
				if (recvSequence>=message.Header.Sequence) {
					Log.Warning("Packet drop: {0} {1}", recvSequence, message.Header.Sequence );
					message = null;
					return;
				}

				recvSequence	=	message.Header.Sequence;
			}
		}
		
	}
}

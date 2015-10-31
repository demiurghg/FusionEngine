using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;


namespace Fusion.Engine.Network {

	[StructLayout(LayoutKind.Explicit, Size=12)]
	public struct NetChanHeader {

		public static readonly int SizeInBytes	=	Marshal.SizeOf(typeof(NetChanHeader));
		public const uint ReliabilityBit		=	0x80000000;
		public const uint OutOfBand				=	0xFFFFFFFF;

		/// <summary>
		/// Reliability bit and Sequence number.
		/// 0xFFFFFFFF means out of band message.
		/// </summary>
		[FieldOffset(0)]
		public uint	Sequence;

		/// <summary>
		/// Acknoledgement sequence.
		/// </summary>
		[FieldOffset(4)]
		public uint	AckSequence;

		/// <summary>
		/// QPort
		/// </summary>
		[FieldOffset(8)]
		public ushort QPort;

		/// <summary>
		/// Fragment number
		/// </summary>
		[FieldOffset(10)]
		public byte Fragment;

		/// <summary>
		/// Fragment count
		/// </summary>
		[FieldOffset(11)]
		public byte FragmentCount;


		/// <summary>
		/// Indicates, is this header is out of band message.
		/// </summary>
		public bool IsOutOfBand {
			get {
				return (Sequence == OutOfBand);
			}
		}



		/// <summary>
		/// Creates in-band non-fragmented header.
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="ack"></param>
		/// <param name="qport"></param>
		public NetChanHeader( bool reliable, uint sequence, uint ack, ushort qport )
		{
			Sequence		=	sequence | ( reliable ? ReliabilityBit : 0 );
			AckSequence		=	ack;
			QPort			=	qport;
			Fragment		=	0;
			FragmentCount	=	1;
		}


		/// <summary>
		/// Creates out-of-band header.
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="ack"></param>
		/// <param name="qport"></param>
		public NetChanHeader( ushort qport )
		{
			Sequence		=	OutOfBand;
			AckSequence		=	OutOfBand;
			QPort			=	qport;
			Fragment		=	0;
			FragmentCount	=	1;
		}



		/// <summary>
		/// Writes header to byte array.
		/// </summary>
		/// <param name="destination"></param>
		public byte[] MakeDatagram ( byte[] dataToSend )
		{
			var message = new byte[ SizeInBytes + dataToSend.Length ];

			var intPtr = Marshal.AllocHGlobal( SizeInBytes );

			Marshal.StructureToPtr( this, intPtr, false );

			Marshal.Copy(  intPtr, message, 0, SizeInBytes );

			Marshal.FreeHGlobal(intPtr);

			Buffer.BlockCopy( dataToSend, 0, message, SizeInBytes, dataToSend.Length );

			return message;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public static NetChanHeader ReadFrom ( byte[] receivedData )
		{
			var intPtr = Marshal.AllocHGlobal( SizeInBytes );

			Marshal.Copy( receivedData, 0, intPtr, SizeInBytes );

			var header = (NetChanHeader)Marshal.PtrToStructure( intPtr, typeof(NetChanHeader) );

			Marshal.FreeHGlobal(intPtr);

			return header;
		}
	}
}

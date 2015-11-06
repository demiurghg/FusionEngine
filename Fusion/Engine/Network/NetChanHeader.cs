using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;


namespace Fusion.Engine.Network {

	public class NetChanHeader {

		public const int SizeInBytes		=	16;
		public const uint ReliabilityBit	=	0x80000000;
		public const uint OutOfBand			=	0xFFFFFFFF;

		/// <summary>
		/// Reliability bit and Sequence number.
		/// 0xFFFFFFFF means out of band message.
		/// </summary>
		public uint	Sequence;

		/// <summary>
		/// Acknoledgement sequence.
		/// </summary>
		public uint	AckSequence;

		/// <summary>
		/// Network protocol command.
		/// </summary>
		public NetCommand	Command;

		/// <summary>
		/// QPort
		/// </summary>
		public ushort QPort;

		/// <summary>
		/// Total number of fragments.
		/// </summary>
		public byte FragmentCount;

		/// <summary>
		/// Fragment ID.
		/// </summary>
		public byte FragmentID;

		/// <summary>
		/// Indicates, is this header is out of band message.
		/// </summary>
		public bool IsOutOfBand {
			get {
				return (Sequence == OutOfBand);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="receivedDat"></param>
		public NetChanHeader ()
		{
			Sequence		=	0;
			AckSequence		=	0;
			QPort			=	0;
			Command			=	NetCommand.None;
		}



		/// <summary>
		/// Creates in-band non-fragmented header.
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="ack"></param>
		/// <param name="qport"></param>
		public NetChanHeader( ushort qport, NetCommand cmd, uint sequence, uint ack, bool reliable )
		{
			Sequence		=	sequence | ( reliable ? ReliabilityBit : 0 );
			AckSequence		=	ack;
			QPort			=	qport;
			Command			=	cmd;
			FragmentID		=	0;
			FragmentCount	=	1;
		}


		/// <summary>
		/// Creates out-of-band header.
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="ack"></param>
		/// <param name="qport"></param>
		public NetChanHeader( ushort qport, NetCommand cmd )
		{
			Sequence		=	OutOfBand;
			AckSequence		=	OutOfBand;
			QPort			=	qport;
			Command			=	cmd;
			FragmentID		=	0;
			FragmentCount	=	1;
		}




		public void Write ( BinaryWriter writer )
		{
			writer.Write( Sequence		);
			writer.Write( AckSequence	);
			writer.Write( (uint)Command	);
			writer.Write( QPort			);
			writer.Write( FragmentCount	);
			writer.Write( FragmentID	);
		}



		public void Read ( BinaryReader reader )
		{
			Sequence		=	reader.ReadUInt32();
			AckSequence		=	reader.ReadUInt32();
			Command			=	(NetCommand)reader.ReadUInt32();
			QPort			=	reader.ReadUInt16();
			FragmentCount	=	reader.ReadByte();
			FragmentID		=	reader.ReadByte();
		}


		/*
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
		/// Writes header to byte array.
		/// </summary>
		/// <param name="destination"></param>
		public byte[] MakeDatagram<T> ( T[] dataToSend ) where T: struct
		{
			var stride	=	Marshal.SizeOf( typeof(T) );
			var count	=	dataToSend.Length;

			var message = new byte[ SizeInBytes + stride * count ];

			var intPtr = Marshal.AllocHGlobal( message.Length );
			var freePtr = intPtr;

			Marshal.StructureToPtr( this, intPtr, false );

			intPtr	=	IntPtr.Add( intPtr, SizeInBytes );

			for ( int i=0; i<count; i++) {
				Marshal.StructureToPtr( dataToSend[i], intPtr, false );
				intPtr	=	IntPtr.Add( intPtr, stride );
			}

			Marshal.Copy( freePtr, message, 0, message.Length );

			Marshal.FreeHGlobal(freePtr);

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
		}	  */
	}
}

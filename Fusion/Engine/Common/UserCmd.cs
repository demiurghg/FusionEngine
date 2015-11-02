using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fusion.Engine.Common {

	/// <summary>
	/// Command is structure which is sent from client to server.
	/// </summary>
	public struct UserCmd {

		/// <summary>
		/// Command sequence.
		/// </summary>
		public int Sequence;

		/// <summary>
		/// Command opcode. Flags allowed.
		/// </summary>
		public int Command;

		/// <summary>
		/// Spatial user orientation (yaw).
		/// </summary>
		public short Yaw;

		/// <summary>
		/// Spatial user orientation (pitch).
		/// </summary>
		public short Pitch;

		/// <summary>
		/// Spatial user orientation (roll).
		/// </summary>
		public short Roll;

		/// <summary>
		/// Two-dimensional X
		/// For clicks.
		/// </summary>
		public short X;

		/// <summary>
		/// Two-dimensional Y
		/// For clicks.
		/// </summary>
		public short Y;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="command"></param>
		public UserCmd ( int sequence, int command )
		{
			Sequence	=	sequence;
			Command		=	command;
			Yaw			=	0;
			Pitch		=	0;
			Roll		=	0;
			X			=	0;
			Y			=	0;
		}


		/// <summary>
		/// Writes UserCmd
		/// </summary>
		/// <param name="writer"></param>
		public void Write ( BinaryWriter writer )
		{
			writer.Write( Sequence	);
			writer.Write( Command	);
			writer.Write( Yaw		);
			writer.Write( Pitch		);
			writer.Write( Roll		);
			writer.Write( X			);
			writer.Write( Y			);
		}


		/// <summary>
		/// Reads UserCmd
		/// </summary>
		/// <param name="writer"></param>
		public static UserCmd Read ( BinaryReader reader )
		{
			var cmd = new UserCmd();

			cmd.Sequence	=	reader.ReadInt32();	
			cmd.Command		=	reader.ReadInt32();	
			cmd.Yaw			=	reader.ReadInt16();	
			cmd.Pitch		=	reader.ReadInt16();	
			cmd.Roll		=	reader.ReadInt16();	
			cmd.X			=	reader.ReadInt16();	
			cmd.Y			=	reader.ReadInt16();	

			return cmd;
		}
	}
}

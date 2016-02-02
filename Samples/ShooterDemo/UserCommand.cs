using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace ShooterDemo {

	[Flags]
	public enum UserCtrlFlags : int {
		None		=	0x0000,
		Forward		=	0x0001,
		Backward	=	0x0002,
		StrafeLeft	=	0x0004,
		StrafeRight	=	0x0008,
		Crouch		=	0x0010,
		Jump		=	0x0020,
	}

	
	/// <summary>
	/// Represents an instant user action and intention.
	/// </summary>
	public struct UserCommand {

		/// <summary>
		/// Current user yaw.
		/// </summary>
		public float Yaw;

		/// <summary>
		/// Current user pitch.
		/// </summary>
		public float Pitch;

		/// <summary>
		/// Current user roll.
		/// </summary>
		public float Roll;

		/// <summary>
		/// Set of user control flags.
		/// </summary>
		public UserCtrlFlags CtrlFlags;

		
		/// <summary>
		/// Gets user command's bytes.
		/// </summary>
		/// <param name="userCmd"></param>
		/// <returns></returns>
		static public byte[] GetBytes(UserCommand userCmd) 
		{
			int size = Marshal.SizeOf(userCmd);
			byte[] array = new byte[size];

			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(userCmd, ptr, true);
			Marshal.Copy(ptr, array, 0, size);
			Marshal.FreeHGlobal(ptr);
			return array;
		}


		/// <summary>
		/// Gets user command from bytes
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		static public UserCommand FromBytes(byte[] array) 
		{
			var userCmd = new UserCommand();

			int size = Marshal.SizeOf(userCmd);
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.Copy(array, 0, ptr, size);

			userCmd = (UserCommand)Marshal.PtrToStructure(ptr, userCmd.GetType());
			Marshal.FreeHGlobal(ptr);

			return userCmd;
		}


		public override string ToString ()
		{
			return string.Format("Angles:[{0} {1} {2}] Ctrl:[{3}]", Yaw, Pitch, Roll, CtrlFlags );
		}
	}
}

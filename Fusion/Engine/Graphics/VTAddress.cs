using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents the address of virtual texture tile.
	/// Address consist of page X index, page Y index & mip level.
	/// Non-zero dummy used for GPU readback. Zero dummy is ignored.
	/// </summary>
	public struct VTAddress : IEquatable<VTAddress> {
		
		public Int16 PageX;
		public Int16 PageY;
		public Int16 MipLevel;
		public Int16 Dummy;


		public static VTAddress CreateBadAddress (int uniqueNumber) {
			var a = new VTAddress();
			a.PageX		= (short)( -( uniqueNumber ) );
			a.PageY		= (short)( -( uniqueNumber >> 16 ) );
			a.MipLevel	= -1;
			a.Dummy		= -1;
			return a;
		}


		public VTAddress ( short pageX, short pageY, short mipLevel )
		{
			if (mipLevel<0 | mipLevel>=VTConfig.MipCount) {
				throw new ArgumentOutOfRangeException("mipLevel");
			}

			int maxPageCount = VTConfig.VirtualPageCount >> mipLevel;

			if (pageX<0 | pageX>=maxPageCount) {
				throw new ArgumentOutOfRangeException("pageX");
			}
			if (pageY<0 | pageY>=maxPageCount) {
				throw new ArgumentOutOfRangeException("pageY");
			}

			PageX		=	pageX;
			PageY		=	pageY;
			MipLevel	=	mipLevel;
			Dummy		=	1;
		}



		public static VTAddress FromChild ( VTAddress feedback )
		{
			if (feedback.MipLevel >= VTConfig.MaxMipLevel) {
				throw new ArgumentException("mip >= max mip");
			}

			return new VTAddress() { 
				PageX		= (short)( feedback.PageX/2 ),
				PageY		= (short)( feedback.PageY/2 ),
				MipLevel	= (short)( feedback.MipLevel + 1 ),
				Dummy		= (short)( feedback.Dummy   )
			};
		}



		public VTAddress GetLessDetailedMip ()
		{
			return FromChild(this);
		}



		public override string ToString ()
		{
			return string.Format("{0},{1}:{2}", PageX, PageY, MipLevel );
		}


        private bool Equals(ref VTAddress other)
        {
            return	( other.PageX == PageX	) &&
					( other.PageY == PageY	) &&
					( other.MipLevel	  == MipLevel	);
        }


        public bool Equals(VTAddress other)
        {
            return Equals(ref other);
        }


        public override bool Equals(object value)
        {
            if (!(value is VTAddress))
                return false;

            var strongValue = (VTAddress)value;
            return Equals(ref strongValue);
        }


		public override int GetHashCode ()
		{
            unchecked
            {
                var hashCode = PageX.GetHashCode();
                hashCode = (hashCode * 397) ^ PageY.GetHashCode();
                hashCode = (hashCode * 397) ^ MipLevel.GetHashCode();
                return hashCode;
            }
		}



		public UInt32 ComputeUIntAddress ()
		{
			return (uint)ComputeIntAddress( PageX, PageY, MipLevel );
		}



		public static Int32 ComputeIntAddress ( int pageX, int pageY, int mipLevel )
		{
			if (pageX>=VTConfig.TextureSize) {
				throw new ArgumentException("pageX");
			}
			if (pageY>=VTConfig.TextureSize) {
				throw new ArgumentException("pageY");
			}
			if (mipLevel>VTConfig.MaxMipLevel) {
				throw new ArgumentException("mipLevel");
			}

			pageX		= pageX & (VTConfig.TextureSize-1);
			pageY		= pageY & (VTConfig.TextureSize-1);
			mipLevel	= mipLevel & 0x7;

			return (Int32)((mipLevel << 24) | (pageY << 12) | pageX);
		}



		public string GetFileNameWithoutExtension ()
		{
			return ComputeUIntAddress().ToString("X8");
		}
	}

}

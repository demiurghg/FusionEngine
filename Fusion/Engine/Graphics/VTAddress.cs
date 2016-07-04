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


		public override string ToString ()
		{
			return string.Format("{0} {1} {2}", PageX, PageY, MipLevel );
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
			if (PageX>=VTConfig.TextureSize) {
				throw new InvalidOperationException("pageX");
			}
			if (PageY>=VTConfig.TextureSize) {
				throw new InvalidOperationException("pageY");
			}
			if (MipLevel>VTConfig.MaxMipLevel) {
				throw new InvalidOperationException("mipLevel");
			}


			var pageX		= PageX & (VTConfig.TextureSize-1);
			var pageY		= PageY & (VTConfig.TextureSize-1);
			var mipLevel	= MipLevel & 0x7;

			return (UInt32)((mipLevel << 20) | (pageY << 10) | pageX);
		}



		public string GetFileNameWithoutExtension ()
		{
			return ComputeUIntAddress().ToString("X8");
		}
	}

}

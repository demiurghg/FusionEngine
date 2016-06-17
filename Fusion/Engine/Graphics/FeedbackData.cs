using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics {

	struct FeedbackData : IEquatable<FeedbackData> {
		public Int16 PageX;
		public Int16 PageY;
		public Int16 Mip;
		public Int16 Dummy;


		public static FeedbackData FromChild ( FeedbackData feedback )
		{
			if (feedback.Mip >= VTConfig.MaxMipLevel) {
				throw new ArgumentException("mip >= max mip");
			}

			return new FeedbackData() { 
				PageX = (short)( feedback.PageX/2 ),
				PageY = (short)( feedback.PageY/2 ),
				Mip	  = (short)( feedback.Mip + 1 ),
				Dummy = (short)( feedback.Dummy   )
			};
		}


		public override string ToString ()
		{
			return string.Format("{0} {1} {2}", PageX, PageY, Mip );
		}

        /// <summary>
        /// Determines whether the specified <see cref="Vector4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        private bool Equals(ref FeedbackData other)
        {
            return	( other.PageX == PageX	) &&
					( other.PageY == PageY	) &&
					( other.Mip	  == Mip	);
        }


        public bool Equals(FeedbackData other)
        {
            return Equals(ref other);
        }


        public override bool Equals(object value)
        {
            if (!(value is FeedbackData))
                return false;

            var strongValue = (FeedbackData)value;
            return Equals(ref strongValue);
        }


		public override int GetHashCode ()
		{
            unchecked
            {
                var hashCode = PageX.GetHashCode();
                hashCode = (hashCode * 397) ^ PageY.GetHashCode();
                hashCode = (hashCode * 397) ^ Mip.GetHashCode();
                hashCode = (hashCode * 397) ^ Dummy.GetHashCode();
                return hashCode;
            }
		}
	}

}

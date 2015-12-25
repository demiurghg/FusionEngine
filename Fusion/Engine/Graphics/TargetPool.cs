using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	public class TargetPool {

		int Encode ( int width, int height, bool hdr, bool uav, bool mips )
		{
			return (width & 0x3FFF) | ((height & 0x3FFFF)<<14) | (hdr?1 : 0);
		}
		

		public RenderTarget2D AllocRT ( int width, int height, bool hdr, bool uav, bool mips )
		{
			throw new NotImplementedException();
		}
		
		
		public DepthStencil2D AllocDS ( int width, int height )
		{
			throw new NotImplementedException();
		}
		
		
		public void Free ( RenderTarget2D target )
		{
			throw new NotImplementedException();
		}


		public void Free ( DepthStencil2D target )
		{
			throw new NotImplementedException();
		}

	}
}

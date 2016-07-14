using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Drivers.Graphics;
using Fusion.Core.Content;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Creates texture from file on disk or in memory.
	/// </summary>
	public class UserTexture : Texture {

		Texture2D texture;


		/// <summary>
		/// Creates texture from stream.
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="texture"></param>
		public UserTexture ( RenderSystem rs, Stream stream, bool forceSRgb  )
		{
			this.texture	=	new Texture2D( rs.Device, stream, forceSRgb );
			this.Width		=	texture.Width;
			this.Height		=	texture.Height;
			this.Srv		=	texture;
		}


		/// <summary>
		/// Creates texture from file in memory 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="texture"></param>
		public UserTexture ( RenderSystem rs, byte[] data, bool forceSRgb )
		{
			this.texture	=	new Texture2D( rs.Device, data, forceSRgb );
			this.Width		=	texture.Width;
			this.Height		=	texture.Height;
			this.Srv		=	texture;
		}


		public UserTexture(Texture2D tex)
		{
			this.texture	= tex;
			this.Width		= texture.Width;
			this.Height		= texture.Height;
			this.Srv		= texture;
		}



		/// <summary>
		///	Disposes DiscTexture 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref texture );
			}

			base.Dispose( disposing );
		}
	}
}

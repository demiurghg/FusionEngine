using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Drivers.Graphics;
using Fusion.Core.Content;
using Fusion.Engine.Common;
using Fusion.Engine.Imaging;

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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="texture"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		UserTexture ( RenderSystem rs, Texture2D texture, int width, int height )
		{
			this.texture	=	texture;
			this.Width		=	width;
			this.Height		=	height;
			this.Srv		=	texture;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="stream"></param>
		/// <param name="forceSRgb"></param>
		/// <returns></returns>
		static public UserTexture CreateFromTga ( RenderSystem rs, Stream stream, bool forceSRgb )
		{
			Image image =	Image.LoadTga( stream );

			var texture	=	new Texture2D( rs.Device, image.Width, image.Height, ColorFormat.Rgba8, false, forceSRgb );

			texture.SetData( image.RawImageData );

			return new UserTexture( rs, texture, image.Width, image.Height );
		}



		public void UpdateFromTga  ( Stream stream )
		{
			Image image =	Image.LoadTga( stream );

			if ( image.Width!=Width || image.Height != Height ) {
				texture.SetData( image.RawImageData );
			}
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

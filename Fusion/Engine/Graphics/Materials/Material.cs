using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Fusion.Core.IniParser;
using System.IO;
using Fusion.Core;
using Fusion.Core.IniParser.Model;
using Fusion.Core.IniParser.Model.Formatting;
using Fusion.Core.Content;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	
	/// <summary>
	/// Reprsents material.
	/// </summary>
	public partial class Material : DisposableBase {

		const int MaxTextures = 16;

		ShaderResource[]	shaderResources;
		ConstantBuffer		constBuffer;

		/// <summary>
		/// Gets material's constant buffer :
		/// </summary>
		internal ConstantBuffer ConstantBuffer {
			get {
				return constBuffer;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="maxTextures"></param>
		internal Material ( RenderSystem rs, MaterialData parameters, IEnumerable<Texture> textures )
		{
			if (rs==null) {
				throw new ArgumentNullException("rs");
			}

			if (textures.Count()<0 || textures.Count()>MaxTextures) {
				throw new ArgumentException("textureCount", "Must be less or equal to " + MaxTextures.ToString() );
			}

			shaderResources	=	textures.Select( tex => tex.Srv ).ToArray();

			constBuffer		=	new ConstantBuffer( rs.Device, typeof(MaterialData) );
			constBuffer.SetData( parameters );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref constBuffer );
			}
			base.Dispose( disposing );
		}

		

		/// <summary>
		/// Sets materials textures
		/// </summary>
		/// <param name="device"></param>
		internal void SetTextures ( GraphicsDevice device )
		{
			for (int i=0; i<shaderResources.Length; i++) {
				device.PixelShaderResources[i] = shaderResources[i];
			}
		}
	}
}
																	    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using SharpDX.Direct3D;
using Native.Dds;
using Native.Wic;
using Fusion.Core;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics.Resources {
	class TextureCubeArray : ShaderResource {

		D3D.Texture2D	texCubeArray;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="size"></param>
		/// <param name="count"></param>
		/// <param name="format"></param>
		/// <param name="mips"></param>
		public TextureCubeArray ( GraphicsDevice device, int size, int count, ColorFormat format, bool mips ) : base(device)
		{
			if (count>2048/6) {
				throw new GraphicsException("Too much elements in texture array");
			}

			this.Width		=	size;
			this.Depth		=	1;
			this.Height		=	size;
			//this.Format		=	format;
			//this.mipCount	=	mips ? ShaderResource.CalculateMipLevels(Width,Height) : 1;

			var texDesc = new Texture2DDescription();

			texDesc.ArraySize		=	6 * count;
			texDesc.BindFlags		=	BindFlags.ShaderResource;
			texDesc.CpuAccessFlags	=	CpuAccessFlags.None;
			texDesc.Format			=	MakeTypeless( Converter.Convert( format ) );
			texDesc.Height			=	Height;
			texDesc.MipLevels		=	0;
			texDesc.OptionFlags		=	ResourceOptionFlags.TextureCube;
			texDesc.SampleDescription.Count	=	1;
			texDesc.SampleDescription.Quality	=	0;
			texDesc.Usage			=	ResourceUsage.Default;
			texDesc.Width			=	Width;


			texCubeArray	=	new D3D.Texture2D( device.Device, texDesc );
			SRV				=	new ShaderResourceView( device.Device, texCubeArray );
		}


		public void UpdateFromRenderTargetCube ( RenderTargetCube rtCube )
		{
			
		}

	}
}

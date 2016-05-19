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
using Fusion.Core;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {
	public class VolumeRWTexture : ShaderResource {

		D3D.Texture3D	tex3D;
		ColorFormat		format;
		int				mipCount;


		UnorderedAccessView	uav;

		internal UnorderedAccessView UAV {
			get { return uav; }
		}


		/// <summary>
		/// Creates texture
		/// </summary>
		/// <param name="device"></param>
		public VolumeRWTexture ( GraphicsDevice device, int width, int height, int depth, ColorFormat format, bool mips ) : base( device )
		{
			this.Width		=	width;
			this.Height		=	height;
			this.Depth		=	depth;
			this.format		=	format;
			this.mipCount	=	mips ? ShaderResource.CalculateMipLevels(Width,Height,Depth) : 1;

			var texDesc = new Texture3DDescription();
			texDesc.BindFlags		=	BindFlags.ShaderResource | BindFlags.UnorderedAccess;
			texDesc.CpuAccessFlags	=	CpuAccessFlags.None;
			texDesc.Format			=	Converter.Convert( format );
			texDesc.Height			=	Height;
			texDesc.MipLevels		=	mipCount;
			texDesc.OptionFlags		=	ResourceOptionFlags.None;
			texDesc.Usage			=	ResourceUsage.Default;
			texDesc.Width			=	Width;
			texDesc.Depth			=	Depth;

			var uavDesc = new UnorderedAccessViewDescription();
			uavDesc.Format		=	Converter.Convert( format );
			uavDesc.Dimension	=	UnorderedAccessViewDimension.Texture3D;
			uavDesc.Texture3D.FirstWSlice	=	0;
			uavDesc.Texture3D.MipSlice		=	0;
			uavDesc.Texture3D.WSize			=	depth;

			tex3D	=	new D3D.Texture3D( device.Device, texDesc );
			SRV		=	new D3D.ShaderResourceView( device.Device, tex3D );
			uav		=	new UnorderedAccessView( device.Device, tex3D, uavDesc );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref tex3D );
				SafeDispose( ref uav );
			}

			base.Dispose( disposing );
		}



	}
}

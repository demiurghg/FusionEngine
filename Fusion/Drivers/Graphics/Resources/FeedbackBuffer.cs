#define DIRECTX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	internal class FeedbackBuffer : ShaderResource {


		/// <summary>
		/// Render target format
		/// </summary>
		public ColorFormat	Format { get; private set; }


		D3D.Texture2D			tex2D;
		D3D.Texture2D			tex2Dstaging;
		RenderTargetSurface		surface;
			


		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public FeedbackBuffer ( GraphicsDevice device, int width, int height ) : base ( device )
		{
			Log.Debug("FeedbackBuffer: w:{0} h:{1}", width, height );

			Width		=	width;
			Height		=	height;
			Depth		=	1;

			//
			//	Create Texture2D :
			//
			var	texDesc	=	new Texture2DDescription();
				texDesc.Width				=	width;
				texDesc.Height				=	height;
				texDesc.ArraySize			=	1;
				texDesc.BindFlags			=	BindFlags.RenderTarget | BindFlags.ShaderResource;
				texDesc.CpuAccessFlags		=	CpuAccessFlags.None;
				texDesc.Format				=	DXGI.Format.R16G16B16A16_Typeless;
				texDesc.MipLevels			=	1;
				texDesc.OptionFlags			=	ResourceOptionFlags.None;
				texDesc.SampleDescription	=	new DXGI.SampleDescription(1, 0);
				texDesc.Usage				=	ResourceUsage.Default;

			tex2D	=	new D3D.Texture2D( device.Device, texDesc );

			//
			//	Create staging Texture2D :
			//
			var	texDescStaging	=	new Texture2DDescription();
				texDescStaging.Width				=	width;
				texDescStaging.Height				=	height;
				texDescStaging.ArraySize			=	1;
				texDescStaging.BindFlags			=	BindFlags.None;
				texDescStaging.CpuAccessFlags		=	CpuAccessFlags.Read;
				texDescStaging.Format				=	DXGI.Format.R16G16B16A16_Typeless;
				texDescStaging.MipLevels			=	1;
				texDescStaging.OptionFlags			=	ResourceOptionFlags.None;
				texDescStaging.SampleDescription	=	new DXGI.SampleDescription(1, 0);
				texDescStaging.Usage				=	ResourceUsage.Staging;

			tex2Dstaging	=	new D3D.Texture2D( device.Device, texDescStaging );

			//
			//	Create SRV :
			//
			var srvDesc	=	new ShaderResourceViewDescription();
				srvDesc.Texture2D.MipLevels			=	1;
				srvDesc.Texture2D.MostDetailedMip	=	0;
				srvDesc.Dimension					=	ShaderResourceViewDimension.Texture2D;
				srvDesc.Format						=	DXGI.Format.R16G16B16A16_UNorm;

			SRV		=	new ShaderResourceView( device.Device, tex2D, srvDesc );

			//
			//	Create RTV :
			//

			width	=	Width	;
			height	=	Height	;

			var rtvDesc = new RenderTargetViewDescription();
				rtvDesc.Texture2D.MipSlice	=	0;
				rtvDesc.Dimension			=	RenderTargetViewDimension.Texture2D;
				rtvDesc.Format				=	DXGI.Format.R16G16B16A16_UInt;

			var rtv	=	new RenderTargetView( device.Device, tex2D, rtvDesc );

			surface	=	new RenderTargetSurface( rtv, null, tex2D, 0, ColorFormat.Unknown, width, height, 1 );
		}






		/// <summary>
		/// Gets top mipmap level's surface.
		/// Equivalent for GetSurface(0)
		/// </summary>
		public RenderTargetSurface Surface {
			get {
				return surface;
			}
		}



		/// <summary>
		/// Sets viewport for given render target
		/// </summary>
		public void SetViewport ()
		{
			device.DeviceContext.Rasterizer.SetViewport( 0,0, Width, Height, 0, 1 );
		}



		/// <summary>
		/// Disposes
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				Log.Debug("RenderTarget2D: disposing");

				SafeDispose( ref surface );
				SafeDispose( ref SRV );
				SafeDispose( ref tex2D );
				SafeDispose( ref tex2Dstaging );
			}

			base.Dispose(disposing);
		}



		/// <summary>
		/// Gets a copy of 2D texture data, specifying a mipmap level, source rectangle, start index, and number of elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="level"></param>
		/// <param name="rect"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		#warning use 3 sequential textures to reduce waiting for gpu 
		/// https://msdn.microsoft.com/en-us/library/windows/desktop/bb205132(v=vs.85).aspx#Performance_Considerations
		/// 
		public void GetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null || data.Length == 0) {
                throw new ArgumentException("data cannot be null");
			}
            
			if (data.Length < startIndex + elementCount) {
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
			}

			var d3dContext = device.DeviceContext;

            var stagingTex = tex2Dstaging;
                
			lock (d3dContext) {

				//
                // Copy the data from the GPU to the staging texture.
				//
                int elementsInRow;
                int rows;
                    
				elementsInRow = Width;
                rows = Height;

                d3dContext.CopySubresourceRegion( tex2D, level, null, stagingTex, 0, 0, 0, 0);


                // Copy the data to the array :
                DataStream stream;
                var databox = d3dContext.MapSubresource(stagingTex, 0, D3D.MapMode.Read, D3D.MapFlags.None, out stream);

                // Some drivers may add pitch to rows.
                // We need to copy each row separatly and skip trailing zeros.
                var currentIndex	=	startIndex;
                var elementSize		=	Marshal.SizeOf(typeof(T));
                    
				for (var row = 0; row < rows; row++) {

                    stream.ReadRange(data, currentIndex, elementsInRow);
                    stream.Seek(databox.RowPitch - (elementSize * elementsInRow), SeekOrigin.Current);
                    currentIndex += elementsInRow;

                }

				d3dContext.UnmapSubresource( stagingTex, 0 );

                stream.Dispose();
            }
        }



		/// <summary>
		/// Gets a copy of 2D texture data, specifying a start index and number of elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
		{
			this.GetData(0, data, startIndex, elementCount);
		}
		


		/// <summary>
		/// Gets a copy of 2D texture data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void GetData<T> (T[] data) where T : struct
		{
			this.GetData(0, data, 0, data.Length);
		}
	}
}

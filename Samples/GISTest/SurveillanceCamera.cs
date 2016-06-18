using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;

namespace GISTest
{
	public class SurveillanceCamera
	{
		public DynamicTexture CameraTexture;
		public string Url;

		MJPEGStream stream;
		RenderSystem renderSystem;

		public void Start(RenderSystem rs, string url)
		{
			Stop();

			Url = url;
			renderSystem = rs;

			stream = new MJPEGStream(url);

			stream.NewFrame += (sender, args) =>
			{
				Bitmap bitmap = args.Frame;

				if (CameraTexture == null) {
					CameraTexture = new DynamicTexture(Game.Instance.RenderSystem, bitmap.Width, bitmap.Height, typeof(Fusion.Core.Mathematics.ColorBGRA), false, false);
				}

				var imageData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				byte[] bytes = new byte[imageData.Height * imageData.Width * 4];
				
				System.Runtime.InteropServices.Marshal.Copy(imageData.Scan0, bytes, 0, bytes.Length);

				CameraTexture.SetData(bytes);

				bitmap.UnlockBits(imageData);
				//bitmap.Dispose();
			};

			stream.VideoSourceError += (sender, args) => {
				Log.Warning("Error occured while trying to connect to SurveillanceCamera: " + Url);
			};

			stream.Start();
		}


		public void Stop()
		{
			if( stream != null && stream.IsRunning ) stream.Stop();

			if (CameraTexture != null) {
				CameraTexture.Dispose();
				CameraTexture = null;
			}
		}
	}
}

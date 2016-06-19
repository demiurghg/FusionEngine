using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Imaging;
using System.Threading;

namespace Fusion.Engine.Graphics {
	internal class VTTileLoader {

		readonly string baseDirectory;
		readonly VirtualTexture vt;

		ConcurrentQueue<VTAddress>	requestQueue;
		ConcurrentQueue<VTTile>		loadedTiles;

		Task	loaderTask;
		CancellationTokenSource	cancelToken;
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseDirectory"></param>
		public VTTileLoader ( VirtualTexture vt, string baseDirectory )
		{
			this.vt				=	vt;
			this.baseDirectory	=	baseDirectory;
			requestQueue		=	new ConcurrentQueue<VTAddress>();
			loadedTiles			=	new ConcurrentQueue<VTTile>();

			cancelToken		=	new CancellationTokenSource();
			loaderTask		=	new Task( LoaderTask, cancelToken.Token );
			loaderTask.Start();
		}


		/// <summary>
		/// Request texture loading
		/// </summary>
		/// <param name="address"></param>
		public void RequestTile ( VTAddress address )
		{
			requestQueue.Enqueue( address );
		}



		/// <summary>
		/// Gets loaded tile or zero
		/// </summary>
		/// <returns></returns>
		public bool TryGetTile ( out VTTile image )
		{
			return loadedTiles.TryDequeue( out image );
		}



		object lockObj = new object();



		/// <summary>
		/// 
		/// </summary>
		public void Stop()
		{
			lock (lockObj) {
				if (cancelToken!=null) {
					cancelToken.Cancel();
				}

				if (loaderTask!=null) {
					loaderTask.Wait();
				}
			}

		}



		void LoaderTask ()
		{
			while (!cancelToken.IsCancellationRequested) {
				
				VTAddress address;
				
				if (!requestQueue.TryDequeue(out address)) {
					Thread.Sleep(1);
					continue;
				}
					
				var fileName = Path.Combine( baseDirectory, address.GetFileNameWithoutExtension() + ".tga" );

				Log.Message("...vt tile load : {0}", fileName );

				try {
					
					var tile = new VTTile( address, File.OpenRead( fileName ) );
					loadedTiles.Enqueue( tile );

				} catch ( IOException ioex ) {
					Log.Warning("{0}", ioex );
				}

			}
		}

	}
}

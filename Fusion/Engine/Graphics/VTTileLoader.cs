#define USE_PRIORITY_QUEUE
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

	/// <summary>
	/// 
	/// </summary>
	internal class VTTileLoader {

		readonly string baseDirectory;
		readonly VirtualTexture vt;

		#if USE_PRIORITY_QUEUE
		ConcurrentPriorityQueue<int,VTAddress>	requestQueue;
		#else
		ConcurrentQueue<VTAddress>	requestQueue;
		#endif
		
		ConcurrentQueue<VTTile>		loadedTiles;

		Task	loaderTask;
		CancellationTokenSource	cancelToken;
		

		object syncLock = new object();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseDirectory"></param>
		public VTTileLoader ( VirtualTexture vt, string baseDirectory )
		{
			this.vt				=	vt;
			this.baseDirectory	=	baseDirectory;

			#if USE_PRIORITY_QUEUE
				requestQueue	=	new ConcurrentPriorityQueue<int,VTAddress>();
			#else
				requestQueue	=	new ConcurrentQueue<VTAddress>();
			#endif

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
			#if USE_PRIORITY_QUEUE
				requestQueue.Enqueue( address.MipLevel, address );
			#else
				requestQueue.Enqueue( address );
			#endif
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



		/// <summary>
		/// Functionas running in separate thread
		/// </summary>
		void LoaderTask ()
		{
			while (!cancelToken.IsCancellationRequested) {
				
				VTAddress address;

			#if USE_PRIORITY_QUEUE
				address = default(VTAddress);
				KeyValuePair<int,VTAddress> result;
				if (!requestQueue.TryDequeue(out result)) {
					//Thread.Sleep(1);
					continue;
				} else {
					address = result.Value;
				}
			#else
				if (!requestQueue.TryDequeue(out address)) {
					//Thread.Sleep(1);
					continue;
				}
			#endif

					
				var fileName = Path.Combine( baseDirectory, address.GetFileNameWithoutExtension() + ".tga" );

				//Log.Message("...vt tile load : {0}", fileName );

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

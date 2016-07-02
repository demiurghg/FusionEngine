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
using Fusion.Core.Extensions;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	internal class VirtualTexture : GameComponent {

		readonly RenderSystem rs;

		[Config]
		public int MaxPPF { get; set; }


		[Config]
		public bool ShowPageCaching { get; set; }

		[Config]
		public bool ShowPageLoads { get; set; }

		[Config]
		public bool ShowPhysicalTextures { get; set; }


		public UserTexture	FallbackTexture;

		public Texture2D	PhysicalPages;
		public Texture2D	PageTable;

		VTTileLoader	tileLoader;
		VTTileCache		tileCache;




		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="baseDirectory"></param>
		public VirtualTexture ( RenderSystem rs ) : base( rs.Game )
		{
			this.rs	=	rs;

			MaxPPF	=	16;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			int physSize	=	VTConfig.PhysicalPageCount * VTConfig.PageSize;
			int tableSize	=	VTConfig.PageCount;
			PhysicalPages	=	new Texture2D( rs.Device, physSize, physSize, ColorFormat.Rgba8_sRGB, false, true );
			PageTable		=	new Texture2D( rs.Device, tableSize, tableSize, ColorFormat.Bgra8, false, false );

			var rand = new Random();
			PageTable.SetData( Enumerable.Range(0,tableSize*tableSize).Select( i => rand.NextColor() ).ToArray() );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				if (tileLoader!=null) {
					tileLoader.Stop();
				}

				SafeDispose( ref PhysicalPages );
				SafeDispose( ref PageTable );

			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseDir"></param>
		public void Start ( string baseDir )
		{
			var fallbackPath	=	Path.Combine( baseDir, "fallback.tga" );
			FallbackTexture		=	UserTexture.CreateFromTga( rs, File.OpenRead(fallbackPath), true );	

			tileLoader			=	new VTTileLoader( this, baseDir );
			tileCache			=	new VTTileCache( VTConfig.PhysicalPageCount );
		}



		/// <summary>
		/// 
		/// </summary>
		public void Stop ()
		{
			tileLoader.Stop();
		}



		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void Update ( VTAddress[] data )
		{
			var feedback = data.Distinct().Where( p => p.Dummy!=0 ).ToArray();

			List<VTAddress> feedbackTree = new List<VTAddress>();

			//	
			//	build tree :
			//
			foreach ( var addr in feedback ) {

				var paddr = addr;

				feedbackTree.Add( paddr );

				while (paddr.MipLevel < VTConfig.MaxMipLevel) {
					paddr = VTAddress.FromChild( paddr );
					feedbackTree.Add( paddr );
				}

			}

			//
			//	distinct :
			//	
			feedbackTree = feedbackTree
			//	.Where( p0 => cache.Contains(p0) )
				.Distinct()
				.OrderBy( p1 => p1.MipLevel )
				.ToList();//*/

			//
			//	put into cache :
			//
			foreach ( var addr in feedbackTree ) {
				
				int physAddr;

				if ( tileCache.Add( addr, out physAddr ) ) {

					Log.Message("...vt tile cache: {0} --> {1}", addr, physAddr );

					tileLoader.RequestTile( addr );
				}
			}

			//
			//	update table :
			//
			if (tileLoader!=null) {
				for (int i=0; i<MaxPPF; i++) {
				
					VTTile tile;

					if (tileLoader.TryGetTile( out tile )) {

						Rectangle rect;

						if (tileCache.TranslateAddress( tile.Address, out rect )) {
							PhysicalPages.SetData( 0, rect, tile.Data, 0, tile.Data.Length );
						}

					}

				}
			}


			//
			//	patch physical texture with loaded tiles :
			//
		}
	}
}

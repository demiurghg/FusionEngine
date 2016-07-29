﻿using System;
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

		[Config]
		public bool ShowPageTexture { get; set; }

		[Config]
		public bool ShowTileBorder { get; set; }


		public UserTexture	FallbackTexture;

		public Texture2D	PhysicalPages;
		public Texture2D	PageTable;

		VTTileLoader	tileLoader;
		VTTileCache		tileCache;
		VTStamper		tileStamper;




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
			int physSize	=	VTConfig.PhysicalTextureSize;
			int tableSize	=	VTConfig.VirtualPageCount;
			PhysicalPages	=	new Texture2D( rs.Device, physSize, physSize, ColorFormat.Rgba8_sRGB, false, true );
			PageTable		=	new Texture2D( rs.Device, tableSize, tableSize, ColorFormat.Rgba32F, VTConfig.MipCount, false );

			var rand = new Random();
			PageTable.SetData( Enumerable.Range(0,tableSize*tableSize).Select( i => rand.NextColor4() ).ToArray() );
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
			tileStamper			=	new VTStamper();
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
			//	Build tree :
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
			//	Distinct :
			//	
			feedbackTree = feedbackTree
			//	.Where( p0 => cache.Contains(p0) )
				.Distinct()
				.OrderBy( p1 => p1.MipLevel )
				.ToList();//*/


			//
			//	Detect thrashing and prevention
			//	Get highest mip, remove them, repeat until no thrashing occur.
			//
			while (feedbackTree.Count >= VTConfig.TotalPhysicalPageCount ) {
				int minMip = feedbackTree.Min( va => va.MipLevel );
				feedbackTree.RemoveAll( va => va.MipLevel == minMip );
			}



			if (tileCache!=null) {
				tileCache.UpdateCache();
			}

			//
			//	Put into cache :
			//
			if (tileCache!=null) {
				foreach ( var addr in feedbackTree ) {
				
					int physAddr;

					if ( tileCache.Add( addr, out physAddr ) ) {

						//Log.Message("...vt tile cache: {0} --> {1}", addr, physAddr );

						tileLoader.RequestTile( addr );
					}
				}
			}

			//
			//	update table :
			//
			if (tileLoader!=null && tileCache!=null) {

				for (int i=0; i<MaxPPF; i++) {
				
					VTTile tile;

					if (tileLoader.TryGetTile( out tile )) {

						Rectangle rect;

						tile.DrawBorder( ShowTileBorder );

						if (tileCache.TranslateAddress( tile.VirtualAddress, tile, out rect )) {
							//PhysicalPages.SetData( 0, rect, tile.Data, 0, tile.Data.Length );
							tileStamper.Add( tile, rect );
						}

					}

				}


				//	emboss tiles to physical texture
				tileStamper.Update( PhysicalPages );


				//	update page table :
				PageTable.SetData( 0, tileCache.GetPageTableData(0) );
				PageTable.SetData( 1, tileCache.GetPageTableData(1) );
				PageTable.SetData( 2, tileCache.GetPageTableData(2) );
				PageTable.SetData( 3, tileCache.GetPageTableData(3) );
				PageTable.SetData( 4, tileCache.GetPageTableData(4) );
				PageTable.SetData( 5, tileCache.GetPageTableData(5) );
			}
		}
	}
}
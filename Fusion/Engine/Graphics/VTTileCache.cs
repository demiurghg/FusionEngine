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
using Fusion.Core.Shell;
using System.Runtime.InteropServices;
using Fusion.Build.Mapping;

namespace Fusion.Engine.Graphics {

	[StructLayout(LayoutKind.Explicit, Size=20)]
	public struct PageGpu {

		public PageGpu ( float vx, float vy, float offsetX, float offsetY, float mip )
		{
			this.VX			= vx;
			this.VY			= vy;
			this.OffsetX	= offsetX;
			this.OffsetY	= offsetY;
			this.Mip		= mip;
		}

		[FieldOffset( 0)] public float VX;
		[FieldOffset( 4)] public float VY;
		[FieldOffset( 8)] public float OffsetX;
		[FieldOffset(12)] public float OffsetY;
		[FieldOffset(16)] public float Mip;
	}



	public class VTTileCache {

		class Page {
			
			public Page ( VTAddress va, int pa, int physPageCount )
			{
				this.VA			=	va;
				this.Address	=	pa;

				var physTexSize	=	(float)VTConfig.PhysicalTextureSize;
				var border		=	VTConfig.PageBorderWidth;
				var pageSize	=	VTConfig.PageSizeBordered;

				this.X			=	((pa % physPageCount) * pageSize + border ) / physTexSize;
				this.Y			=	((pa / physPageCount) * pageSize + border ) / physTexSize;
			}
			
			public readonly VTAddress VA;
			public readonly int Address;
			public readonly float X;
			public readonly float Y;

			public VTTile Tile = null;

			public override string ToString ()
			{
				return string.Format("{0} {1} {2}", Address, X, Y );
			}
		}


		readonly int pageCount;
		readonly int capacity;
		LRUCache<VTAddress,Page> cache;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="size">Physical page count</param>
		public VTTileCache ( int physPageCount )
		{
			this.pageCount	=	physPageCount;
			this.capacity	=	physPageCount * physPageCount;

			cache	=	new LRUCache<VTAddress,Page>( capacity );

			//	fill cache with dummy pages :
			for (int i=0; i<capacity; i++) {
				var va		= VTAddress.CreateBadAddress(i);
				var page	= new Page( va, i, pageCount );
				cache.Add( va, page );
			}
		}



		/// <summary>
		/// Clears cache
		/// </summary>
		public void Purge ()
		{
			cache.Clear();
		}



		/// <summary>
		/// Translates virtual texture address to physical rectangle in physical texture.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="rectangle"></param>
		/// <returns>False if address is not presented in cache</returns>
		public bool TranslateAddress ( VTAddress address, VTTile tile, out Rectangle rectangle )
		{
			Page page;

			if (cache.TryGetValue(address, out page)) {

				var pa		=	page.Address;
				var ppc		=	VTConfig.PhysicalPageCount;
				var size	=	VTConfig.PageSizeBordered;
				int x		=	(pa % ppc) * size;
				int y		=	(pa / ppc) * size;
				int w		=	size;
				int h		=	size;
				rectangle 	=	new Rectangle( x,y,w,h );

				page.Tile	=	tile;

				return true;

			} else {
				rectangle	=	new Rectangle();
				return false;
			}
		}



		/// <summary>
		/// Gets gpu data for compute shader that updates page table
		/// </summary>
		/// <returns></returns>
		public PageGpu[] GetGpuPageData ()
		{
			return cache.GetValues()
				.OrderByDescending( pair0 => pair0.VA.MipLevel )
				.Where( pair1 => pair1.Tile!=null )
				.Select( pair2 => new PageGpu( 
					pair2.VA.PageX, 
					pair2.VA.PageY, 
					pair2.X,
					pair2.Y,
					pair2.VA.MipLevel ) )
				.ToArray();
		}



		/// <summary>
		/// Adds new page to cache.
		///		
		///	If page exists:
		///		- LFU index of existing page is shifted.
		///		- returns FALSE
		/// 
		/// If page with given address does not exist:
		///		- page added to cache.
		///		- some pages could be evicted
		///		- return TRUE
		///		
		/// </summary>
		/// <param name="address"></param>
		/// <returns>False if page is already exist</returns>
		public bool Add ( VTAddress virtualAddress, out int physicalAddress )
		{
			Page page;

			if (cache.TryGetValue( virtualAddress, out page )) {

				physicalAddress	=	page.Address;

				return false;

			} else {

				cache.Discard( out page );

				var newPage	=	new Page( virtualAddress, page.Address, pageCount );

				cache.Add( virtualAddress, newPage ); 

				physicalAddress	=	newPage.Address;

				return true;

			}
		}
	}
}

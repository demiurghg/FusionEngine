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

		readonly Dictionary<VTAddress,Page> dictionary;
		readonly Page[]	table;
		readonly int capacity;
		readonly int[] bitCount;
		

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
				//this.TS			=	
			}
			
			public uint LfuIndex = 0xFFFFFFFF;
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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="size">Physical page count</param>
		public VTTileCache ( int physicalPageCount )
		{
			this.capacity	=	physicalPageCount * physicalPageCount;
			table			=	new Page[capacity];
			//mapping			=	new Page[VTConfig.PageCount * VTConfig.PageCount];
			dictionary		=	new Dictionary<VTAddress,Page>(capacity);

			bitCount		=	Enumerable
								.Range(0,256)
								.Select( n => MathUtil.IteratedBitcount( n ) )
								.ToArray();
		}



		public void Purge ()
		{
			dictionary.Clear();
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

			if (dictionary.TryGetValue(address, out page)) {

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



		public PageGpu[] GetGpuPageData ()
		{
			return dictionary
				.OrderByDescending( pair0 => pair0.Key.MipLevel )
				.Where( pair1 => pair1.Value.Tile!=null )
				.Select( pair2 => new PageGpu( 
					pair2.Key.PageX, 
					pair2.Key.PageY, 
					pair2.Value.X,
					pair2.Value.Y,
					pair2.Key.MipLevel ) )
				.ToArray();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Vector4[] GetPageTableData ( int mipLevel )
		{
			var mappingSize = VTConfig.VirtualPageCount >> mipLevel;
			var mapping		= new Vector4[mappingSize*mappingSize];

			var pairList = dictionary
				.OrderByDescending( pair => pair.Key.MipLevel )
				.ToArray();

			foreach ( var pair in pairList ) {

				var va	=	pair.Key;
				var pa	=	pair.Value;

				if (pa.Tile==null || va.MipLevel<mipLevel) {
					continue;
				}

				var sz		=	1 << (va.MipLevel-mipLevel);
				var minMip	=	va.MipLevel;// / (float)VTConfig.MipCount;
				var value	=	new Vector4( pa.X, pa.Y, minMip, 1 );
				//var value	=	MathUtil.PackRGB10A2( new Vector4( pa.X, pa.Y, minMip, 1 ) );


				for ( int x=0; x<sz; x++ ) {
					for ( int y=0; y<sz; y++ ) {

						int vaX = va.PageX * sz + x;
						int vaY = va.PageY * sz + y;

						int addr = vaX + mappingSize * vaY;

						mapping[ addr ] = value;

					}
				}
			}

			return mapping;
		}



		/// <summary>
		/// 
		/// </summary>
		public void UpdateCache ()
		{
			foreach ( var p in table ) {
				if (p!=null) {
					p.LfuIndex = (uint)(p.LfuIndex << 1);
				}
			}

			//	check consistency:

		}


		/// <summary>
		/// Gets page to discard
		/// </summary>
		/// <returns></returns>
		Page GetPageToDiscard ()
		{
			int minBitCount = int.MaxValue;
			Page page = null;

			foreach ( var p in table ) {

				if (p==null) {
					throw new InvalidOperationException("Null page, impossible due to GetFreePhysicalAddress");
				}

				var bitCount = 
					  this.bitCount[ ( p.LfuIndex>>0  ) & 0xFF ]
					+ this.bitCount[ ( p.LfuIndex>>8  ) & 0xFF ]
					+ this.bitCount[ ( p.LfuIndex>>16 ) & 0xFF ]
					+ this.bitCount[ ( p.LfuIndex>>24 ) & 0xFF ];

				if (minBitCount > bitCount) {
					minBitCount = bitCount;
					page = p;
				}
			}

			return page;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="physycalAddress"></param>
		/// <returns></returns>
		bool GetFreePhysicalAddress (out int physycalAddress)
		{
			physycalAddress = -1;
			int length =  table.Length;

			for (int addr = 0; addr < length; addr ++ ) {
				if (table[addr]==null) {
					physycalAddress = addr;
					return true;
				}
			}
			
			return false;
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
		/// <returns></returns>
		public bool Add ( VTAddress virtualAddress, out int physicalAddress )
		{
			Page page;

			if (dictionary.TryGetValue(virtualAddress, out page)) {

				page.LfuIndex	|=	(byte)0x1;

				physicalAddress	=	page.Address;

				return false;

			} else {

				if ( GetFreePhysicalAddress( out physicalAddress ) ) {

					page	=	new Page( virtualAddress, physicalAddress, VTConfig.PhysicalPageCount );
					
					table[ physicalAddress ] = page;
					dictionary.Add( virtualAddress, page );

					return true;

				} else {

					page	=	GetPageToDiscard();
					
					physicalAddress = page.Address;
					dictionary.Remove( page.VA );					
					
					page	=	new Page( virtualAddress, physicalAddress, VTConfig.PhysicalPageCount );

					table[ physicalAddress ] = page;
					dictionary.Add( virtualAddress, page );
	
					return true;
				}

			}
		}
	}
}

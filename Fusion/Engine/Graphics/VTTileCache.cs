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

namespace Fusion.Engine.Graphics {
	public class VTTileCache {

		readonly Dictionary<VTAddress,Page> dictionary;
		readonly Page[]	table;
		readonly int capacity;
		readonly int[] bitCount;
		
		int		counter;

		class Page {
			
			public Page ( VTAddress va, int pa, int physPageCount )
			{
				this.VA			=	va;
				this.Address	=	pa;
				this.X			=	(pa % physPageCount) / (float)physPageCount;
				this.Y			=	(pa / physPageCount) / (float)physPageCount;
			}
			
			public byte LfuIndex = 0xFF;
			public readonly VTAddress VA;
			public readonly int Address;
			public readonly float X;
			public readonly float Y;
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



		/// <summary>
		/// Translates virtual texture address to physical rectangle in physical texture.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="rectangle"></param>
		/// <returns>False if address is not presented in cache</returns>
		public bool TranslateAddress ( VTAddress address, out Rectangle rectangle )
		{
			Page page;

			if (dictionary.TryGetValue(address, out page)) {

				var pa		=	page.Address;
				var ppc		=	VTConfig.PhysicalPageCount;
				var size	=	VTConfig.PageSize;
				int x		=	(pa % ppc) * size;
				int y		=	(pa / ppc) * size;
				int w		=	size;
				int h		=	size;
				rectangle 	=	new Rectangle( x,y,w,h );

				return true;

			} else {
			
				rectangle	=	new Rectangle();
				return false;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Vector4[] GetPageTableData ()
		{
			Vector4[] mapping = new Vector4[VTConfig.VirtualPageCount * VTConfig.VirtualPageCount];

			int vpc = VTConfig.VirtualPageCount;

			var pairList = dictionary
				.OrderByDescending( pair => pair.Key.MipLevel )
				.ToArray();

			foreach ( var pair in pairList ) {

				var va	=	pair.Key;
				var pa	=	pair.Value;
				var sz	=	1 << va.MipLevel;

				for ( int x=0; x<sz; x++ ) {
					for ( int y=0; y<sz; y++ ) {

						int vaX = va.PageX * sz + x;
						int vaY = va.PageY * sz + y;

						int addr = vaX + vpc * vaY;

						float minMip	=	va.MipLevel;

						mapping[ addr ] = new Vector4( pa.X, pa.Y, minMip, 1 );

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
					p.LfuIndex = (byte)(p.LfuIndex << 1);
				}
			}
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

				var bitCount = this.bitCount[ p.LfuIndex ];

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
		///		- LRU index of existing page is increased.
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

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
		
		int		counter;

		class Page {
			
			public Page ( int address, int physPageCount, VTAddress va )
			{
				this.VA			=	va;
				this.Address	=	address;
				this.X			=	(address % physPageCount) / (float)physPageCount;
				this.Y			=	(address / physPageCount) / (float)physPageCount;
			}
			
			public readonly VTAddress VA;
			public readonly int Address;
			public readonly float X;
			public readonly float Y;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="size"></param>
		public VTTileCache ( int size )
		{
			this.capacity	=	size * size;
			table			=	new Page[capacity];
			//mapping			=	new Page[VTConfig.PageCount * VTConfig.PageCount];
			dictionary		=	new Dictionary<VTAddress,Page>(capacity);
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

						mapping[ addr ] = new Vector4( pa.X, pa.Y, va.MipLevel, 1 );

					}
				}
			}

			return mapping;
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

				physicalAddress	=	 page.Address;
				return false;

			} else {

				page				=	new Page( counter, VTConfig.PhysicalPageCount, virtualAddress );

				if (table[counter]!=null) {
					dictionary.Remove( table[counter].VA );
				}

				table[ counter ]	=	page;
				counter				=	(counter+1) % capacity;

				dictionary.Add( virtualAddress, page );

				physicalAddress		=	page.Address;

				return true;
			}
		}
	}
}

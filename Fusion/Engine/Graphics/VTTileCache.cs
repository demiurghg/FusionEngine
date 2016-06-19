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
			
			public Page ( int address )
			{
				this.Address	=	address;
			}
			
			public readonly int Address;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="size"></param>
		public VTTileCache ( int size )
		{
			this.capacity	=	size * size;
			table			=	new Page[capacity];
			dictionary		=	new Dictionary<VTAddress,Page>(capacity);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="rectangle"></param>
		/// <returns></returns>
		public bool Translate ( VTAddress address, out Rectangle rectangle )
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
		public bool Add ( VTAddress address, out int physicalAddress )
		{
			Page page;

			if (dictionary.TryGetValue(address, out page)) {

				physicalAddress	=	 page.Address;
				return false;

			} else {

				page				=	new Page( counter );
				table[ counter ]	=	page;
				counter				=	(counter+1) % capacity;

				dictionary.Add( address, page );

				physicalAddress		=	page.Address;

				return true;
			}
		}
	}
}

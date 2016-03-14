using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using System.Collections;

namespace Fusion.Engine.Graphics {
	internal class SpriteLayerCollection : ICollection<SpriteLayer> {

		private List<SpriteLayer> layerList;


		/// <summary>
		/// Creates instance of SpriteLayerCollection
		/// </summary>
		public SpriteLayerCollection ()
		{
			layerList	=	new List<SpriteLayer>();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator() 
		{ 
			return ((IEnumerable)layerList).GetEnumerator();
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerator<SpriteLayer> GetEnumerator() {
			return layerList.GetEnumerator();
		}



		/// <summary>
		/// Gets the number of elements contained in the SpriteLayerCollection.
		/// </summary>
		public int Count {
			get {
				return layerList.Count;
			}
		}



		/// <summary>
		/// Gets a value indicating whether the SpriteLayerCollection
		/// is read-only.
		/// </summary>
		public bool IsReadOnly {
			get {
				return false;
			}
		}


		/// <summary>
		/// Adds sprite layer to SpriteLayerCollection
		/// </summary>
		/// <param name="spriteLayer"></param>
		public void Add ( SpriteLayer spriteLayer )
		{
			if (spriteLayer==null) {
				throw new ArgumentNullException("spriteLayer");
			}
			layerList.Add( spriteLayer );
		}


		/// <summary>
		/// Clears SpriteLayerCollection
		/// </summary>
		public void Clear ()
		{
			layerList.Clear();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="spriteLayer"></param>
		/// <returns></returns>
		public bool Contains ( SpriteLayer spriteLayer )
		{
			return layerList.Contains( spriteLayer );
		}
	

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo ( SpriteLayer[] array, int arrayIndex )
		{
			layerList.CopyTo( array, arrayIndex );
		}



		/// <summary>
		/// Removes sprite layer from SpriteLayerCollection
		/// </summary>
		/// <param name="spriteLayer"></param>
		/// <returns></returns>
		public bool Remove ( SpriteLayer spriteLayer )
		{
			return layerList.Remove( spriteLayer );
		}

	}
}

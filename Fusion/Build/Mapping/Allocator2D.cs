using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using System.Diagnostics;

namespace Fusion.Build.Mapping {


	/// <summary>
	/// http://www.memorymanagement.org/mmref/alloc.html
	/// </summary>
	public partial class Allocator2D {

		public readonly int Size;

			class Block {

			public readonly Int2	Address;
			public readonly int		Size;
			
			public Block ( Int2 address, int size )
			{
				this.Address	=	address;
				this.Size		=	size;
			}
		}


		LinkedList<Block> freeBlocks;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="size"></param>
		public Allocator2D ( int size = 1024 )
		{
			freeBlocks	=	new LinkedList<Block>();
			freeBlocks.AddFirst( new Block( new Int2(0,0), 1024 ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public Int2 Alloc ( int size, object tag )
		{
			if (tag==null) {
				throw new ArgumentNullException("tag");
			}
			if (size<=0) {
				throw new ArgumentOutOfRangeException("size");
			}

			size		=	MathUtil.RoundUpNextPowerOf2( size );


			//	search for empty block :
			var node = GetFreeBlockNode( size );

			while (node.Value.Size>size) {
				node = SplitBlockNode( node );
			}

			freeBlocks.Remove( node );

			return node.Value.Address;
		}



		/// <summary>
		/// Gets first free block with given minimum size.
		/// </summary>
		/// <param name="minimumSize"></param>
		/// <returns></returns>
		LinkedListNode<Block> GetFreeBlockNode ( int minimumSize )
		{
			var node = freeBlocks.First;

			while (node!=null) {
				if (node.Value.Size >= minimumSize) {
					return node;
				}
				node = node.Next;
			}

			throw new OutOfMemoryException(string.Format("No free blocks (size={0})", minimumSize));
		}



		/// <summary>
		/// Splits node onto four smaller nodes and returns first one.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		LinkedListNode<Block> SplitBlockNode ( LinkedListNode<Block> node )
		{
			var addr	=	node.Value.Address;
			var size	=	node.Value.Size / 2;

			var block00	=	new Block( new Int2( addr.X,		addr.Y			), size );
			var block01	=	new Block( new Int2( addr.X + size, addr.Y			), size );
			var block10	=	new Block( new Int2( addr.X,		addr.Y + size	), size );
			var block11	=	new Block( new Int2( addr.X + size, addr.Y + size	), size );

			var node11	=	freeBlocks.AddAfter( node, block11 );
			var node10	=	freeBlocks.AddAfter( node, block10 );
			var node01	=	freeBlocks.AddAfter( node, block01 );
			var node00	=	freeBlocks.AddAfter( node, block00 );

			freeBlocks.Remove( node );

			return node00;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		public void Free ( Int2 address )
		{
			throw new NotImplementedException();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Size2 GetBlockSize ( Int2 address )
		{
			throw new NotImplementedException();
		}
	}
}

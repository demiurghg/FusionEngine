using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using System.Diagnostics;

namespace Fusion.Build.Mapping {
	public partial class Allocator2D {

		public readonly int Size;

			[DebuggerDisplay("InUse={InUse}; Size={Size}; Children={Children}")]
			class Block {

			public object			Tag;
			public readonly Int2	Address;
			public readonly int		Size;
			public readonly Block	Parent;
			
			public Block[] Children { get; private set; }
			
			public Block ( Int2 address, int size, Block parent )
			{
				this.Address	=	address;
				this.Size		=	size;
			}


			public void Split ()
			{
				if (Size<=1) {
					throw new InvalidOperationException("Can not split block with size 1");
				}

				if (Children!=null) {
					return;
				}

				Children	=	new Block[4];
				var addr	=	this.Address;
				var size	=	this.Size / 2;
										   
				Children[0]	=	new Block( new Int2( addr.X,		addr.Y			), size, this );
				Children[1]	=	new Block( new Int2( addr.X + size, addr.Y			), size, this );
				Children[2]	=	new Block( new Int2( addr.X,		addr.Y + size	), size, this );
				Children[3]	=	new Block( new Int2( addr.X + size, addr.Y + size	), size, this );
			}
		}


		Block rootBlock;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="size"></param>
		public Allocator2D ( int size = 1024 )
		{
			rootBlock	=	new Block( new Int2(0,0), size, null );
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

			var block	=	GetFreeBlock( size );

			/*while (block.Size > size) {
				block = block.Split();
			} */

			block.Tag	=	tag;
			return block.Address;
		}



		Block GetFreeBlock( int size )
		{
			var Q = new Stack<Block>();

			Q.Push( rootBlock );

			while ( Q.Any() ) {
				
				var block = Q.Pop();

				//	block is already used:
				if (block.Tag!=null) {
					continue;
				}

				//	we've found free block of given size:
				if (block.Size==size && block.Children==null) {
					return block;
				}

				//	block is too large, split it:
				if (block.Size>size) {
					block.Split();
				}

				foreach ( var child in block.Children ) {
					Q.Push( child );
				}
			}

			throw new OutOfMemoryException("No free blocks size=" + size.ToString());
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

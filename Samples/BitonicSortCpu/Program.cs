using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitonicSort {
	class Program {

		const int Log2Size = 5;
		const int Size = 1 << Log2Size;


		static void Print ( int step, int[] data )
		{
			Console.WriteLine( step.ToString("D3") + ": " + string.Join(" ", data.Select( i => string.Format("{0,2}", i) ) ) );
		}


		static void CSwap ( ref int a, ref int b )
		{
			if (a>b) {
				int t = a;
				a = b;
				b = t;
			}
		}


		static void Kernel ( int[] a, int p, int q ) 
		{
			int d = 1 << (p-q);

			for( int i=0;i<a.Length;i+=2 ) {
				
				bool up = ((i >> p) & 2) == 0;

				if ((i & d) == 0 && (a[i] > a[i | d]) == up) {
					int t = a[i]; 
					a[i] = a[i | d]; 
					a[i | d] = t;
				}
			}

			for( int i=1;i<a.Length;i+=2 ) {
				
				bool up = ((i >> p) & 2) == 0;

				if ((i & d) == 0 && (a[i] > a[i | d]) == up) {
					int t = a[i]; 
					a[i] = a[i | d]; 
					a[i | d] = t;
				}
			}
		}


		static void BitonicSort ( int[] a, int logLen )
		{
			for( int i=0; i<logLen; i++ ) {
				for( int j=0; j<=i; j++ ) {
					Kernel(a, i, j);
				}
			}
		}


		/*static void Block ( int[] data,  )
		{
			for (int i=0; i<data.Length/2; i++) {
				CSwap( ref data[i*2], ref data[i*2+1] );
			}
		} */



		static void Main ( string[] args )
		{
			var rand	=	new Random();
			var data	=	Enumerable.Range(0,Size).Select( i => rand.Next(100) ).ToArray();

			Print( 0, data );

			BitonicSort( data, Log2Size );

			Print( 1, data );
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; 

namespace Fusion.Build.Mapping {
	class Map {
		
		public Map ()
		{
		}




		public static Map Load ( Stream	stream )
		{
			return new Map();
		}


		public static void Save ( Map map, Stream stream )
		{
			
		}
	}
}

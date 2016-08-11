using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Build2 {
	
	public class ContentProject {

		
		/// <summary>
		/// Collection of binary directories.
		/// Directory could be absolute or relative path, registry or environment variable.
		/// </summary>
		public ICollection<string> BinaryDirectories {
			get; private set;
		}

		
		/// <summary>
		/// Collection of content directories.
		/// Directory could be absolute or relative path, registry or environment variable.
		/// </summary>
		public ICollection<string> ContentDirectories {
			get; private set;
		}





	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fusion.Editor {

	/// <summary>
	/// Represents editor settings
	/// </summary>
	public class EditorContext {

		public string Name { get; set; }
		public string FileExt { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public EditorContext ()
		{
		}
	}
}

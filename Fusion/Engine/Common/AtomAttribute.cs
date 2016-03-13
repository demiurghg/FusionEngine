using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Core.Content;
using Lidgren.Network;

namespace Fusion.Engine.Common {

	/// <summary>
	/// Represents distributed/remote atoms colleciton.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class AtomAttribute : Attribute {
		
		public readonly string Atom;

		public AtomAttribute ( string atom )
		{
			this.Atom	=	atom;
		}
	}
}

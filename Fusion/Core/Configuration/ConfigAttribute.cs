using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Configuration {

	/// <summary>
	/// Marks property as config
	/// 
	/// Note on multi-threading:
	///		Be sure that all structure properties 
	///		larger than 4 (32-bit) or 8 (64-bit) bytes in config classes 
	///		have lock on set and get.
	/// </summary>
	public sealed class ConfigAttribute : Attribute {
	}
}

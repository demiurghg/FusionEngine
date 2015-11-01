using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Fusion.Engine.Common {
	
	#pragma warning disable 0618

	internal static class NetUtils {

		static public bool IsIPsEqual ( IPEndPoint a, IPEndPoint b )
		{
			return ( a.Address.MapToIPv4().Equals( b.Address.MapToIPv4() ) && a.Port == b.Port );
		}
		
	}
}

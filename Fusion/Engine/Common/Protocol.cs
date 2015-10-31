using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Common {
	static class Protocol {

		public const string ClientConnect		=	"CL_CONNECT";
		public const string ClientDisconnect	=	"CL_DISCONNECT";

		public const string ServerConnectAck	=	"SV_CONNECT_ACK";
		public const string ServerConnectRefuse	=	"SV_CONNECT_REFUSE";


	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Server {
	internal enum NetCommand : byte {
		UserCommand,
		Snapshot,
		Notification,
	}
}

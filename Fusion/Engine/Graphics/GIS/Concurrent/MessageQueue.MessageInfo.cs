using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics.GIS.Concurrent
{
	public partial class MessageQueue
	{

		// Executes in the main thread
		public class WorkRequest
		{
			[Flags]
			public enum InfoFlags
			{
				None				= 1 << 0,
				FillRWQueue			= 1 << 1,
				FillProcessQueue	= 1 << 2,
			}

			public InfoFlags Flags;

			public MessageQueue DiskWRQueue;
			public MessageQueue ProcessQueue;
			public MessageQueue DoneQueue;


			// TODO: create redirect flags

			public WorkRequest(Action<WorkRequest> callback, object data, object done)
			{
				Callback	= callback;
				Data		= data;
				Done		= done;
			}

			// Object should be WorkRequest class
			public Action<WorkRequest> Callback;
			public object Data;
			public object Done;
		}

	}
}

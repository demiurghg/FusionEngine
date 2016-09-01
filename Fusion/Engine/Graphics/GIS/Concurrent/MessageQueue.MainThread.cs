using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics.GIS.Concurrent
{
	public partial class MessageQueue
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////

		// We should consider different use cases:
		// 1. Read file from disk only														// Executes in disk io thread
		// 2. Process only																	// Executes in process thread
		// 3. Read and process after and vice versa											// 
		// 4. Write data to disk
		// 5. Download file (from internet) to disk											// Same as 
		// 6. Download data from internet and send it to process and write it to disk		// 
		// 7. Read chunk of big file and process it, repeat to the end of the file

		private CancellationTokenSource CancellationRequest;

		private List<MessageQueue>	ProcessQueues;
		private MessageQueue		DiskRWQueue;

		const int	ProcessQueueCount = 4;
		int			CurrentProcessQueueIndex = 0;


		/// <summary>
		/// Starts the main thread queue in a separate thread.  This method returns immediately.
		/// The thread created by this method will continue running until <see cref="Terminate"/>
		/// is called.
		/// </summary>
		public void StartMainThread()
		{
			lock (queue)
			{
				if (state != State.Stopped)
					throw new InvalidOperationException("The MessageQueue is already running.");
				state = State.Running;
			}


			ProcessQueues	= new List<MessageQueue>();
			DiskRWQueue		= new MessageQueue();

			DiskRWQueue.StartInAnotherThread();

			for (int i = 0; i < ProcessQueueCount; i++) {
				var q = new MessageQueue();
				q.StartInAnotherThread();
				ProcessQueues.Add(q);
			}


			CancellationRequest = new CancellationTokenSource();


			// TODO: change thread to task
			Thread thread = new Thread(() => MainThread(CancellationRequest.Token));
			thread.IsBackground = true;
			thread.Start();
		}



		public void MainThread(CancellationToken token)
		{
			Log.Debug("MainThread reporting for duty!!!");

			List<WorkRequest> current = new List<WorkRequest>();

			while (true)
			{
				// Check is cancellation requested
				if (token.IsCancellationRequested)
				{
					Log.Debug("MainThread finish his mission!");
					return;
				}

				// Check new items in the queue
				lock (queue)
				{
					if (queue.Count > 0)
					{
						current.AddRange(queue);
						queue.Clear();
					}
					else
					{
						Monitor.Wait(queue);

						current.AddRange(queue);
						queue.Clear();
					}
				}

				// Process items in queue
				//ProcessCurrentQueue(current);

				foreach (var request in current) {
					if (request.Flags.HasFlag(WorkRequest.InfoFlags.FillProcessQueue))	request.ProcessQueue	= ProcessQueues[CurrentProcessQueueIndex++ % ProcessQueueCount];
					if (request.Flags.HasFlag(WorkRequest.InfoFlags.FillRWQueue))		request.DiskWRQueue		= DiskRWQueue;

					request.Callback(request);
				}

				current.Clear();
			}

		}
	}
}

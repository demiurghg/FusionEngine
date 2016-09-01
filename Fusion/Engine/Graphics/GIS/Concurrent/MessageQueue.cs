using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics.GIS.Concurrent
{
	public class MessageQueueEventArgs : EventArgs
	{
		public MessageQueueEventArgs(object message)
		{
			this.message = message;
		}

		public	object Message { get { return message; } }
		private object message;
	}


	public partial class MessageQueue
	{
		/// <summary>
		/// Raised when a message is received by the message queue.  This event is raised in the thread that
		/// is handling messages for the queue.
		/// </summary>
		public event EventHandler<MessageQueueEventArgs> MessageReceived;

		/// <summary>
		/// Starts processing the queue in the current thread.  This method does not return until
		/// <see cref="Terminate"/> is called.
		/// </summary>
		public void StartInCurrentThread()
		{
			lock (queue) {
				if (state != State.Stopped)
					throw new InvalidOperationException("The MessageQueue is already running.");
				state = State.Running;
			}

			Run();
		}

		/// <summary>
		/// Starts processing the queue in a separate thread.  This method returns immediately.
		/// The thread created by this method will continue running until <see cref="Terminate"/>
		/// is called.
		/// </summary>
		public void StartInAnotherThread()
		{
			lock (queue) {
				if (state != State.Stopped)
					throw new InvalidOperationException("The MessageQueue is already running.");
				state = State.Running;
			}

			// TODO: change thread to task
			Thread thread		= new Thread(Run);
			thread.IsBackground = true;
			thread.Start();
		}

		/// <summary>
		/// Processes messages currently in the queue using the calling thread.  This method returns as soon as all
		/// messages currently in the queue have been processed.
		/// </summary>
		public void ProcessQueue()
		{
			List<WorkRequest> current = null;

			lock (queue) {
				if (state != State.Stopped)
					throw new InvalidOperationException("The MessageQueue is already running.");

				if (queue.Count > 0) {
					state	= State.Running;
					current = new List<WorkRequest>(queue);
					queue.Clear();
				}
			}

			if (current != null) {
				try {
					ProcessCurrentQueue(current);
				}
				finally {
					lock (queue) {
						state = State.Stopped;
					}
				}
			}
		}

		/// <summary>
		/// Stops queue processing started by <see cref="StartInCurrentThread"/>, <see cref="StartInAnotherThread"/>,
		/// or <see cref="ProcessQueue"/>.  This method returns immediately without waiting for the message queue
		/// to stop.  To wait for the message queue to stop, call <see cref="TerminateAndWait"/> instead.  If the message
		/// queue is not running when this method is called, a "stop" message will be queued such that the message
		/// queue will be stopped when it starts processing messages again.  
		/// </summary>
		public void Terminate()
		{
			Post(StopQueue, null);
		}

		/// <summary>
		/// Stops queue processing started by <see cref="StartInCurrentThread"/>, <see cref="StartInAnotherThread"/>,
		/// or <see cref="ProcessQueue"/>.  This method does not return until the message queue has stopped.
		/// To signal the message queue to terminate without waiting, call <see cref="Terminate"/> instead.  If the message
		/// queue is not running when this method is called, a "stop" message will be queued such that the message
		/// queue will be stopped when it starts processing messages again, and the calling thread will be blocked
		/// until that happens.
		/// </summary>
		public void TerminateAndWait()
		{
			Send(StopQueue, null);
		}

		/// <summary>
		/// Posts a delegate to the queue.  This method returns immediately without waiting for the delegate to be invoked.
		/// </summary>
		/// <param name="callback">The callback to invoke when the message is processed.</param>
		/// <param name="userData">Optional data to pass to the <paramref name="callback"/> when it is invoked.</param>
		public void Post(Action<WorkRequest> callback, object userData)
		{
			lock (queue) {
				queue.Add(new WorkRequest(callback, userData, null) {Flags = WorkRequest.InfoFlags.FillProcessQueue | WorkRequest.InfoFlags.FillRWQueue});
				Monitor.Pulse(queue);
			}
		}

		/// <summary>
		/// Posts a message to the queue.  This method returns immediately without waiting for the message to be processed.
		/// </summary>
		/// <param name="message">The message to post to the queue.</param>
		public void Post(object message)
		{
			lock (queue) {
				queue.Add(new WorkRequest(null, message, null));
				Monitor.Pulse(queue);
			}
		}

		/// <summary>
		/// Sends a delegate to the queue.  This method waits for the delegate to be invoked in the queue thread
		/// before returning.  Calling this message from the queue thread itself will result in a deadlock.
		/// </summary>
		/// <param name="callback">The callback to invoke when the message is processed.</param>
		/// <param name="userData">Optional data to pass to the <paramref name="callback"/> when it is invoked.</param>
		public void Send(Action<object> callback, object userData)
		{
			WorkRequest workRequest = new WorkRequest(callback, userData, new object());
			lock (workRequest.Done) {
				lock (queue) {
					queue.Add(workRequest);
					Monitor.Pulse(queue);
				}
				Monitor.Wait(workRequest.Done);
			}
		}

		/// <summary>
		/// Sends a message to the queue.  This method waits for the delegate to be invoked in the queue thread
		/// before returning.  Calling this message from the queue thread itself will result in a deadlock.
		/// </summary>
		/// <param name="message">The message to post to the queue.</param>
		public void Send(object message)
		{
			WorkRequest workRequest = new WorkRequest(null, message, new object());
			lock (workRequest.Done) {
				lock (queue) {
					queue.Add(workRequest);
					Monitor.Pulse(queue);
				}
				Monitor.Wait(workRequest.Done);
			}
		}

		/// <summary>
		/// Blocks the calling thread until a message is waiting in the queue.
		/// This message should only be called on queues for which messages are processed
		/// explicitly with a call to <see cref="ProcessQueue"/>.
		/// </summary>
		public void WaitForMessage()
		{
			lock (queue) {
				while (queue.Count == 0) {
					Monitor.Wait(queue);
				}
			}
		}

		/// <summary>
		/// Calls <see cref="Terminate"/>.
		/// </summary>
		public void Dispose()
		{
			Terminate();
		}

		private void Run()
		{
			try {
				List<WorkRequest> current = new List<WorkRequest>();

				do {
					lock (queue) {
						if (queue.Count > 0) {
							current.AddRange(queue);
							queue.Clear();
						}
						else {
							Monitor.Wait(queue);

							current.AddRange(queue);
							queue.Clear();
						}
					}

					ProcessCurrentQueue(current);
					current.Clear();
				} while (state == State.Running);
			}
			finally {
				lock (queue) {
					state = State.Stopped;
				}
			}
		}

		private void ProcessCurrentQueue(List<WorkRequest> currentQueue)
		{
			for (int i = 0; i < currentQueue.Count; ++i) {
				if (state == State.Stopping) {
					// Push the remainder of 'current' back into '_queue'.
					lock (queue) {
						currentQueue.RemoveRange(0, i);
						queue.InsertRange(0, currentQueue);
					}
					break;
				}
				ProcessMessage(currentQueue[i]);
			}
		}

		private void ProcessMessage(WorkRequest message)
		{
			if (message.Callback != null) {
				message.Callback(message);
			}
			else {
				EventHandler<MessageQueueEventArgs> e = MessageReceived;
				if (e != null) {
					e(this, new MessageQueueEventArgs(message.Data));
				}
			}

			if (message.Done != null) {
				lock (message.Done) {
					Monitor.Pulse(message.Done);
				}
			}
		}

		private void StopQueue(object userData)
		{
			state = State.Stopping;
		}


		private enum State
		{
			Stopped,
			Running,
			Stopping,
		}

		private List<WorkRequest>	queue = new List<WorkRequest>();
		private State				state = State.Stopped;
	}
}

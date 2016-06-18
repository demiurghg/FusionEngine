using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Client {
	class JitterBuffer {


		class BufferedSnapshot {

			public readonly byte[]	SnapshotData;
			public readonly uint	AckCmdID;
			public readonly long	ServerTicks;

			public BufferedSnapshot ( byte[] snapshotData, uint ackCmdID, long serverTicks )
			{
				SnapshotData	=	snapshotData;
				AckCmdID		=	ackCmdID;
				ServerTicks		=	serverTicks;
			}
		}


		Queue<BufferedSnapshot> snapshots;

		readonly long initialServerTicks;
		readonly Game game;

		/// <summary>
		/// due to initialServerTicks clientServerDelta should be set zero.
		/// </summary>
		long clientServerDelta	=	0;


		/// <summary>
		/// Creates instance of jitter buffer.
		/// </summary>
		public JitterBuffer ( Game game, long initialServerTicks )
		{
			this.game				=	game;
			this.initialServerTicks =	initialServerTicks;
			snapshots	=	new Queue<BufferedSnapshot>(5);
		}



		bool pushed = false;
		long lastServerTicks = 0;

		long playoutDelay	=	50 * 10000L;


		/// <summary>
		/// Pushes snapshot to the queue. 
		/// </summary>
		/// <param name="snapshotData"></param>
		/// <param name="ackCmdID"></param>
		/// <param name="serverTicks"></param>
		public void Push ( byte[] snapshotData, uint ackCmdID, long serverTicks, long clientTicks )
		{
			//	FIXME: actually snapshots should not be unoredered, but...
			snapshots.Enqueue( new BufferedSnapshot(snapshotData, ackCmdID, serverTicks) );

			var serverTicksBiased	=	serverTicks - initialServerTicks;
			var currentDelta		=	clientTicks - serverTicksBiased;

			//clientServerDelta		=	Drift( clientServerDelta, currentDelta, 50000, 500 );
			//clientServerDelta		=	Math.( clientServerDelta, currentDelta, 50000, 500 );

			clientServerDelta		=	SlowDecay( clientServerDelta, currentDelta, 5000 );

			pushed			= true;
			lastServerTicks	= serverTicks - initialServerTicks;
		}



		long SlowDecay ( long current, long target, long decayRate )
		{
			if (current<target) {
				return target;
			} else {
				
				if (current>target+decayRate) {
					return current - decayRate;
				} else {
					return target;
				}
			}
		}


		/// <summary>
		/// Drifts value to target with constant velocity.
		/// </summary>
		/// <param name="current"></param>
		/// <param name="target"></param>
		/// <param name="up"></param>
		/// <param name="down"></param>
		/// <returns></returns>
		long Drift ( long current, long target, long up, long down )
		{
			if (current==target) {
				return target;
			}

			//	go down :
			if (current>target) {
				if (current>target+down) {
					return current - down;
				} else {
					return target;
				}
			}

			// go up:
			if (current<target) {
				if (current<target-up) {
					return current + up;
				} else {
					return target;
				}
			}

			return current;
		}



		string SignedDelta ( long sv, long cl )
		{
			if (sv==0) {
				return "--";
			}

			var value = (cl - sv)/10000;

			return value.ToString();
		}


		/// <summary>
		/// Shows incoming message jittering stuff
		/// </summary>
		/// <param name="queueSize"></param>
		/// <param name="svTicks"></param>
		/// <param name="clTicks"></param>
		/// <param name="push"></param>
		/// <param name="pop"></param>
		void ShowJitter ( int queueSize, long svTicks, long clTicks, bool push, bool pull, long delay )
		{											
			var queue = (new string('*', queueSize)).PadLeft(5,' ');

			Log.Message("{1}[{0}]{2} {3,8} {4,8} | {5,5}  {6,5} {7,5} {8,5}", 
				queue, 
				push?">>":"  ",
				pull?">>":"  ",
				svTicks / 10000,
				clTicks / 10000,
				SignedDelta( svTicks, clTicks ),
				clientServerDelta/10000,
				delay/10000,
				clientServerDelta/10000 + delay/10000
			);
		} 


		int errorCounter	=	0;


		/// <summary>
		/// Pops snapshot from the queue.
		/// </summary>
		/// <param name="clientTicks"></param>
		/// <returns></returns>
		public byte[] Pop ( long clientTicks, int playoutDelay, out uint ackCmdID )
		{
			ackCmdID		=	0;
			byte[] result	=	null;

			int queueSize			=	snapshots.Count;
			var pulled				=	false;
			var bufferedSnapshot	=	(BufferedSnapshot)null;
			var	serverTicksBiased	=	0L;

			if (snapshots.Any()) {

				bufferedSnapshot	=	snapshots.Peek();
				serverTicksBiased	=	bufferedSnapshot.ServerTicks - initialServerTicks;

				if (serverTicksBiased < (clientTicks - clientServerDelta - playoutDelay*10000) ) {

					snapshots.Dequeue();

					pulled		=	true;

					ackCmdID	=	bufferedSnapshot.AckCmdID;
					result		=	bufferedSnapshot.SnapshotData;

				} else {
					result	=	null;
				}

			} else {
				result	=	null;
			}

			//if (game.Network.Config.ShowJitter) {
			//	ShowJitter( queueSize, serverTicksBiased, clientTicks, pushed, pulled, playoutDelay*10000 );
			//}

			pushed	=	false;

			return result;
		}




		#if false
		/// <summary>
		/// Pops snapshot from the queue.
		/// </summary>
		/// <param name="clientTicks"></param>
		/// <returns></returns>
		public byte[] Pop ( long clientTicks, int playoutDelay, long latencyTicks, out uint ackCmdID )
		{
			ackCmdID		=	0;
			byte[] result	=	null;

			int queueSize			=	snapshots.Count;
			var pulled				=	false;
			var bufferedSnapshot	=	(BufferedSnapshot)null;
			var	serverTicksBiased	=	0L;

			if (snapshots.Any()) {

				bufferedSnapshot	=	snapshots.Peek();
				serverTicksBiased	=	bufferedSnapshot.ServerTicks - initialServerTicks + latencyTicks;

				if (serverTicksBiased < (clientTicks - playoutDelay * 10000 + clientServerDelta*0) ) {

					snapshots.Dequeue();

					pulled		=	true;

					ackCmdID	=	bufferedSnapshot.AckCmdID;
					result		=	bufferedSnapshot.SnapshotData;

				} else {
					result	=	null;
				}

			} else {
				result	=	null;
			}

			if (game.Network.Config.ShowJitter) {
				ShowJitter( queueSize, serverTicksBiased, clientTicks, pushed, pulled, latencyTicks );
			}

			pushed	=	false;

			return result;
		}
		#endif

		
	}
}

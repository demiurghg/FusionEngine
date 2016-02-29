using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		const long MinDelta	=	-500 * 10000;
		const long MaxDelta	=	 500 * 10000;


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
			var currentDelta		=	serverTicksBiased - clientTicks;
			clientServerDelta		=	Drift( clientServerDelta, currentDelta, 20000 /* 1ms */ );

			pushed			= true;
			lastServerTicks	= serverTicks - initialServerTicks;
		}



		/// <summary>
		/// Drifts value to target with constant velocity.
		/// </summary>
		/// <param name="current"></param>
		/// <param name="target"></param>
		/// <param name="velocity"></param>
		/// <returns></returns>
		long Drift ( long current, long target, long velocity )
		{
			if ( Math.Abs(current - target) < velocity ) {
				return target;
			}

			if (current>target) {
				return current - velocity;
			}

			if (current<target) {
				return current + velocity;
			}

			return current;
		}



		/// <summary>
		/// Shows incoming message jittering stuff
		/// </summary>
		/// <param name="queueSize"></param>
		/// <param name="svTicks"></param>
		/// <param name="clTicks"></param>
		/// <param name="push"></param>
		/// <param name="pop"></param>
		void ShowJitter ( int queueSize, long svTicks, long clTicks, bool push, bool pull )
		{
			Log.Message("qs:{0} [{1}{2}] sv:{3} cl:{4}", 
				queueSize, 
				push?"enq":"   ",
				pull?"deq":"   ",
				svTicks / 10000,
				clTicks / 10000
				);
		} 



		/// <summary>
		/// Pops snapshot from the queue.
		/// </summary>
		/// <param name="clientTicks"></param>
		/// <returns></returns>
		public byte[] Pop ( long clientTicks, int playoutDelay, out uint ackCmdID )
		{
			ackCmdID		=	0;
			byte[] result	=	null;

			#if true

			int queueSize	=	snapshots.Count;
			bool pulled		=	false;

			if (snapshots.Any()) {

				var bufferedSnapshot	=	snapshots.Peek();

				var serverTicksBiased	=	bufferedSnapshot.ServerTicks - initialServerTicks;

				if (serverTicksBiased < (clientTicks - playoutDelay * 10000) ) {

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
				ShowJitter( queueSize, lastServerTicks, clientTicks, pushed, pulled	);
			}

			pushed	=	false;

			return result;

			#else

			if (snapshots.Any()) {

				var bs		=	snapshots.Dequeue();
				ackCmdID	=	bs.AckCmdID;

				var svTick	=	bs.ServerTicks - initialServerTicks;

				var currentDelta	=	svTick - clientTicks;

				clientServerDelta	=	Drift( clientServerDelta, currentDelta, 20000 /* 1ms */ );
			
				Log.Verbose("snaps:{0} sv:{1:0.00} svb:{2:0.00} cl:{3:0.00} delta:{4:0.00} drift:{5:0.00}", snapshots.Count+1, bs.ServerTicks/10000.0f, svTick/10000.0f, clientTicks/10000.0f, (svTick - clientTicks)/10000.0f, clientServerDelta/10000.0f );
			
				return bs.SnapshotData;

			} else {
				return null;
			}
			#endif
		}

		
	}
}

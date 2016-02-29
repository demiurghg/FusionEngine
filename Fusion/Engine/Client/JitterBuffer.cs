using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		/// <summary>
		/// due to initialServerTicks clientServerDelta should be set zero.
		/// </summary>
		long clientServerDelta	=	0;

		const long MinDelta	=	-500 * 10000;
		const long MaxDelta	=	 500 * 10000;


		/// <summary>
		/// Creates instance of jitter buffer.
		/// </summary>
		public JitterBuffer ( long initialServerTicks )
		{
			this.initialServerTicks = initialServerTicks;
			snapshots	=	new Queue<BufferedSnapshot>(5);
		}



		/// <summary>
		/// Pushes snapshot to the queue. 
		/// </summary>
		/// <param name="snapshotData"></param>
		/// <param name="ackCmdID"></param>
		/// <param name="serverTicks"></param>
		public void Push ( byte[] snapshotData, uint ackCmdID, long serverTicks )
		{
			snapshots.Enqueue( new BufferedSnapshot(snapshotData, ackCmdID, serverTicks) );
		}



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
		/// Pops snapshot from the queue.
		/// </summary>
		/// <param name="clientTicks"></param>
		/// <returns></returns>
		public byte[] Pop ( long clientTicks, out uint ackCmdID )
		{
			ackCmdID	=	0;

			if (snapshots.Any()) {

				var bs		=	snapshots.Dequeue();
				ackCmdID	=	bs.AckCmdID;

				var svTick	=	bs.ServerTicks - initialServerTicks;

				var currentDelta	=	svTick - clientTicks;

				clientServerDelta	=	Drift( clientServerDelta, currentDelta, 10000 /* 1ms */ );
				
				Log.Verbose("snaps:{0} sv:{1:0.00} svb:{2:0.00} cl:{3:0.00} delta:{4:0.00} drift:{5:0.00}", snapshots.Count+1, bs.ServerTicks/10000.0f, svTick/10000.0f, clientTicks/10000.0f, (svTick - clientTicks)/10000.0f, clientServerDelta/10000.0f );
				
				return bs.SnapshotData;

			} else {
				return null;
			}
		}

		
	}
}

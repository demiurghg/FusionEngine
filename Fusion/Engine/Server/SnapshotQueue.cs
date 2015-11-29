using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Network;
using Fusion.Engine.Common;

namespace Fusion.Engine.Server {
	
	class SnapshotQueue {

		uint frameCounter	=	0;

		readonly int capacity;
				
		
		Queue<Snapshot> queue;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="capacity"></param>
		public SnapshotQueue ( int capacity )
		{
			this.capacity	=	capacity;
			queue			=	new Queue<Snapshot>( capacity );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="snapshot"></param>
		public void Push ( byte[] snapshot )
		{
			frameCounter ++;
			Push( new Snapshot(frameCounter, snapshot) );			
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="snapshot"></param>
		public void Push ( Snapshot snapshot )
		{
			queue.Enqueue ( snapshot );

			while (queue.Count>capacity) {
				queue.Dequeue();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public uint LastFrame {
			get {
				return queue.Last().Frame;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="prevFrame"></param>
		/// <returns></returns>
		public byte[] Compress ( ref uint prevFrame )
		{
			var lastSnapshot	=	queue.Peek();

			var prevFrameLocal	=	prevFrame;

			var prevSnapshot	=	queue.SingleOrDefault( s => s.Frame == prevFrameLocal );

			if (prevSnapshot==null) {
				prevFrame = 0;
				return NetworkEngine.Compress( lastSnapshot.Data );
			}
			

			var delta	=	new byte[lastSnapshot.Data.Length];
			var minSize	=	Math.Min( delta.Length, prevSnapshot.Data.Length );

			for (int i=0; i<minSize; i++ ) {
				delta[i] = (byte)(lastSnapshot.Data[i] ^ prevSnapshot.Data[i]);
			}

			if (delta.Length > prevSnapshot.Data.Length) {
				for (int i=minSize; i<delta.Length; i++) {
					delta[i] = lastSnapshot.Data[i];
				}
			}

			return NetworkEngine.Compress( delta );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="frameId"></param>
		/// <param name="prevFrameId"></param>
		/// <param name="snapshot"></param>
		/// <returns></returns>
		public byte[] Decompress ( uint prevFrameId, byte[] snapshot )
		{
			if (prevFrameId==0) {
				return NetworkEngine.Decompress( snapshot );
			}

			var prevSnapshot	=	queue.LastOrDefault( s => s.Frame == prevFrameId );

			if (prevSnapshot==null) {
				Log.Warning("Missing snapshot #{0}. Waiting for full snapshot.", prevFrameId );
				return null;
			}


			var delta	=	NetworkEngine.Decompress( snapshot );
			var minSize	=	Math.Min( delta.Length, prevSnapshot.Data.Length );

			var newSnapshot	=	new byte[ delta.Length ];

			for (int i=0; i<minSize; i++ ) {
				newSnapshot[i] = (byte)(delta[i] ^ prevSnapshot.Data[i]);
			}

			if (delta.Length > prevSnapshot.Data.Length) {
				for (int i=minSize; i<delta.Length; i++) {
					newSnapshot[i] = delta[i];
				}
			}

			return newSnapshot;
		}
	}
}

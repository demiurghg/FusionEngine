using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Imaging;
using System.Threading;
using Fusion.Build.Mapping;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// 
	/// </summary>
	internal class VTStamper {


		const float StampTimeInterval		=	0.10f;
		const float StampJitterAmplitude	=	0.03f;


		class Stamp {
			public readonly VTTile		Tile;
			public readonly Rectangle	Rectangle;

			int		counter = 0;
			float	timer	= 0;


			/// <summary>
			/// 
			/// </summary>
			/// <param name="tile"></param>
			/// <param name="rect"></param>
			public Stamp ( VTTile tile, Rectangle rect ) 
			{
				this.Tile			=	tile;
				this.Rectangle		=	rect;
			}


			/// <summary>
			/// Update internal counters and sets GPU data if necessary
			/// </summary>
			/// <param name="physicalPage"></param>
			/// <param name="dt"></param>
			public void AdvanceStamping ( VTSystem vtSystem, float dt, float jitter )
			{
				timer -= dt;

				if (timer<=0) {
					vtSystem.PhysicalPages0.SetData( Rectangle, Tile.GetGpuData(0) );
					vtSystem.PhysicalPages1.SetData( Rectangle, Tile.GetGpuData(1) );
					vtSystem.PhysicalPages2.SetData( Rectangle, Tile.GetGpuData(2) );
					counter++;
					timer = StampTimeInterval + jitter;
				}
			}


			/// <summary>
			/// Indicates that given tile is fully imprinted to physical texture.
			/// </summary>
			public bool IsFullyStamped {
				get { return counter>=1; }
			}

		}


		Random rand = new Random();
		Dictionary<Rectangle,Stamp> stamps = new Dictionary<Rectangle,Stamp>();

		
		/// <summary>
		/// Add tile to stamp queue
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="rect"></param>
		public void Add ( VTTile tile, Rectangle rect )
		{
			stamps[ rect ] = new Stamp( tile, rect );
		}


		/// <summary>
		/// Sequntually imprints enqued tiles to physical texture.
		/// </summary>
		/// <param name="physicalPage"></param>
		public void Update ( VTSystem vtSystem, float dt )
		{
			foreach ( var stamp in stamps ) {

				float jitter = rand.NextFloat( -StampJitterAmplitude, StampJitterAmplitude );

				stamp.Value.AdvanceStamping( vtSystem, dt, jitter );

			}	

			stamps = stamps.Where( pair => !pair.Value.IsFullyStamped ).ToDictionary( pair => pair.Key, pair => pair.Value );
		}
		
	}
}

﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Imaging;
using System.Threading;


namespace Fusion.Engine.Graphics {

	/// <summary>
	/// 
	/// </summary>
	internal class VTStamper {

		class Stamp {
			public readonly VTTile		Tile;
			public readonly Rectangle	Rectangle;
			public int StampCount;


			public Stamp ( VTTile tile, Rectangle rect ) 
			{
				this.Tile		=	tile;
				this.Rectangle	=	rect;
				this.StampCount	=	0;
			}
		}


		List<Stamp> stamps = new List<Stamp>();

		
		/// <summary>
		/// Add tile to stamp queue
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="rect"></param>
		public void Add ( VTTile tile, Rectangle rect )
		{
			stamps.Add( new Stamp( tile, rect ) );
		}


		/// <summary>
		/// Sequntually imprints enqued tiles to physical texture.
		/// </summary>
		/// <param name="physicalPage"></param>
		public void Update ( Texture2D physicalPage )
		{
			foreach ( var stamp in stamps ) {
				stamp.StampCount++;
				physicalPage.SetData( 0, stamp.Rectangle, stamp.Tile.Data, 0, stamp.Tile.Data.Length );
			}	

			stamps.RemoveAll( s => s.StampCount > 0 );
		}
		
	}
}
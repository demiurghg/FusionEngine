using System;
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

namespace Fusion.Engine.Graphics {
	internal class VirtualTexture : GameComponent {

		readonly RenderSystem rs;

		[Config]
		public int MaxPPF { get; set; }

		public UserTexture	FallbackTexture;
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="baseDirectory"></param>
		public VirtualTexture ( RenderSystem rs ) : base( rs.Game )
		{
			this.rs	=	rs;

			MaxPPF	=	16;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="baseDir"></param>
		public void Start ( string baseDir )
		{
			var fallbackPath	=	Path.Combine( baseDir, "fallback.tga" );
			FallbackTexture		=	UserTexture.CreateFromTga( rs, File.OpenRead(fallbackPath), true );	
		}



		/// <summary>
		/// 
		/// </summary>
		public void Stop ( bool wait )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void Update ( FeedbackData[] data )
		{
		}

	}
}

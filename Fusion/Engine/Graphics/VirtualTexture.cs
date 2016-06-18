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


		[Config]
		public bool ShowPageRequest { get; set; }

		[Config]
		public bool ShowPageLoads { get; set; }


		public UserTexture		FallbackTexture;
		

		public RenderTarget2D	PhysicalPages;
		public Texture2D		PageTable;

		HashSet<VTAddress>		cache;

		VTTileLoader			tileLoader;

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
			int size		=	VTConfig.PhysicalPageCount * VTConfig.PageSize;
			PhysicalPages	=	new RenderTarget2D( rs.Device, ColorFormat.Rgba8_sRGB, size, size, false );

			//PageTable		=	new Texture2D( rs.Device, 
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				if (tileLoader!=null) {
					tileLoader.Stop();
				}

				SafeDispose( ref PhysicalPages );
				SafeDispose( ref PageTable );

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
			tileLoader			=	new VTTileLoader( this, baseDir );
		}



		/// <summary>
		/// 
		/// </summary>
		public void Stop ()
		{
			tileLoader.Stop();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public void Update ( VTAddress[] data )
		{
			var uniquePages = data.Distinct().Where( p => p.Dummy!=0 ).ToArray();

			List<VTAddress> feedbackTree = new List<VTAddress>();

			//	
			//	build tree :
			//
			foreach ( var page in uniquePages ) {

				var ppage = page;

				feedbackTree.Add( ppage );

				while (ppage.MipLevel < VTConfig.MaxMipLevel) {
					ppage = VTAddress.FromChild( ppage );
					feedbackTree.Add( ppage );
				}

			}

			var pageRequest = feedbackTree
			//	.Where( p0 => cache.Contains(p0) )
				.Distinct()
				.OrderBy( p1 => p1.MipLevel )
				.ToArray();//*/




			if (ShowPageRequest) {
				Log.Message("VT page requests: ");
				foreach ( var p in pageRequest ) {
					Log.Message(" - {0}", p.ToString());
				}
			}
		}
	}
}

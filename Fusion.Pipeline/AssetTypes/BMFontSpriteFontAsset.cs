﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Graphics;
using Fusion.Mathematics;
using Fusion.Content;


namespace Fusion.Pipeline.AssetTypes {
	
	[Asset("Content", "BMFC Sprite Font", "*.bmfc")]
	public class BMFontSpriteFontAsset : Asset {

		public string SourcePath { get; set; }

		public override string[] Dependencies
		{
			get { return new[]{ SourcePath }; }
		}



		public BMFontSpriteFontAsset ( string path ) : base(path)
		{
		}



		/// <summary>
		/// Builds asset
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Build ( BuildContext buildContext )
		{														   
			string tempFileName		= buildContext.GetTempFileName( AssetPath, ".fnt" );
			string tempFileNameR	= buildContext.GetTempFileName( AssetPath, ".fnt");
			string resolvedPath		= buildContext.Resolve( SourcePath );	

			//	Launch 'bmfont.com' with temporary output file :
			buildContext.RunTool( @"bmfont.com",  string.Format("-c \"{0}\" -o \"{1}\"", resolvedPath, tempFileNameR ) );


			//	load temporary output :
			SpriteFont.FontFile font;
			using ( var stream = File.OpenRead( tempFileNameR ) ) {
				font = SpriteFont.FontLoader.Load( stream );
			}


			//	perform some checks :
			if (font.Common.Pages!=1) {
				throw new ContentException("Only one page of font image is supported");
			}


			//	patch font description and add children (e.g. "secondary") content :
			foreach (var p in font.Pages) {

				var newAssetPath	=	Path.Combine( AssetPath, "Page#" + p.ID.ToString() );
				var newSrcPath		=	Path.Combine( Path.GetDirectoryName(tempFileName), p.File );

				if ( Path.GetExtension( newSrcPath ).ToLower() == ".dds" ) {

					var asset			=	buildContext.AddAsset<RawFileAsset>( newAssetPath );
					asset.SourceFile	=	newSrcPath;

				} else {

					var asset			=	buildContext.AddAsset<ImageFileTextureAsset>( newAssetPath );
					asset.SourceFile	=	newSrcPath;
				}

				p.File	=	newAssetPath;
			}

			using ( var stream = buildContext.OpenTargetStream( this ) ) {
				SpriteFont.FontLoader.Save( stream, font );
			}
		}
	} 
}

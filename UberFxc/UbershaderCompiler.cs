﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using SharpDX;
using Fusion.Graphics;
using System.Threading;
using Fusion.Mathematics;
using FX = SharpDX.D3DCompiler;
using SharpDX.D3DCompiler;


namespace UberFxc {

	public partial class UbershaderCompiler {

		class UsdbEntry {

			public string Defines;
			public byte[] PSBytecode;
			public byte[] VSBytecode;
			public byte[] GSBytecode;
			public byte[] HSBytecode;
			public byte[] DSBytecode;
			public byte[] CSBytecode;

			public UsdbEntry ( string defines, byte[] ps, byte[] vs, byte[] gs, byte[] hs, byte[] ds, byte[] cs ) 
			{
				this.Defines	=	defines;
				this.PSBytecode	=	ps;
				this.VSBytecode	=	vs;
				this.GSBytecode	=	gs;
				this.HSBytecode	=	hs;
				this.DSBytecode	=	ds;
				this.CSBytecode	=	cs;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public void Build ( Parameters param )
		{
			//
			//	Get combinations :
			//
			var combDecl	=	param.DefineEnum;

			var defineList = new List<string>();

			foreach ( var comb in combDecl ) {
				var ue = new UbershaderEnumerator( comb.Trim(), "" );
				defineList.AddRange( ue.DefineList );
			}


			//
			//	Start listing builder :
			//	
			ListingPath	=	buildContext.GetTempFileName( AssetPath, ".html", true );
			var htmlBuilder = new StringBuilder();

			htmlBuilder.AppendFormat("<pre>");
			htmlBuilder.AppendLine("<b>Ubershader assembly listing</b>");
			htmlBuilder.AppendLine("");
			htmlBuilder.AppendLine("<b>Source:</b> <i>" + AssetPath + "</i>" );
			htmlBuilder.AppendLine("");
			htmlBuilder.AppendLine("<b>Declarations:</b>");

			foreach ( var comb in combDecl ) {
				htmlBuilder.AppendLine("  <i>" + comb + "</i>");
			}
			htmlBuilder.AppendLine("");


			var usdb = new List<UsdbEntry>();

			//
			//	Build all :
			//
			foreach ( var defines in defineList ) {

				var id		=	defineList.IndexOf( defines );

				var psbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".PS.dxbc", true );
				var vsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".VS.dxbc", true );
				var gsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".GS.dxbc", true );
				var hsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".HS.dxbc", true );
				var dsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".DS.dxbc", true );
				var csbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".CS.dxbc", true );

				var pshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".PS.html", true );
				var vshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".VS.html", true );
				var gshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".GS.html", true );
				var hshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".HS.html", true );
				var dshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".DS.html", true );
				var cshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".CS.html", true );

				var ps = Compile( buildContext, SourceFile, "ps_5_0", PSEntryPoint, defines, psbc, pshtm );
				var vs = Compile( buildContext, SourceFile, "vs_5_0", VSEntryPoint, defines, vsbc, vshtm );
				var gs = Compile( buildContext, SourceFile, "gs_5_0", GSEntryPoint, defines, gsbc, gshtm );
				var hs = Compile( buildContext, SourceFile, "hs_5_0", HSEntryPoint, defines, hsbc, hshtm );
				var ds = Compile( buildContext, SourceFile, "ds_5_0", DSEntryPoint, defines, dsbc, dshtm );
				var cs = Compile( buildContext, SourceFile, "cs_5_0", CSEntryPoint, defines, csbc, cshtm );
				

				htmlBuilder.AppendFormat( (vs.Length==0) ? ".. " : "<a href=\"{0}\">vs</a> ", Path.GetFileName(vshtm) );
				htmlBuilder.AppendFormat( (ps.Length==0) ? ".. " : "<a href=\"{0}\">ps</a> ", Path.GetFileName(pshtm) );
				htmlBuilder.AppendFormat( (hs.Length==0) ? ".. " : "<a href=\"{0}\">hs</a> ", Path.GetFileName(hshtm) );
				htmlBuilder.AppendFormat( (ds.Length==0) ? ".. " : "<a href=\"{0}\">ds</a> ", Path.GetFileName(dshtm) );
				htmlBuilder.AppendFormat( (gs.Length==0) ? ".. " : "<a href=\"{0}\">gs</a> ", Path.GetFileName(gshtm) );
				htmlBuilder.AppendFormat( (cs.Length==0) ? ".. " : "<a href=\"{0}\">cs</a> ", Path.GetFileName(cshtm) );

				htmlBuilder.Append( "[" + defines + "]<br>" );

				usdb.Add( new UsdbEntry( defines, ps, vs, gs, hs, ds, cs ) );
			}

			File.WriteAllText( buildContext.GetTempFileName(AssetPath, ".html", true), htmlBuilder.ToString() );


			//
			//	Write ubershader :
			//
			using ( var fs = buildContext.TargetStream( this ) ) {

				using ( var bw = new BinaryWriter( fs ) ) {

					bw.Write( new[]{'U','S','D','B'});

					bw.Write( usdb.Count );

					foreach ( var entry in usdb ) {

						bw.Write( entry.Defines );

						bw.Write( new[]{'P','S','B','C'});
						bw.Write( entry.PSBytecode.Length );
						bw.Write( entry.PSBytecode );

						bw.Write( new[]{'V','S','B','C'});
						bw.Write( entry.VSBytecode.Length );
						bw.Write( entry.VSBytecode );

						bw.Write( new[]{'G','S','B','C'});
						bw.Write( entry.GSBytecode.Length );
						bw.Write( entry.GSBytecode );

						bw.Write( new[]{'H','S','B','C'});
						bw.Write( entry.HSBytecode.Length );
						bw.Write( entry.HSBytecode );

						bw.Write( new[]{'D','S','B','C'});
						bw.Write( entry.DSBytecode.Length );
						bw.Write( entry.DSBytecode );

						bw.Write( new[]{'C','S','B','C'});
						bw.Write( entry.CSBytecode.Length );
						bw.Write( entry.CSBytecode );
					}
				}
			}
		}


		class IncludeHandler : FX.Include {

			readonly Parameters buildContext;
		
			public IncludeHandler ( Parameters buildContext )
			{
				this.buildContext	=	buildContext;
			}


			public Stream Open( IncludeType type, string fileName, Stream parentStream )
			{
				//Log.Information("{0} {1} {2}", type, fileName, parentStream );

				return File.OpenRead( buildContext.Resolve( fileName ) );
			}


			public void Close( Stream stream )
			{
				stream.Close();
			}


			IDisposable ICallbackable.Shadow {
				get; set;
			}


			public void Dispose ()
			{
				
			}
				
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		/// <param name="sourceFile"></param>
		/// <param name="profile"></param>
		/// <param name="entryPoint"></param>
		/// <param name="defines"></param>
		/// <param name="output"></param>
		/// <param name="listing"></param>
		/// <returns></returns>
		byte[] Compile ( Parameters buildContext, string sourceFile, string profile, string entryPoint, string defines, string output, string listing )
		{
			//Log.Debug("{0} {1} {2} {3}", sourceFile, profile, entryPoint, defines );

			var	flags	=	FX.ShaderFlags.None;

			if ( DisableOptimization)	flags |= FX.ShaderFlags.OptimizationLevel0;
			// (!DisableOptimization)	flags |= FX.ShaderFlags.OptimizationLevel3;
			if ( PreferFlowControl)		flags |= FX.ShaderFlags.PreferFlowControl;
			if ( AvoidFlowControl)		flags |= FX.ShaderFlags.AvoidFlowControl;

			if ( MatrixPacking==ShaderMatrixPacking.ColumnMajor )	flags |= FX.ShaderFlags.PackMatrixColumnMajor;
			if ( MatrixPacking==ShaderMatrixPacking.RowMajor )		flags |= FX.ShaderFlags.PackMatrixRowMajor;

			var defs = defines.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries)	
						.Select( entry => new SharpDX.Direct3D.ShaderMacro( entry, "1" ) )
						.ToArray();

			sourceFile	=	buildContext.Resolve( sourceFile );
					
			try {
			
				var result = FX.ShaderBytecode.CompileFromFile( sourceFile, entryPoint, profile, flags, FX.EffectFlags.None, defs, new IncludeHandler(buildContext) );
			
				if ( result.Message!=null ) {
					Log.Warning( result.Message );
				}

				File.WriteAllText( listing, result.Bytecode.Disassemble( FX.DisassemblyFlags.EnableColorCode, "" ) );

				return result.Bytecode.Data;

			} catch ( Exception ex ) {

				if (ex.Message.Contains("error X3501")) {
					Log.Debug("No entry point '{0}'. It's ok", entryPoint );
					return new byte[0];
				}

				throw;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <param name="profile"></param>
		/// <param name="entryPoint"></param>
		/// <param name="defines"></param>
		/// <returns></returns>
		byte[] RunFxc ( Parameters param, string sourceFile, string profile, string entryPoint, string defines, string output, string listing )
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("/Cc" + " ");
			sb.Append("/T" + profile + " ");
			sb.Append("/E" + entryPoint + " ");
			sb.Append("/Fo\"" + output + "\" ");
			sb.Append("/Fc\"" + listing + "\" ");
			sb.Append("/nologo" + " ");
			sb.Append("/O" + OptimizationLevel.ToString() + " ");

			if ( DisableOptimization)	sb.Append("/Od ");
			if ( PreferFlowControl)	sb.Append("/Gfp ");
			if ( AvoidFlowControl)	sb.Append("/Gfa ");

			if ( MatrixPacking==ShaderMatrixPacking.ColumnMajor )	sb.Append("/Zpc ");
			if ( MatrixPacking==ShaderMatrixPacking.RowMajor )	sb.Append("/Zpr ");

			foreach ( var def in defines.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries) ) {
				sb.AppendFormat("/D{0}=1 ", def);
			}

			sb.Append("\"" + buildContext.Resolve( sourceFile ) + "\"");

			try {
				
				buildContext.RunTool("fxc_1.exe", sb.ToString());

			} catch ( ToolException tx ) {
				///	entry point not fount - that is ok.
				if (tx.Message.Contains("error X3501")) {
					Log.Debug("No entry point '{0}'. That is ok.", entryPoint );
					return new byte[0];
				}

				throw;
			}

			return File.ReadAllBytes( output );
		}
	}
}

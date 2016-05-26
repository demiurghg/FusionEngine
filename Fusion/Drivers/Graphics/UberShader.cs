using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using System.Text.RegularExpressions;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {
	
	internal partial class Ubershader : GraphicsResource {

		public const string UbershaderSignature = "USH1";
		public const string PSBytecodeSignature = "PSBC";
		public const string VSBytecodeSignature = "VSBC";
		public const string GSBytecodeSignature = "GSBC";
		public const string HSBytecodeSignature = "HSBC";
		public const string DSBytecodeSignature = "DSBC";
		public const string CSBytecodeSignature = "CSBC";

		class UsdbEntry {

			public string Defines;

			public ShaderBytecode PixelShader;
			public ShaderBytecode VertexShader;
			public ShaderBytecode GeometryShader;
			public ShaderBytecode HullShader;
			public ShaderBytecode DomainShader;
			public ShaderBytecode ComputeShader;


			public UsdbEntry ( string defines, byte[] ps, byte[] vs, byte[] gs, byte[] hs, byte[] ds, byte[] cs ) 
			{
				this.Defines		=	defines;
				this.PixelShader	=	NullOrShaderBytecode( ps );
				this.VertexShader	=	NullOrShaderBytecode( vs );
				this.GeometryShader	=	NullOrShaderBytecode( gs );
				this.HullShader		=	NullOrShaderBytecode( hs );
				this.DomainShader	=	NullOrShaderBytecode( ds );
				this.ComputeShader	=	NullOrShaderBytecode( cs );
			}


			ShaderBytecode NullOrShaderBytecode ( byte[] array )
			{
				if (array.Length==0) {
					return null;
				}
				return new ShaderBytecode( array );
			}
		}


		List<StateFactory> factories = new List<StateFactory>();
		Dictionary<string,UsdbEntry> database = new Dictionary<string,UsdbEntry>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="path"></param>
		/// <param name="combinerEnum"></param>
		public Ubershader ( GraphicsDevice device, Stream stream ) : base(device)
		{
			database.Clear();

			using ( var br = new BinaryReader( stream ) ) {

				var foucCC = br.ReadFourCC();

				if (foucCC!=UbershaderSignature) {
					throw new IOException("Bad ubershader signature");
				}


				var count = br.ReadInt32();

				for (int i=0; i<count; i++) {
					var defines		=	br.ReadString();
					int length;

					br.ExpectFourCC("PSBC", "ubershader");
					length	=	br.ReadInt32();
					var ps	=	br.ReadBytes( length );

					br.ExpectFourCC("VSBC", "ubershader");
					length	=	br.ReadInt32();
					var vs	=	br.ReadBytes( length );

					br.ExpectFourCC("GSBC", "ubershader");
					length	=	br.ReadInt32();
					var gs	=	br.ReadBytes( length );

					br.ExpectFourCC("HSBC", "ubershader");
					length	=	br.ReadInt32();
					var hs	=	br.ReadBytes( length );

					br.ExpectFourCC("DSBC", "ubershader");
					length	=	br.ReadInt32();
					var ds	=	br.ReadBytes( length );

					br.ExpectFourCC("CSBC", "ubershader");
					length	=	br.ReadInt32();
					var cs	=	br.ReadBytes( length );

					//Log.Message("{0}", profile );
					//PrintSignature( bytecode, "ISGN" );
					//PrintSignature( bytecode, "OSGN" );
					//PrintSignature( bytecode, "OSG5" );
					if (database.ContainsKey(defines)) {
						Log.Warning("Duplicate definitions: {0}", defines );
						continue;
					}

					database.Add( defines, new UsdbEntry( defines, ps, vs, gs, hs, ds, cs ) );
				}
			}

			Log.Debug("Ubershader: {0} shaders", database.Count );
		}



		/// <summary>
		/// Creates pipeline state factory
		/// </summary>
		/// <param name="type"></param>
		/// <param name="enumerator"></param>
		/// <returns></returns>
		public StateFactory CreateFactory ( Type type, Action<PipelineState,int> enumerator )
		{
			lock (factories) {
				var factory = new StateFactory( this, type, enumerator );
				factories.Add(factory);
				return factory;
			}
		}



		/// <summary>
		/// Creates pipeline state factory
		/// </summary>
		/// <param name="type"></param>
		/// <param name="primitive"></param>
		/// <param name="vertexInputElements"></param>
		/// <returns></returns>
		public StateFactory CreateFactory ( Type type, Primitive primitive, VertexInputElement[] vertexInputElements )
		{
			return CreateFactory( type, (ps,i) => { 
					ps.VertexInputElements = vertexInputElements; ps.Primitive = primitive; 
				});
		}



		/// <summary>
		/// Creates pipeline state factory
		/// </summary>
		public StateFactory CreateFactory ( Type type, Primitive primitive, VertexInputElement[] vertexInputElements, BlendState blendState, RasterizerState rasterizerState )
		{
			return CreateFactory( type, (ps,i) => { 
					ps.Primitive = primitive;
					ps.VertexInputElements = vertexInputElements; 
					ps.BlendState		=	blendState;
					ps.RasterizerState	=	rasterizerState;
				});
		}



		/// <summary>
		/// Creates pipeline state factory
		/// </summary>
		public StateFactory CreateFactory ( Type type, Primitive primitive, VertexInputElement[] vertexInputElements, BlendState blendState, RasterizerState rasterizerState, DepthStencilState depthStencilState )
		{
			return CreateFactory( type, (ps,i) => { 
					ps.Primitive = primitive;
					ps.VertexInputElements	=	vertexInputElements; 
					ps.BlendState			=	blendState;
					ps.RasterizerState		=	rasterizerState;
					ps.DepthStencilState	=	depthStencilState;
				});
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				foreach ( var obj in factories ) {
					if (obj!=null) {
						obj.Dispose();
					}
				}
				factories.Clear();
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Gets all defines
		/// </summary>
		public ICollection<string>	Defines {
			get {
				return database.Select( dbe => dbe.Key ).ToArray();
			}
		}



		/// <summary>
		/// Gets PixelShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetPixelShader( string key = "" )
		{
			return ( database[key].PixelShader );
		}



		/// <summary>
		/// Gets VertexShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetVertexShader( string key = "" )
		{
			return ( database[key].VertexShader );
		}



		/// <summary>
		/// Gets GeometryShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetGeometryShader( string key = "" )
		{
			return ( database[key].GeometryShader );
		}



		/// <summary>
		/// Gets HullShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetHullShader( string key = "" )
		{
			return ( database[key].HullShader );
		}



		/// <summary>
		/// Gets DomainShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetDomainShader( string key = "" )
		{
			return ( database[key].DomainShader );
		}



		/// <summary>
		/// Gets ComputeShader
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ShaderBytecode GetComputeShader( string key = "" )
		{
			return ( database[key].ComputeShader );
		}
	}
}

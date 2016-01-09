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


namespace Fusion.Drivers.Graphics {
	

	public partial class Ubershader : GraphicsResource {

		public const string UbershaderSignature = "USH1";
		public const string PipelineSignature	= "PIPE";
		public const string PSBytecodeSignature = "PSBC";
		public const string VSBytecodeSignature = "VSBC";
		public const string GSBytecodeSignature = "GSBC";
		public const string HSBytecodeSignature = "HSBC";
		public const string DSBytecodeSignature = "DSBC";
		public const string CSBytecodeSignature = "CSBC";

		class UsdbEntry {

			public string Defines;

			public string BlendState;
			public string RasterizerState;
			public string DepthStencilState;
			public string PrimitiveType;
			public string VertexInputElements;
			public string VertexOutputElements;
			public string RasterizedStream;

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



		/// <summary>
		/// Ubershader's texture parameters
		/// </summary>
		public class TextureParameter {
			
			static Regex	Pattern	=	new Regex(@"\$texture\s+(\w+)\s+([a-zA-Z\.\\/]+)(?:\s*\/\/\s*(.*))?");

			public string Name  { get; private set; }
			public string Default  { get; private set; }
			public string Comment  { get; private set; }

			public TextureParameter ( string declaration )
			{
				declaration		=	declaration.Trim();

				var match		=	Pattern.Match( declaration );
				var captures	=	match.Groups.Cast<Group>().Select( c => c.Value ).ToArray();

				this.Name		=	captures[1];
				this.Default	=	captures[2];
				this.Comment	=	captures.Length > 3 ? captures[3] : "";
			}

			public TextureParameter ( string name, string defValue, string comment )
			{
				this.Name		=	name;
				this.Default	=	defValue;
				this.Comment	=	comment;
			}
		}



		/// <summary>
		/// Ubershader's texture parameters
		/// </summary>
		public class UniformParameter {
													
			static Regex	Pattern	=	new Regex(@"\$uniform\s+(\w+)\s+(\-?\d(?:\.\d+)?)(?:\s*\/\/\s*(.*))?");

			public string Name  { get; private set; }
			public float Default  { get; private set; }
			public string Comment  { get; private set; }

			public UniformParameter ( string declaration )
			{
				declaration	=	declaration.Trim();

				var match		=	Pattern.Match( declaration );
				var captures	=	match.Groups.Cast<Group>().Select( c => c.Value ).ToArray();

				this.Name		=	captures[1];
				this.Default	=	float.Parse(captures[2]);
				this.Comment	=	captures.Length > 3 ? captures[3] : "";
			}

			public UniformParameter ( string name, float defValue, string comment )
			{
				this.Name		=	name;
				this.Default	=	defValue;
				this.Comment	=	comment;
			}
		}



		Dictionary<string,UsdbEntry>	database = new Dictionary<string,UsdbEntry>();


		/// <summary>
		/// Shader tag. From $tag 'TagName'.
		/// </summary>
		public string ShaderTag { get; private set; }

		/// <summary>
		/// Gets texture parameters
		/// </summary>
		public TextureParameter[]	Textures { get; private set; }

		/// <summary>
		/// Gets texture parameters
		/// </summary>
		public UniformParameter[]	Uniforms { get; private set; }


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="path"></param>
		/// <param name="combinerEnum"></param>
		public Ubershader ( GraphicsDevice device, Stream stream ) : base(device)
		{
			Recreate( stream );
		}



		/// <summary>
		/// Resets all data and recreates UberShader
		/// </summary>
		/// <param name="path"></param>
		void Recreate ( Stream stream )
		{
			database.Clear();

			using ( var br = new BinaryReader( stream ) ) {

				var foucCC = br.ReadFourCC();

				if (foucCC!=UbershaderSignature) {
					throw new IOException("Bad ubershader signature");
				}

				this.Textures	=	new TextureParameter[ br.ReadInt32() ];
				for (int i=0; i<Textures.Length; i++) {
					Textures[i] = new TextureParameter( br.ReadString(), br.ReadString(), br.ReadString() );
				}

				this.Uniforms	=	new UniformParameter[ br.ReadInt32() ];
				for (int i=0; i<Uniforms.Length; i++) {
					Uniforms[i]	=	new UniformParameter( br.ReadString(), br.ReadSingle(), br.ReadString() );
				}



				var count = br.ReadInt32();

				for (int i=0; i<count; i++) {
					var defines		=	br.ReadString();
					int length;

					/*br.ExpectFourCC(PipelineSignature);
					length	=	br.ReadInt32();
					for (int i=0; i<length; i++) {
						var key = br.ReadString();
						var value = br.ReadString();
					} */

					br.ExpectFourCC("PSBC");
					length	=	br.ReadInt32();
					var ps	=	br.ReadBytes( length );

					br.ExpectFourCC("VSBC");
					length	=	br.ReadInt32();
					var vs	=	br.ReadBytes( length );

					br.ExpectFourCC("GSBC");
					length	=	br.ReadInt32();
					var gs	=	br.ReadBytes( length );

					br.ExpectFourCC("HSBC");
					length	=	br.ReadInt32();
					var hs	=	br.ReadBytes( length );

					br.ExpectFourCC("DSBC");
					length	=	br.ReadInt32();
					var ds	=	br.ReadBytes( length );

					br.ExpectFourCC("CSBC");
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

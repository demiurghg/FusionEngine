using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	/// <summary>
	///	Represents texture that could be updated by CPU (video frame of dynamic texture)
	/// </summary>
	public class DynamicTexture : Texture{

		/// <summary>
		/// Gets texture's element type
		/// </summary>
		public Type ElementType {
			get; private set;
		}
		

		Texture2D texture;


		static Tuple<Type,ColorFormat>[] formats = 
			new[] {
				new Tuple<Type,ColorFormat>( typeof(ColorBGRA),	ColorFormat.Bgra8 ),
				new Tuple<Type,ColorFormat>( typeof(Color),		ColorFormat.Rgba8 ),
				new Tuple<Type,ColorFormat>( typeof(Color4),	ColorFormat.Rgba32F ),
				new Tuple<Type,ColorFormat>( typeof(Vector4),	ColorFormat.Rgba32F ),
				new Tuple<Type,ColorFormat>( typeof(Half2),		ColorFormat.Rg16F ),
				new Tuple<Type,ColorFormat>( typeof(Half4),		ColorFormat.Rgba16F ),
				new Tuple<Type,ColorFormat>( typeof(float),		ColorFormat.R32F ),
			};


		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public DynamicTexture ( RenderSystem rs, int width, int height, Type elementType, bool mips = false, bool srgb = false )
		{
			Log.Message("DynamicTexture: {0}x{1}@{2} {3} {4}", width, height, elementType.Name, mips, srgb );
			this.ElementType	=	elementType;
			this.Width			=	width;
			this.Height			=	height;

			var format			=	formats.FirstOrDefault( f => f.Item1==elementType ).Item2;

			if (format == ColorFormat.Unknown) {
				throw new ArgumentException("elementType must be " + string.Join(", ", formats.Select(f=>f.Item2.ToString())) );
			}

			texture			=	new Texture2D( rs.Device, width, height, format, mips, srgb );
			Srv				=	texture;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref texture );
			}

			base.Dispose( disposing );
		}


		
		/// <summary>
		/// Set's texture data
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="level"></param>
		/// <param name="rect"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void SetData<T> ( int level, Rectangle? rect, T[] data, int startIndex, int elementCount ) where T: struct
		{
			texture.SetData<T>( level, rect, data, startIndex, elementCount );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void SetData<T> ( T[] data, int startIndex, int elementCount ) where T: struct
		{
			texture.SetData<T>( data, startIndex, elementCount );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void SetData<T> ( T[] data ) where T: struct
		{
			texture.SetData<T>( data );
		}
	}
}

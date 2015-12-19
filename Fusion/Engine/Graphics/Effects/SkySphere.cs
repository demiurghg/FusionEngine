using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Creates sky sphere
	/// </summary>
	static class SkySphere {

		/// <summary>
		/// Gets sphere vertices.
		/// </summary>
		/// <param name="interations"></param>
		/// <returns></returns>
		public static Vector4[] GetVertices ( int iterations = 4 )
		{
			var shape = CreateOctahedron();
			
			for ( int i=0; i<iterations; i++ ) {
				shape = Refine( shape );
			}

			var result = new Vector4[ shape.Length * 3 ];

			for ( int i=0; i<shape.Length; i++ ) {
				result[ i*3+0 ] = new Vector4( shape[i].A, 1 );
				result[ i*3+1 ] = new Vector4( shape[i].B, 1 );
				result[ i*3+2 ] = new Vector4( shape[i].C, 1 );
			}

			return result;
		}



		struct Triangle {
			public Vector3 A;
			public Vector3 B;
			public Vector3 C;

			public Triangle ( Vector3 a, Vector3 b, Vector3 c )
			{
				A = a; B = b; C = c;
			}
		}



		static Triangle[] Refine ( Triangle[] initialShape )
		{
			List<Triangle> shape = new List<Triangle>();

			foreach ( var tri in initialShape ) {

				var a = tri.A;
				var b = tri.B;
				var c = tri.C;
				var d = Vector3.Lerp( a, b, 0.5f ).Normalized();
				var e = Vector3.Lerp( a, c, 0.5f ).Normalized();
				var f = Vector3.Lerp( c, b, 0.5f ).Normalized();

				shape.Add( new Triangle( a, d, e ) );
				shape.Add( new Triangle( d, b, f ) );
				shape.Add( new Triangle( e, f, c ) );
				shape.Add( new Triangle( e, d, f ) );
			}			

			return shape.ToArray();
		}



		static Triangle[] CreateOctahedron ()
		{
			List<Triangle> shape = new List<Triangle>();

			shape.Add( new Triangle(  Vector3.UnitX,  Vector3.UnitY,  Vector3.UnitZ ) );
			shape.Add( new Triangle(  Vector3.UnitZ,  Vector3.UnitY, -Vector3.UnitX ) );
			shape.Add( new Triangle( -Vector3.UnitX,  Vector3.UnitY, -Vector3.UnitZ ) );
			shape.Add( new Triangle( -Vector3.UnitZ,  Vector3.UnitY,  Vector3.UnitX ) );

			shape.Add( new Triangle(  Vector3.UnitZ, -Vector3.UnitY,  Vector3.UnitX ) );
			shape.Add( new Triangle( -Vector3.UnitX, -Vector3.UnitY,  Vector3.UnitZ ) );
			shape.Add( new Triangle( -Vector3.UnitZ, -Vector3.UnitY, -Vector3.UnitX ) );
			shape.Add( new Triangle(  Vector3.UnitX, -Vector3.UnitY, -Vector3.UnitZ ) );

			return shape.ToArray();
		}




	}
}

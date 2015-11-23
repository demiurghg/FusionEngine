using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using System.Globalization;

namespace Fusion.Core {
	/// <summary>
	/// String-to-value and value-to-string converter.
	/// </summary>
	public static class StringConverter {

		public static string ToString( object value )
		{
			var type = value.GetType();

			if (type.IsEnum)			return ToString( (Enum)   value );
			if (type==typeof( Int16		) )	return ToString( ( Int16	)value );
			if (type==typeof( Int32		) )	return ToString( ( Int32	)value );
			if (type==typeof( Int64		) )	return ToString( ( Int64	)value );
			if (type==typeof( Single	) )	return ToString( ( Single	)value );
			if (type==typeof( Double	) )	return ToString( ( Double	)value );
			if (type==typeof( Color		) )	return ToString( ( Color	)value );
			if (type==typeof( Color4	) )	return ToString( ( Color4	)value );
			if (type==typeof( Double	) )	return ToString( ( Double	)value );
			if (type==typeof( Vector2	) )	return ToString( ( Vector2	)value );
			if (type==typeof( Vector3	) )	return ToString( ( Vector3	)value );
			if (type==typeof( Vector4	) )	return ToString( ( Vector4	)value );
			if (type==typeof( Half		) )	return ToString( ( Half		)value );
			if (type==typeof( Half2		) )	return ToString( ( Half2	)value );
			if (type==typeof( Half3		) )	return ToString( ( Half3	)value );
			if (type==typeof( Half4		) )	return ToString( ( Half4	)value );

			throw new ArgumentException(string.Format("Can not convert {0} to string", type));
		}


		public static object FromString ( Type type, string value )
		{
			if (type.IsEnum)			return Enum.Parse( type, value, true );
			if (type==typeof(Int16)  )	return ToInt16	( value );
			if (type==typeof(Int32)  )	return ToInt32	( value );
			if (type==typeof(Int64)  )	return ToInt64	( value );
			if (type==typeof(Single) )	return ToSingle	( value );
			if (type==typeof(Double) )	return ToDouble	( value );
			if (type==typeof(Color)  )	return ToColor	( value );
			if (type==typeof(Color4) )	return ToColor4	( value );
			if (type==typeof(Double) )	return ToDouble	( value );
			if (type==typeof(Vector2))	return ToVector2( value );
			if (type==typeof(Vector3))	return ToVector3( value );
			if (type==typeof(Vector4))	return ToVector4( value );
			if (type==typeof(Half)   )	return ToHalf	( value );
			if (type==typeof(Half2)  )	return ToHalf2	( value );
			if (type==typeof(Half3)  )	return ToHalf3	( value );
			if (type==typeof(Half4)  )	return ToHalf4	( value );

			throw new ArgumentException(string.Format("{0} is not valid value for {1}", value, type));
		}


		public static T FromString<T>( string value )
		{
			return (T)FromString( typeof(T), value );
		}


		public static string ToString( Byte value )
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString( Int16 value )
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString( Int32 value )
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString( Int64 value )
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString( Single value )
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString( Double value )
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString ( Enum value )
		{
			return Enum.GetName( value.GetType(), value );
		}

		public static string ToString ( Color value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.R, value.G, value.B, value.A );
		}

		public static string ToString ( Color4 value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.Red, value.Green, value.Blue, value.Alpha );
		}

		public static string ToString ( Vector4 value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.X, value.Y, value.Z, value.W );
		}

		public static string ToString ( Vector3 value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", value.X, value.Y, value.Z );
		}

		public static string ToString ( Vector2 value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", value.X, value.Y );
		}

		public static string ToString ( Half value )
		{
			return value.ToString();
		}

		public static string ToString ( Half2 value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", value.X, value.Y );
		}

		public static string ToString ( Half3 value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", value.X, value.Y, value.Z );
		}

		public static string ToString ( Half4 value )
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.X, value.Y, value.Z, value.W );
		}

		

		public static Byte ToByte ( string value )
		{
			return Byte.Parse( value, CultureInfo.InvariantCulture );
		}

		public static Int16 ToInt16 ( string value )
		{
			return Int16.Parse( value, CultureInfo.InvariantCulture );
		}

		public static Int32 ToInt32 ( string value )
		{
			return Int32.Parse( value, CultureInfo.InvariantCulture );
		}

		public static Int64 ToInt64 ( string value )
		{
			return Int64.Parse( value, CultureInfo.InvariantCulture );
		}

		public static Half ToHalf ( string value )
		{
			return new Half( float.Parse(value, CultureInfo.InvariantCulture) );
		}

		public static Single ToSingle ( string value )
		{
			return Single.Parse( value, CultureInfo.InvariantCulture );
		}

		public static Double ToDouble ( string value )
		{
			return Double.Parse( value, CultureInfo.InvariantCulture );
		}

		public static Color ToColor ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Color");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Color( byte.Parse(args[0], cult), byte.Parse(args[1], cult), byte.Parse(args[2], cult), byte.Parse(args[3], cult) );
		}

		public static Color4 ToColor4 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Color4");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Color4( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult), float.Parse(args[3], cult) );
		}

		public static Vector4 ToVector4 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Vector4");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Vector4( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult), float.Parse(args[3], cult) );
		}

		public static Vector3 ToVector3 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Vector3");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Vector3( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult) );
		}

		public static Vector2 ToVector2 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=2) {
				throw new ArgumentException(value + " is not valid value for Vector2");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Vector2( float.Parse(args[0], cult), float.Parse(args[1], cult) );
		}


		public static Half4 ToHalf4 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Half4");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Half4( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult), float.Parse(args[3], cult) );
		}

		public static Half3 ToHalf3 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Half3");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Half3( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult) );
		}

		public static Half2 ToHalf2 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','});
			if (args.Length!=2) {
				throw new ArgumentException(value + " is not valid value for Half2");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Half2( float.Parse(args[0], cult), float.Parse(args[1], cult) );
		}

	}
}

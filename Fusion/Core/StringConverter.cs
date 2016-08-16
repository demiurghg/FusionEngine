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

		public static bool TryConvertToString ( object value, out string stringValue )
		{
			stringValue = default(string);

			var type = value.GetType();

			if (type.IsEnum)				{ stringValue = ToString( (Enum)   value );	  return true; }
			if (type==typeof( String	) )	{ stringValue = ToString( ( String	)value ); return true; }
			if (type==typeof( Boolean	) )	{ stringValue = ToString( ( Boolean	)value ); return true; }
			if (type==typeof( Int16		) )	{ stringValue = ToString( ( Int16	)value ); return true; }
			if (type==typeof( Int32		) )	{ stringValue = ToString( ( Int32	)value ); return true; }
			if (type==typeof( Int64		) )	{ stringValue = ToString( ( Int64	)value ); return true; }
			if (type==typeof( Single	) )	{ stringValue = ToString( ( Single	)value ); return true; }
			if (type==typeof( Double	) )	{ stringValue = ToString( ( Double	)value ); return true; }
			if (type==typeof( Color		) )	{ stringValue = ToString( ( Color	)value ); return true; }
			if (type==typeof( Color4	) )	{ stringValue = ToString( ( Color4	)value ); return true; }
			if (type==typeof( Double	) )	{ stringValue = ToString( ( Double	)value ); return true; }
			if (type==typeof( Vector2	) )	{ stringValue = ToString( ( Vector2	)value ); return true; }
			if (type==typeof( Vector3	) )	{ stringValue = ToString( ( Vector3	)value ); return true; }
			if (type==typeof( Vector4	) )	{ stringValue = ToString( ( Vector4	)value ); return true; }
			if (type==typeof( Half		) )	{ stringValue = ToString( ( Half	)value ); return true; }
			if (type==typeof( Half2		) )	{ stringValue = ToString( ( Half2	)value ); return true; }
			if (type==typeof( Half3		) )	{ stringValue = ToString( ( Half3	)value ); return true; }
			if (type==typeof( Half4		) )	{ stringValue = ToString( ( Half4	)value ); return true; }

			return false;
		}



		public static bool TryConvertFromString ( Type type, string stringValue, out object value )
		{
			value	=	default(object);

			if (type.IsEnum)			{ value = Enum.Parse( type, stringValue, true ); return true; }
			if (type==typeof(String))	{ value = ToString  ( stringValue );	return true; }
			if (type==typeof(Boolean))	{ value = ToBoolean ( stringValue );	return true; }
			if (type==typeof(Int16)  )	{ value = ToInt16	( stringValue );	return true; }
			if (type==typeof(Int32)  )	{ value = ToInt32	( stringValue );	return true; }
			if (type==typeof(Int64)  )	{ value = ToInt64	( stringValue );	return true; }
			if (type==typeof(Single) )	{ value = ToSingle	( stringValue );	return true; }
			if (type==typeof(Double) )	{ value = ToDouble	( stringValue );	return true; }
			if (type==typeof(Color)  )	{ value = ToColor	( stringValue );	return true; }
			if (type==typeof(Color4) )	{ value = ToColor4	( stringValue );	return true; }
			if (type==typeof(Double) )	{ value = ToDouble	( stringValue );	return true; }
			if (type==typeof(Vector2))	{ value = ToVector2 ( stringValue );	return true; }
			if (type==typeof(Vector3))	{ value = ToVector3 ( stringValue );	return true; }
			if (type==typeof(Vector4))	{ value = ToVector4 ( stringValue );	return true; }
			if (type==typeof(Half)   )	{ value = ToHalf	( stringValue );	return true; }
			if (type==typeof(Half2)  )	{ value = ToHalf2	( stringValue );	return true; }
			if (type==typeof(Half3)  )	{ value = ToHalf3	( stringValue );	return true; }
			if (type==typeof(Half4)  )	{ value = ToHalf4	( stringValue );	return true; }

			return false;
		}



		public static string ConvertToString( object value )
		{
			string stringValue;

			if (!TryConvertToString( value, out stringValue )) {
				throw new ArgumentException(string.Format("Can not convert {0} to string", value.GetType()));
			}

			return stringValue;
		}



		public static object ConvertFromString ( Type type, string stringValue )
		{
			object value;
			
			if (!TryConvertFromString( type, stringValue, out value )) {
				throw new ArgumentException(string.Format("{0} is not valid value for {1}", stringValue, type));
			}

			return value;
		}





		public static T FromString<T>( string value )
		{
			return (T)ConvertFromString( typeof(T), value );
		}

		public static string ToString( String value )
		{
			return value;
		}

		public static string ToString( Boolean value )
		{
			return value.ToString(CultureInfo.InvariantCulture);
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

		
		//public static Bool ToBool ( string value )
		//{
		//	value = value.ToLowerInvariant();
		//	if (value=="0" || value=="false") return false;
		//	if (value=="1" || value=="true")  return true;
		//	throw new ArgumentException(value + " is not valid value for Bool");
		//}

		public static Boolean ToBoolean ( string value )
		{
			value = value.ToLowerInvariant();
			if (value=="0" || value=="false") return false;
			if (value=="1" || value=="true")  return true;
			throw new ArgumentException(value + " is not valid value for Bool");
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
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Color");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Color( byte.Parse(args[0], cult), byte.Parse(args[1], cult), byte.Parse(args[2], cult), byte.Parse(args[3], cult) );
		}

		public static Color4 ToColor4 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Color4");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Color4( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult), float.Parse(args[3], cult) );
		}

		public static Vector4 ToVector4 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Vector4");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Vector4( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult), float.Parse(args[3], cult) );
		}

		public static Vector3 ToVector3 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=3) {
				throw new ArgumentException(value + " is not valid value for Vector3");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Vector3( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult) );
		}

		public static Vector2 ToVector2 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=2) {
				throw new ArgumentException(value + " is not valid value for Vector2");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Vector2( float.Parse(args[0], cult), float.Parse(args[1], cult) );
		}


		public static Half4 ToHalf4 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=4) {
				throw new ArgumentException(value + " is not valid value for Half4");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Half4( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult), float.Parse(args[3], cult) );
		}

		public static Half3 ToHalf3 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=3) {
				throw new ArgumentException(value + " is not valid value for Half3");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Half3( float.Parse(args[0], cult), float.Parse(args[1], cult), float.Parse(args[2], cult) );
		}

		public static Half2 ToHalf2 ( string value )
		{
			var args = value.Split(new[]{' ','\t',';',','}, StringSplitOptions.RemoveEmptyEntries);
			if (args.Length!=2) {
				throw new ArgumentException(value + " is not valid value for Half2");
			}
			var cult = CultureInfo.InvariantCulture;
			return new Half2( float.Parse(args[0], cult), float.Parse(args[1], cult) );
		}

	}
}

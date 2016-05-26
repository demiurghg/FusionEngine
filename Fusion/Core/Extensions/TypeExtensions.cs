using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Extensions {
	public static class TypeExtensions {




		/// <summary>
		/// http://stackoverflow.com/questions/2296288/how-to-decide-a-type-is-a-custom-struct
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsStruct(this Type type)
		{
			return type.IsValueType 
					&& !type.IsPrimitive 
					&& !type.IsEnum 
					&& type != typeof(decimal)
					;
		}


		/// <summary>
		/// Get attribute of given type
		/// </summary>
		/// <typeparam name="AttributeType"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T GetCustomAttribute<T>( this Type type ) where T : Attribute 
		{
			var ca = type.GetCustomAttributes( typeof(T), true );
			if (ca.Count()<1) {
				return null;
			}
			return (T)ca.First(); 
		}


		/// <summary>
		/// Checks whether type has attribute of given type
		/// </summary>
		/// <typeparam name="AttributeType"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasAttribute<T> ( this Type type ) where T : Attribute
		{
			return type.GetCustomAttributes( typeof(T), true ).Any();
		}	  
	}
}

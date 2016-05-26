using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Fusion.Core.Extensions {
	public static class MemberInfoExtensions {




		/// <summary>
		/// Checks whether type has attribute of given type
		/// </summary>
		/// <typeparam name="AttributeType"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasAttribute<T> ( this PropertyInfo type ) where T : Attribute
		{
			return type.GetCustomAttributes( typeof(T), true ).Any();
		}	  


		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        public static bool IsList(this PropertyInfo field)
        {
            return typeof(IList).IsAssignableFrom(field.PropertyType);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        public static IList GetList(this PropertyInfo field, object obj)
        {
            return (IList)field.GetValue(obj);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        public static Type GetListElementType(this PropertyInfo field)
        {
            var interfaces = from i in field.PropertyType.GetInterfaces()
                             where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                             select i;

            return interfaces.First().GetGenericArguments()[0];
        }

	}
}

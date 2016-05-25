using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace Fusion.Core.Configuration {

	internal class ConfigVariable {

		public readonly string Name;
		public readonly string FullName;
		public readonly PropertyInfo TargetProperty;
		public readonly object TargetObject;
		public readonly string ComponentName;

		readonly TypeConverter converter;

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="pi"></param>
		/// <param name="obj"></param>
		public ConfigVariable ( string componentName, string prefix, string name, PropertyInfo targetProperty, object targetObject )
		{
			Name			=	name;
			FullName		=	prefix + "." + name;
			ComponentName	=	componentName;
			TargetProperty	=	targetProperty;
			TargetObject	=	targetObject;
			converter		=	TypeDescriptor.GetConverter( TargetProperty.PropertyType );
		}


		/// <summary>
		/// Sets config variable from text value.
		/// </summary>
		/// <param name="value"></param>
		public void Set ( string value )
		{
			TargetProperty.SetValue( TargetObject, converter.ConvertFromInvariantString( value ) );
		}


		
		/// <summary>
		/// Gets config variable value as string.
		/// </summary>
		/// <returns></returns>
		public string Get ()
		{
			return converter.ConvertToInvariantString( TargetProperty.GetValue(TargetObject) );
		}
	}

}

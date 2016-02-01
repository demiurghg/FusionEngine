using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Fusion.Engine.Graphics;
using Fusion.Engine.Audio;
using Fusion.Engine.Input;
using System.IO;

namespace ShooterDemo {
	public class SpawnParameters {

		readonly Dictionary<string,object> dictionary;

		static readonly Dictionary<string, Type> schema = new Dictionary<string, Type>();


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		public static void AddSchemaType<T> ( string key )
		{
			schema.Add( key, typeof(T) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		public static void AddSchemaType<T> ( string key, Type type )
		{
			schema.Add( key, type );
		}



		/// <summary>
		/// 
		/// </summary>
		public SpawnParameters ()
		{
			dictionary	=	new Dictionary<string,object>();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Add ( string key, object value )
		{
			Type type;

			if (schema.TryGetValue(key, out type)) {
				if ( !(value.GetType()==type) && !value.GetType().IsSubclassOf(type) ) {
					throw new ArgumentException(string.Format("The type of 'value' must be {0}", type));
				}
			}

			try {
				dictionary.Add( key, value );
				return true;
			} catch ( ArgumentException ) {
				return false;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool Remove ( string key )
		{
			try {
				dictionary.Remove( key );
				return true;
			} catch ( ArgumentException ) {
				return false;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object this[string key] {
			get {
				return dictionary[key];	
			}
		}
	}
}

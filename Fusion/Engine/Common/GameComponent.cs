using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Drivers.Graphics;
using Fusion.Core;
using Fusion.Framework;
using Fusion.Core.IniParser.Model;
using System.ComponentModel;
using System.Threading;


namespace Fusion.Engine.Common {

	public abstract class GameComponent : DisposableBase {

		public Game Game { get; protected set; }

		Stack<GameComponent> initializedComponents = new Stack<GameComponent>();

		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="game"></param>
		public GameComponent ( Game game )
		{
			this.Game = game;
		}


		/// <summary>
		/// Intializes module.
		/// </summary>
		public abstract void Initialize ();



		/// <summary>
		/// Iniitalizes componets and push it to initialized component stack.
		/// </summary>
		/// <param name="component"></param>
		protected void InitializeComponent ( GameComponent component )
		{
			initializedComponents.Push( component );
			Log.Message("---- Init : {0} ----", component.GetType().Name );
			component.Initialize();
		}



		/// <summary>
		/// Disposes all initialized components in reverse order.
		/// </summary>
		protected void DisposeComponents ()
		{
			while (initializedComponents.Any()) {
				var component = initializedComponents.Pop();
				Log.Message("Dispose : {0}", component.GetType().Name );
				SafeDispose( ref component );
			}
		}



		/// <summary>
		/// Gets configuration as collection of KeyData.
		/// Derived classes can replace this to customize what to save into configuration. 
		/// </summary>
		/// <returns>Collection of KeyData</returns>
		public virtual IEnumerable<KeyData> GetConfiguration ()
		{
			var sectionData		=	new List<KeyData>();
			var configObject	=	this;

			if (configObject==null) {
				return sectionData;
			}

			foreach ( var prop in GetConfigurationProperties() ) {

				var name	=	prop.Name;
				var value	=	prop.GetValue( configObject );
				var conv	=	TypeDescriptor.GetConverter( prop.PropertyType );
				var keyData	=	new KeyData(name);

				keyData.Value	=	conv.ConvertToInvariantString( value );

				sectionData.Add( keyData );
			}

			return sectionData;
		}



		/// <summary>
		/// Sets module configuration from collection of KeyData.
		/// Derived classes can replace this to customize how to load configuration. 
		/// </summary>
		/// <param name="configuration"></param>
		public virtual void SetConfiguration ( IEnumerable<KeyData> configuration )
		{
			var configObject	=	this;

			if (configObject==null) {
				return;
			}

			foreach ( var keyData in configuration ) {
						
				var prop =	GetConfigurationProperty( keyData.KeyName );

				if (prop==null) {
					Log.Warning("Config property {0} does not exist. Key ignored.", keyData.KeyName );
					continue;
				}

				var conv	=	TypeDescriptor.GetConverter( prop.PropertyType );
						
				prop.SetValue( configObject, conv.ConvertFromInvariantString( keyData.Value ));
			}
		}



		/// <summary>
		/// Gets all properties marked with ConfigAttribute.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<PropertyInfo> GetConfigurationProperties ()
		{
			return GetType().GetProperties().Where( p=>p.GetCustomAttribute(typeof(ConfigAttribute))!=null).ToArray();
		}



		/// <summary>
		/// Gets all properties marked with ConfigAttribute.
		/// </summary>
		/// <returns></returns>
		PropertyInfo GetConfigurationProperty ( string name )
		{
			var prop = GetType().GetProperty( name );

			if (prop==null) {
				return null;
			}

			if (prop.GetCustomAttribute(typeof(ConfigAttribute))==null) {
				return null;
			}

			return prop;
		}
	}
}

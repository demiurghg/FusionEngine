﻿using System;
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

	public abstract class GameModule : DisposableBase {

		public Game Game { get; protected set; }
		Queue<Action> actionQueue = new Queue<Action>();

		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="game"></param>
		public GameModule ( Game game )
		{
			this.Game = game;
		}


		/// <summary>
		/// Intializes module.
		/// </summary>
		public abstract void Initialize ();



		/// <summary>
		/// Pushes action to the invoke queue.
		/// </summary>
		/// <param name="action"></param>
		public virtual void Invoke ( Action action )
		{
			lock ( actionQueue ) {
				actionQueue.Enqueue( action );
			}
		}



		/// <summary>
		/// Calls all actions pushed to invoke queue.
		/// </summary>
		protected virtual void Dispatch ()
		{
			List<Action> actions;

			lock ( actionQueue ) {
				actions = actionQueue.ToList();
				actionQueue.Clear();
			}

			foreach ( var action in actions ) {
				action();
			}
		}



		//[Conditional("DEBUG")]
		/// <summary>
		/// 
		/// </summary>
		internal void VerifyServiceThread()
		{
			//Thread ct = Thread.CurrentThread;
			//if (Thread.CurrentThread != m_networkThread)
			//	throw new NetException("Executing on wrong thread! Should be service thread thread (is " + ct.Name + " mId " + ct.ManagedThreadId + ")");
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
		public PropertyInfo GetConfigurationProperty ( string name )
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


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Internal stuff
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Represents game module binding.
		/// </summary>
		internal class ModuleBinding {
			public readonly string NiceName;
			public readonly string ShortName;
			public readonly GameModule Module;
			public readonly InitOrder InitOrder;
			
			public ModuleBinding ( GameModule module, string niceName, string shortName, InitOrder initOrder )
			{
				if (module==null) {
					throw new ArgumentNullException(string.Format("Module \"{0}\" is null.", niceName));
				}
				Module		=	module;
				NiceName	=	niceName;
				ShortName	=	shortName;
				InitOrder	=	initOrder;
			}
		}



		/// <summary>
		/// Enumerates all modules and submodules in BFS order.
		/// </summary>
		/// <param name="rootObj"></param>
		/// <returns></returns>
		internal static IEnumerable<ModuleBinding> Enumerate ( object rootObj )
		{
			var list = new List<ModuleBinding>();

			var queue = new LinkedList<ModuleBinding>();

			foreach ( var module in GetModuleBindings(rootObj).Reverse() ) {
				queue.AddFirst( module );
			}

			while (queue.Any()) {
				var module = queue.First();
				queue.RemoveFirst();

				list.Add(module);

				foreach ( var childModule in GetModuleBindings(module.Module) ) {
					queue.AddFirst( childModule );
				}
			}

			return list;
		}



		/// <summary>
		/// Gets module binding for each module declared in parentModule.
		/// </summary>
		/// <param name="parentModule"></param>
		/// <returns></returns>
		static IEnumerable<ModuleBinding> GetModuleBindings ( object parentModule )
		{
			return parentModule.GetType()
						.GetProperties()
						.Where( prop => prop.GetCustomAttribute<GameModuleAttribute>() != null )
						.Select( prop1 => new ModuleBinding((GameModule)prop1.GetValue( parentModule ), 
							prop1.GetCustomAttribute<GameModuleAttribute>().NiceName, 
							prop1.GetCustomAttribute<GameModuleAttribute>().ShortName,
							prop1.GetCustomAttribute<GameModuleAttribute>().InitOrder ) )
						;
		}



		/// <summary>
		/// Perform recursive module initialization.
		/// </summary>
		/// <param name="root"></param>
		static void InitRecursive ( ModuleBinding root, string prefix )
		{
			var children = GetModuleBindings( root.Module );

			foreach ( var child in children ) {
				if (child.InitOrder==InitOrder.Before) {
					InitRecursive( child, prefix + root.NiceName + "/" );
				}
			}

			Log.Message( "---- Init : {0} ----", prefix + root.NiceName );
			root.Module.Initialize();

			disposeList.Add( root );

			foreach ( var child in children ) {
				if (child.InitOrder==InitOrder.After) {
					InitRecursive( child, prefix + root.NiceName + "/" );
				}
			}
		}



		/// <summary>
		/// Prints module names.
		/// </summary>
		static internal void PrintModuleNames ()
		{
			Log.Message("");
			foreach ( var bind in disposeList ) {
				int cfgVarCount = bind.Module.GetConfigurationProperties().Count();
				Log.Message( "  {0,-18} {1,-7} {2,7} {3,4} vars", bind.NiceName, bind.ShortName, bind.InitOrder, cfgVarCount );
			}
			Log.Message("{0} modules", disposeList.Count);
		}



		/// <summary>
		/// Calls 'Initialize' method on all modules starting from top one tree.
		/// </summary>
		/// <param name="obj"></param>
		static internal void InitializeAll ( object rootObj )
		{
			foreach ( var m in GetModuleBindings(rootObj) ) {
				InitRecursive(m,"");
			}
		}


		static List<ModuleBinding> disposeList = new List<ModuleBinding>();


		/// <summary>
		/// Calls 'Initialize' method on all services starting from top one tree.
		/// </summary>
		/// <param name="rootObj"></param>
		static internal void DisposeAll ( object rootObj )
		{
			disposeList.Reverse();
			foreach ( var bind in disposeList ) {
				Log.Message( "---- Dispose : {0} ----", bind.NiceName );

				bind.Module.Dispose();
			}
		}
	}
}

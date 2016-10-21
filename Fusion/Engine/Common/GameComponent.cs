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
using Fusion.Engine.Tools;
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
		/// Intializes component.
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
	}
}

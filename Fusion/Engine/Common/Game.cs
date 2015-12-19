using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading.Tasks;
using Fusion.Drivers.Audio;
using System.Globalization;
using System.Threading;
using Fusion.Drivers.Input;
using System.IO;
using System.Diagnostics;
using Fusion.Drivers.Graphics;
using SharpDX.Windows;
using Fusion.Core;
using Fusion.Core.Development;
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.IniParser;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;
using Fusion.Engine.Network;
using Fusion.Engine.Client;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Server;
using Lidgren.Network;
using Fusion.Engine.Storage;


namespace Fusion.Engine.Common {

	/// <summary>
	/// Provides basic graphics device initialization, game logic, and rendering code. 
	/// </summary>
	public class Game : DisposableBase {

		/// <summary>
		/// Game instance.
		/// </summary>
		public static Game Instance = null;

		/// <summary>
		/// Gets the current audio device
		/// </summary>
		internal AudioDevice	AudioDevice { get { return audioDevice; } }

		/// <summary>
		/// Gets the current input device
		/// </summary>
		internal InputDevice	InputDevice { get { return inputDevice; } }

		/// <summary>
		/// Gets the current graphics device
		/// </summary>
		internal GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }

		/// <summary>
		/// Gets the current graphics engine
		/// </summary>
		[GameModule("Graphics", "ge", InitOrder.After)]
		public	GraphicsEngine GraphicsEngine { get { return graphicsEngine; } }

		[GameModule("Network", "net", InitOrder.After)]
		public NetworkEngine Network { get { return network; } }


		/// <summary>
		/// Gets current content manager
		/// </summary>
		public ContentManager Content { get { return content; } }

		/// <summary>
		/// Gets keyboard.
		/// </summary>
		[GameModule("Keyboard", "kb", InitOrder.After)]
		public Keyboard Keyboard { get { return keyboard; } }

		/// <summary>
		/// Gets mouse.
		/// </summary>
		[GameModule("Mouse", "mouse", InitOrder.After)]
		public Mouse Mouse { get { return mouse; } }

		/// <summary>
		/// Gets gamepads
		/// </summary>
		public GamepadCollection Gamepads { get { return gamepads; } }

		/// <summary>
		/// Gets current content manager
		/// </summary>
		public	Invoker Invoker { get { return invoker; } }

		/// <summary>
		/// Gets user storage.
		/// </summary>
		public UserStorage UserStorage { get { return userStorage; } }


		/// <summary>
		/// Sets and gets game window icon.
		/// </summary>
		public System.Drawing.Icon Icon {
			get {
				return windowIcon;
			}
			set {
				if (IsInitialized) {
					throw new InvalidOperationException("Can not set Icon after game engine initialization");
				}
				windowIcon = value;
			}
		}
		System.Drawing.Icon windowIcon = null;


		/// <summary>
		/// Gets and sets game window title.
		/// </summary>
		public string GameTitle { 
			get {
				return gameTitle;
			} 
			set {
				if (value==null) {
					throw new ArgumentNullException();
				}
				if (string.IsNullOrWhiteSpace(value)) {
					throw new ArgumentException("GameTitle must be readable string", "value");
				}
				if (IsInitialized) {
					throw new InvalidOperationException("Can not set GameTitle after game engine initialization");
				}
				gameTitle = value;
			} 
		}
		string gameTitle = Path.GetFileNameWithoutExtension( Process.GetCurrentProcess().ProcessName );


		/// <summary>
		/// Enable COM object tracking
		/// </summary>
		public bool TrackObjects {
			get {
				return SharpDX.Configuration.EnableObjectTracking;
			} 
			set {
				SharpDX.Configuration.EnableObjectTracking = value;
			}
		}


		/// <summary>
		/// Indicates whether the game is initialized.
		/// </summary>
		public	bool IsInitialized { get { return initialized; } }

		/// <summary>
		/// Indicates whether Game.Update and Game.Draw should be called on each frame.
		/// </summary>
		public	bool Enabled { get; set; }

		/// <summary>
		/// Raised when the game exiting before disposing
		/// </summary>
		public event	EventHandler Exiting;

		/// <summary>
		/// Raised after Game.Reload() called.
		/// This event used primarily for developement puprpose.
		/// </summary>
		public event	EventHandler Reloading;


		/// <summary>
		/// Raised when the game gains focus.
		/// </summary>
		public event	EventHandler Activated;

		/// <summary>
		/// Raised when the game loses focus.
		/// </summary>
		public event	EventHandler Deactivated;


		bool	initialized		=	false;
		bool	requestExit		=	false;
		bool	requestReload	=	false;

		internal bool ExitRequested { get { return requestExit; } }


		AudioDevice			audioDevice		;
		InputDevice			inputDevice		;
		GraphicsDevice		graphicsDevice	;
		//AudioEngine			audioEngine		;
		//InputEngine			inputEngine		;
		GraphicsEngine		graphicsEngine	;
		NetworkEngine		network			;
		ContentManager		content			;
		Invoker				invoker			;
		Keyboard			keyboard		;
		Mouse				mouse			;
		GamepadCollection	gamepads		;
		UserStorage			userStorage		;


		GameTime	gameTimeInternal;

		GameServer	sv;
		GameClient cl;
		GameInterface gi;


		/// <summary>
		/// Current game server.
		/// </summary>
		[GameModule("Server", "sv", InitOrder.After)]
		public GameServer GameServer { get { return sv; } set { sv = value; } }
		
		/// <summary>
		/// Current game client.
		/// </summary>
		[GameModule("Client", "cl", InitOrder.After)]
		public GameClient GameClient { get { return cl; } set { cl = value; } }

		/// <summary>
		/// Current game interface.
		/// </summary>
		[GameModule("Interface", "ui", InitOrder.After)]
		public GameInterface GameInterface { get { return gi; } set { gi = value; } }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="p"></param>
		/// <param name="sv"></param>
		/// <param name="cl"></param>
		/// <param name="gi"></param>
		public void Run ()
		{
			InitInternal();
			RenderLoop.Run( GraphicsDevice.Display.Window, UpdateInternal );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string GetReleaseInfo ()
		{
			return string.Format("{0} {1} {2}", 
				Assembly.GetExecutingAssembly().GetName().Name, 
				Assembly.GetExecutingAssembly().GetName().Version,
				#if DEBUG
					"debug"
				#else
					"release"
				#endif
			);
		}



		/// <summary>
		/// Game ID is used for networking as application identifier.
		/// </summary>
		public string GameID {
			get { return gameId; }
		}
		readonly string gameId;


		/// <summary>
		/// Initializes a new instance of this class, which provides 
		/// basic graphics device initialization, game logic, rendering code, and a game loop.
		/// </summary>
		public Game (string gameId)
		{
			this.gameId	=	gameId;
			Enabled	=	true;

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += currentDomain_UnhandledException;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			CultureInfo.DefaultThreadCurrentCulture	=	CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture		=	CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture	=	CultureInfo.InvariantCulture;

			Debug.Assert( Instance == null );

			Instance	=	this;

			Log.Message(GetReleaseInfo());
			Log.Message("Startup directory : {0}", AppDomain.CurrentDomain.BaseDirectory );
			Log.Message("Current directory : {0}", Directory.GetCurrentDirectory() );

			//	For animation rendering applications :
			//	http://msdn.microsoft.com/en-us/library/bb384202.aspx
			GCSettings.LatencyMode	=	GCLatencyMode.SustainedLowLatency;

			audioDevice			=	new AudioDevice( this );
			inputDevice			=	new InputDevice( this );
			graphicsDevice		=	new GraphicsDevice( this );
			graphicsEngine		=	new GraphicsEngine( this );
			network				=	new NetworkEngine( this );
			content				=	new ContentManager( this );
			gameTimeInternal	=	new GameTime();
			invoker				=	new Invoker(this, CommandAffinity.Default);

			keyboard			=	new Keyboard(this);
			mouse				=	new Mouse(this);
			gamepads			=	new GamepadCollection(this);

			userStorage			=	new UserStorage(this);

		}



		void currentDomain_UnhandledException ( object sender, UnhandledExceptionEventArgs e )
		{
			ExceptionDialog.Show( (Exception) e.ExceptionObject );
		}



		
		/// <summary>
		/// Manage game to raise Reloading event.
		/// </summary>
		public void Reload()
		{
			if (!IsInitialized) {
				throw new InvalidOperationException("Game is not initialized");
			}
			requestReload = true;
		}



		/// <summary>
		/// Request game to exit.
		/// Game will quit when update & draw loop will be completed.
		/// </summary>
		public void Exit ()
		{
			if (!IsInitialized) {
				Log.Warning("Game is not initialized");
				return;
			}
			requestExit	=	true;
		}



		/// <summary>
		/// InitInternal
		/// </summary>
		internal bool InitInternal ()
		{
			Log.Message("");
			Log.Message("-------- Game Initializing --------");

			var p = new GraphicsParameters();
			GraphicsEngine.ApplyParameters( ref p );

			GraphicsDevice.Initialize( p );
			InputDevice.Initialize();
			AudioDevice.Initialize();

			//	init game :
			Log.Message("");

			GameModule.InitializeAll( this );

			initialized	=	true;

			Log.Message("-----------------------------------------");
			Log.Message("");

			return true;
		}





		/// <summary>
		/// Overloaded. Immediately releases the unmanaged resources used by this object. 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (!initialized) {
				return;
			}

			Log.Message("");
			Log.Message("-------- Game Shutting Down --------");

			//	wait for server 
			//	if it is still running :
			cl.Wait();
			sv.Wait();

			
			//	call exit event :
			if (Exiting!=null) {
				Exiting(this, EventArgs.Empty);
			}

			if (disposing) {

				GameModule.DisposeAll( this );

				content.Dispose();

				Log.Message("Disposing : Input Device");
				SafeDispose( ref inputDevice );

				Log.Message("Disposing : Audio Device");
				SafeDispose( ref audioDevice );

				Log.Message("Disposing : Graphics Device");
				SafeDispose( ref graphicsDevice );

				Log.Message("Disposing : User Storage");
				SafeDispose( ref userStorage );
			}

			base.Dispose(disposing);

			Log.Message("------------------------------------------");
			Log.Message("");

			ReportActiveComObjects();
		}



		/// <summary>
		/// Print warning message if leaked objectes detected.
		/// Works only if GameParameters.TrackObjects set.
		/// </summary>
		public void ReportActiveComObjects ()
		{
			if (SharpDX.Configuration.EnableObjectTracking) {
				if (SharpDX.Diagnostics.ObjectTracker.FindActiveObjects().Any()) {
					Log.Warning("{0}", SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects() );
				} else {
					Log.Message("Leaked COM objects are not detected.");
				}
				SharpDX.Configuration.EnableObjectTracking = false;
			} else {
				Log.Message("Object tracking disabled.");
			}
		}



		/// <summary>
		/// Returns true if game is active and receive user input
		/// </summary>
		public bool IsActive {
			get {
				return GraphicsDevice.Display.Window.Focused;
			}
		}




		bool isActiveLastFrame = true;


		/// <summary>
		/// 
		/// </summary>
		internal void UpdateInternal ()
		{
			if (IsDisposed) {
				throw new ObjectDisposedException("Game");
			}

			if (!IsInitialized) {
				throw new InvalidOperationException("Game is not initialized");
			}

			bool isActive = IsActive;  // to reduce access to winforms.
			if (isActive!=isActiveLastFrame) {
				isActiveLastFrame = isActive;
				if (isActive) {
					if (Activated!=null) { Activated(this, EventArgs.Empty); } 
				} else {
					if (Deactivated!=null) { Deactivated(this, EventArgs.Empty); } 
				}
			}

			if (Enabled) {

				if (requestReload) {
					if (Reloading!=null) {
						Reloading(this, EventArgs.Empty);
					}
					requestReload = false;
				}

				graphicsDevice.Display.Prepare();

				//	pre update :
				gameTimeInternal.Update();

				InputDevice.UpdateInput();

				//GIS.Update(gameTimeInternal);

				UpdateClientServerGame( gameTimeInternal );


				//
				//	Render :
				//
				var eyeList	= graphicsDevice.Display.StereoEyeList;

				foreach ( var eye in eyeList ) {

					GraphicsDevice.ResetStates();

					GraphicsDevice.Display.TargetEye = eye;

					GraphicsDevice.RestoreBackbuffer();

					GraphicsDevice.ClearBackbuffer(Color.Zero);

					this.Draw( gameTimeInternal, eye );

					gameTimeInternal.AddSubframe();
				}

				GraphicsDevice.Present(GraphicsEngine.Config.VSyncInterval);

				InputDevice.EndUpdateInput();
			}

			try {
				invoker.ExecuteQueue( gameTimeInternal, CommandAffinity.Default );
			} catch ( Exception e ) {
				Log.Error( e.Message );
			}

			CheckExitInternal();
		}



		/// <summary>
		/// Called when the game determines it is time to draw a frame.
		/// In stereo mode this method will be called twice to render left and right parts of stereo image.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected virtual void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			//GIS.Draw(gameTime, stereoEye);

			GraphicsEngine.Draw( gameTime, stereoEye );

			GraphicsDevice.ResetStates();
			GraphicsDevice.RestoreBackbuffer();
		}
		

		
		/// <summary>
		/// Performs check and does exit
		/// </summary>
		private void CheckExitInternal () 
		{
			if (requestExit) {
				GraphicsDevice.Display.Window.Close();
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Configuration stuff :
		 * 
		-----------------------------------------------------------------------------------------*/


		/// <summary>
		/// Loads configuration for each subsystem
		/// </summary>
		/// <param name="path"></param>
		public void LoadConfiguration ( string filename )
		{
			Log.Message("Loading configuration...");

			Invoker.FeedConfigs( ConfigSerializer.GetConfigVariables( GameModule.Enumerate(this) ) );

			if (userStorage.FileExists(filename)) {
				ConfigSerializer.LoadFromStream( GameModule.Enumerate(this), UserStorage.OpenFile(filename, FileMode.Open, FileAccess.Read) );
			} else {
				Log.Warning("Can not load configuration from {0}", filename);
			}
		}


		/// <summary>
		/// Saves configuration to XML file	for each subsystem
		/// </summary>
		/// <param name="path"></param>
		public void SaveConfiguration ( string filename )
		{	
			Log.Message("Saving configuration...");

			UserStorage.DeleteFile(filename);
			ConfigSerializer.SaveToStream( GameModule.Enumerate(this), UserStorage.OpenFile(filename, FileMode.Create, FileAccess.Write) );
		}




		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Client-server stuff :
		 * 
		-----------------------------------------------------------------------------------------*/


		
		/// <summary>
		/// Updates game logic and client-server interaction.
		/// </summary>
		/// <param name="gameTime"></param>
		void UpdateClientServerGame ( GameTime gameTime )
		{
			cl.UpdateInternal( gameTime );

			gi.UpdateInternal( gameTime );
		}



		internal void StartServer ( string map )
		{
			var postCmd = string.Format("connect 127.0.0.1 {0}", Network.Config.Port );

			//	Disconnect!

			if (GameClient!=null) {
				GameServer.StartInternal( map, postCmd );
			} else {
				GameServer.StartInternal( map, null );
			}
		}


		internal void KillServer ()
		{
			GameServer.KillInternal();
		}



		internal void Connect ( string host, int port )
		{
			GameClient.ConnectInternal(host, port);
			//	Kill server!
		}


		internal void Disconnect ( string message )
		{
			GameClient.DisconnectInternal(message);
			//	Kill server!
		}
	}
}

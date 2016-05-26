using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading.Tasks;
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
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Lidgren.Network;
using Fusion.Engine.Storage;
using Fusion.Engine.Audio;
using Fusion.Framework;
using Fusion.Engine.Frames;


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
		/// Gets settings.
		/// </summary>
		public ConfigManager Config { get { return config; } }

		/// <summary>
		/// Gets the current input device
		/// </summary>
		internal InputDevice	InputDevice { get { return inputDevice; } }

		/// <summary>
		/// Gets the current graphics device
		/// </summary>
		internal GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }

		/// <summary>
		/// Gets the render system
		/// </summary>
		public	RenderSystem RenderSystem { get { return renderSystem; } }

		/// <summary>
		/// Gets the sound system
		/// </summary>
		public SoundSystem SoundSystem { get { return soundSystem; } }

		/// <summary>
		/// Gets the network system.
		/// Actually used only for configuration both client and server.
		/// </summary>
		public Network Network { get { return network; } }

		/// <summary>
		/// Gets current content manager
		/// </summary>
		public ContentManager Content { get { return content; } }

		/// <summary>
		/// Gets keyboard.
		/// </summary>
		public Keyboard Keyboard { get { return keyboard; } }

		/// <summary>
		/// Gets mouse.
		/// </summary>
		public Mouse Mouse { get { return mouse; } }

		/// <summary>
		/// Gets mouse.
		/// </summary>
		public Touch Touch { get { return touch; } }

		/// <summary>
		/// Gets gamepads
		/// </summary>
		public GamepadCollection Gamepads { get { return gamepads; } }

		/// <summary>
		/// Gets invoker
		/// </summary>
		public	Invoker Invoker { get { return invoker; } }

		/// <summary>
		/// Gets user storage.
		/// </summary>
		public UserStorage UserStorage { get { return userStorage; } }

		/// <summary>
		/// Gets console
		/// </summary>
		public GameConsole Console { get { return console; } }

		/// <summary>
		/// Gets frame processor
		/// </summary>
		public FrameProcessor Frames { get { return frames; } }


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


		ConfigManager		config		;
		InputDevice			inputDevice		;
		GraphicsDevice		graphicsDevice	;
		RenderSystem		renderSystem	;
		SoundSystem			soundSystem		;
		Network				network			;
		ContentManager		content			;
		Invoker				invoker			;
		Keyboard			keyboard		;
		Mouse				mouse			;
		Touch				touch;
		GamepadCollection	gamepads		;
		UserStorage			userStorage		;
		GameConsole			console;
		FrameProcessor		frames;


		GameTime	gameTimeInternal;

		GameServer	sv;
		GameClient cl;
		UserInterface gi;


		/// <summary>
		/// Current game server.
		/// </summary>
		public GameServer GameServer { 
			get { return sv; } 
			set { 
				if (IsInitialized) {
					throw new InvalidOperationException("Can not set server after initialization");
				}
				if (sv!=null) {
					throw new InvalidOperationException("Game server is already set");
				}
				sv = value; 
				Config.ExposeProperties( sv, "GameServer", "sv" );
			} 
		}
		
		/// <summary>
		/// Current game client.
		/// </summary>
		public GameClient GameClient {
			get { return cl; } 
			set { 
				if (IsInitialized) {
					throw new InvalidOperationException("Can not set client after initialization");
				}
				if (cl!=null) {
					throw new InvalidOperationException("Game client is already set");
				}
				cl = value; 
				Config.ExposeProperties( cl, "GameClient", "cl" );
			} 
		}

		/// <summary>
		/// Current game interface.
		/// </summary>
		public UserInterface UserInterface {
			get { return gi; } 
			set { 
				if (IsInitialized) {
					throw new InvalidOperationException("Can not set user interface after initialization");
				}
				if (gi!=null) {
					throw new InvalidOperationException("User interface is already set");
				}
				gi = value; 
				Config.ExposeProperties( gi, "UserInterface", "ui" );
			} 
		}


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
			return string.Format("{0} {1} {2} {3}", 
				Assembly.GetExecutingAssembly().GetName().Name, 
				Assembly.GetExecutingAssembly().GetName().Version,
				#if DEBUG
					"debug",
				#else
					"release",
				#endif
				(IntPtr.Size==4? "x86" : "x64")
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
		public Game ( string gameId )
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


			//ForceAssemblies();


			Debug.Assert( Instance == null );

			Instance	=	this;

			Log.Message(GetReleaseInfo());
			Log.Message("Startup directory : {0}", AppDomain.CurrentDomain.BaseDirectory );
			Log.Message("Current directory : {0}", Directory.GetCurrentDirectory() );

			//	For animation rendering applications :
			//	http://msdn.microsoft.com/en-us/library/bb384202.aspx
			GCSettings.LatencyMode	=	GCLatencyMode.SustainedLowLatency;

			config				=	new ConfigManager( this );
			inputDevice			=	new InputDevice( this );
			graphicsDevice		=	new GraphicsDevice( this );
			renderSystem		=	new RenderSystem( this );
			soundSystem			=	new SoundSystem( this );
			network				=	new Network( this );
			content				=	new ContentManager( this );
			gameTimeInternal	=	new GameTime();
			invoker				=	new Invoker(this);
			console				=	new GameConsole(this);

			keyboard			=	new Keyboard(this);
			mouse				=	new Mouse(this);
			touch				=	new Touch(this);
			gamepads			=	new GamepadCollection(this);

			frames				=	new FrameProcessor(this, "profont");

			userStorage			=	new UserStorage(this);

			config.ExposeProperties( SoundSystem,	"SoundSystem",		"snd"	);
			config.ExposeProperties( RenderSystem,	"RenderSystem",		"rs"	);
			config.ExposeProperties( Frames,		"Frames",			"frames");
			config.ExposeProperties( Console,		"Console",			"con"	);
			config.ExposeProperties( Network,		"Network",			"net"	);

			config.ExposeProperties( Keyboard,		"Keyboard",			"kb"	);
			config.ExposeProperties( Touch,			"Touch",			"touch"	);
			config.ExposeProperties( Mouse,			"Mouse",			"mouse"	);
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


		bool requestFullscreenOnStartup = false;



		/// <summary>
		/// InitInternal
		/// </summary>
		internal bool InitInternal ()
		{
			Log.Message("");
			Log.Message("-------- Game Initializing --------");

			var p = new GraphicsParameters();
			RenderSystem.ApplyParameters( ref p );

			//	going to fullscreen immediatly on startup lead to 
			//	bugs and inconsistnecy for diferrent stereo modes,
			//	so we store fullscreen mode and apply it on next update step.
			requestFullscreenOnStartup	=	p.FullScreen;
			p.FullScreen = false;


			//	initialize drivers :
			GraphicsDevice.Initialize(p);
			InputDevice.Initialize();
																		   
			//	initiliaze core systems :
			Initialize( SoundSystem );
			Initialize( RenderSystem );
			Initialize( Keyboard );
			Initialize( Mouse );
			Initialize( Touch );

			//	initialize additional systems :
			Initialize( Console );
			Initialize( Frames );

			//	initialize game-specific systems :
			Initialize( UserInterface );
			Initialize( GameClient );
			Initialize( GameServer );

			//	init game :
			Log.Message("");

			//	attach console sprite layer :
			Console.ConsoleSpriteLayer.Order = int.MaxValue / 2;
			RenderSystem.SpriteLayers.Add( Console.ConsoleSpriteLayer );

			Frames.FramesSpriteLayer.Order = int.MaxValue / 2 - 1;
			RenderSystem.SpriteLayers.Add( Frames.FramesSpriteLayer );

			initialized	=	true;

			Log.Message("-----------------------------------------");
			Log.Message("");

			return true;
		}



		Stack<GameComponent> modules = new Stack<GameComponent>();


		void Initialize ( GameComponent module )
		{
			Log.Message( "---- Init : {0} ----", module.GetType().Name);
			module.Initialize();

			modules.Push( module );
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

				while ( modules.Any() ) {
					var module = modules.Pop();
					Log.Message("Disposing : {0}", module.GetType().Name );
					module.Dispose();
				}

				Log.Message("Disposing : Content");
				SafeDispose( ref content );

				Log.Message("Disposing : InputDevice");
				SafeDispose( ref inputDevice );

				Log.Message("Disposing : GraphicsDevice");
				SafeDispose( ref graphicsDevice );

				Log.Message("Disposing : UserStorage");
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

		bool skipFirstFrame	= true;


		/// <summary>
		/// 
		/// </summary>
		internal void UpdateInternal ()
		{
			/*if (skipFirstFrame) {
				skipFirstFrame = false;
				Thread.Sleep(1);
				return;
			} */

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

				if (requestFullscreenOnStartup) {
					graphicsDevice.FullScreen = requestFullscreenOnStartup;
					requestFullscreenOnStartup = false;
				}

				if (requestReload) {
					if (Reloading!=null) {
						Reloading(this, EventArgs.Empty);
					}
					requestReload = false;
				}

				graphicsDevice.Prepare();
				graphicsDevice.Display.Prepare();

				//	pre update :
				gameTimeInternal.Update();

				InputDevice.UpdateInput();

				//GIS.Update(gameTimeInternal);

				UpdateClientServerGame( gameTimeInternal );

				//
				//	Sound :
				//
				SoundSystem.Update( gameTimeInternal );

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
				}

				GraphicsDevice.Present(RenderSystem.VSyncInterval);

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

			RenderSystem.Draw( gameTime, stereoEye );

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
			//Log.Message("Loading configuration...");

			//Invoker.FeedConfigs( ConfigSerializer.GetConfigVariables( GameModule.Enumerate(this) ) );

		}


		/// <summary>
		/// Saves configuration to XML file	for each subsystem
		/// </summary>
		/// <param name="path"></param>
		public void SaveConfiguration ( string filename )
		{	
			//Log.Message("Saving configuration...");

			//UserStorage.DeleteFile(filename);
			//ConfigSerializer.SaveToStream( GameModule.Enumerate(this), UserStorage.OpenFile(filename, FileMode.Create, FileAccess.Write) );
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
			var postCmd = string.Format("connect 127.0.0.1 {0}", Network.Port );

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

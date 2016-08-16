//#define USE_PROFONT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion.Core;
using Fusion.Core.Utils;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Core.Configuration;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;
using Fusion.Core.Shell;



namespace Fusion.Framework {
	
	public sealed partial class GameConsole : GameComponent {



		class Line {
			public readonly TraceEventType EventType;
			public readonly string Message;

			public Line ( TraceEventType eventType, string message ) 
			{
				EventType	=	eventType;
				Message		=	message;
			}
		}
		
		List<string> lines = new List<string>();

		#if USE_PROFONT
		const string FontName = "profont";
		SpriteFont	consoleFont;
		#else
		const string FontName = "conchars";
		DiscTexture	consoleFont;
		#endif
		SpriteLayer consoleLayer;
		SpriteLayer editLayer;
		

		float showFactor = 0;
		string font;

		EditBox	editBox;


		int scroll = 0;

		bool isShown = false;

		/// <summary>
		/// Show/Hide console.
		/// </summary>
		public bool IsShown { get { return isShown; } }


		Invoker.Suggestion suggestion = null;




		/// <summary>
		/// 
		/// </summary>
		/// <param name="Game"></param>
		/// <param name="font"></param>
		public GameConsole ( Game Game ) : base(Game)
		{
			SetDefaults();

			this.font		=	FontName;

			editBox		=	new EditBox(this);
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			editBox.FeedHistory( GetHistory() );

			consoleLayer	=	new SpriteLayer( Game.RenderSystem, 1024 );
			editLayer		=	new SpriteLayer( Game.RenderSystem, 1024 );
			consoleLayer.Order = 9999;
			consoleLayer.Layers.Add( editLayer );

			LoadContent();
			Game.Reloading += (s,e) => LoadContent();

			Game.GraphicsDevice.DisplayBoundsChanged += GraphicsDevice_DisplayBoundsChanged;
			LogRecorder.TraceRecorded += TraceRecorder_TraceRecorded;
			Game.Keyboard.KeyDown += Keyboard_KeyDown;
			Game.Keyboard.FormKeyPress += Keyboard_FormKeyPress;
			Game.Keyboard.FormKeyDown += Keyboard_FormKeyDown;

			RefreshConsole();
			RefreshEdit();
		}


		#if USE_PROFONT
			int charHeight { get { return consoleFont.LineHeight; } }
			int charWidth { get { return consoleFont.SpaceWidth; } }
		#else
			int charHeight { get { return 9; } }
			int charWidth { get { return 8; } }
		#endif


		/// <summary>
		/// Gets root console's sprite layer
		/// </summary>
		public SpriteLayer ConsoleSpriteLayer {
			get {
				return consoleLayer;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			#if USE_PROFONT
			consoleFont			=	Game.Content.Load<SpriteFont>("profont");
			#else
			consoleFont			=	Game.Content.Load<DiscTexture>(font);
			#endif

			RefreshConsole();
		}



		public void Show ()
		{
			isShown	=	true;
		}


		public void Hide ()
		{
			isShown = false;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				Game.GraphicsDevice.DisplayBoundsChanged -= GraphicsDevice_DisplayBoundsChanged;
				LogRecorder.TraceRecorded -= TraceRecorder_TraceRecorded;
				Game.Keyboard.KeyDown -= Keyboard_KeyDown;
				Game.Keyboard.FormKeyPress -= Keyboard_FormKeyPress;
				Game.Keyboard.FormKeyDown -= Keyboard_FormKeyDown;

				SafeDispose( ref consoleLayer );
				SafeDispose( ref editLayer );
			}

			base.Dispose( disposing );
		}


		void DrawString ( SpriteLayer layer, int x, int y, string text, Color color )
		{
			#if USE_PROFONT
			consoleFont.DrawString( layer, text, x,y + consoleFont.BaseLine, color );
			#else
			layer.DrawDebugString( consoleFont, x, y, text, color );
			#endif
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			var vp		=	Game.GraphicsDevice.DisplayBounds;

			RefreshConsoleLayer();

			if (isShown) {
				showFactor = MathUtil.Clamp( showFactor + FallSpeed * gameTime.ElapsedSec, 0,1 );
			} else {															   
				showFactor = MathUtil.Clamp( showFactor - FallSpeed * gameTime.ElapsedSec, 0,1 );
			}

			consoleLayer.Visible	=	showFactor > 0;

			//Log.Message("{0} {1}", showFactor, Show);
			float offset	=	(int)(- (vp.Height / 2+1) * (1 - showFactor));

			consoleLayer.SetTransform( new Vector2(0, offset), Vector2.Zero, 0 );
			editLayer.SetTransform( 0, vp.Height/2 - charHeight );

			Color cursorColor = CmdLineColor;
			cursorColor.A = (byte)( cursorColor.A * (0.5 + 0.5 * Math.Cos( 2 * CursorBlinkRate * Math.PI * gameTime.Total.TotalSeconds ) > 0.5 ? 1 : 0 ) );

			editLayer.Clear();

			//consoleFont.DrawString( editLayer, "]" + editBox.Text, 0,0, Config.CmdLineColor );
			//consoleFont.DrawString( editLayer, "_", charWidth + charWidth * editBox.Cursor, 0, cursorColor );
			DrawString( editLayer, 0, 0,										"]" + editBox.Text, CmdLineColor );
			DrawString( editLayer, charWidth + charWidth * editBox.Cursor,	0,  "_", cursorColor );


			var version = Game.GetReleaseInfo();
			DrawString( editLayer, vp.Width - charWidth * version.Length, -charHeight, version, VersionColor);

			var frameRate = string.Format("fps = {0,7:0.00}", gameTime.Fps);
			DrawString( editLayer, vp.Width - charWidth * frameRate.Length, 0, frameRate, VersionColor);

			
			//
			//	Draw suggestions :
			//	
			if (isShown && suggestion!=null && suggestion.Candidates.Any()) {

				var candidates = suggestion.Candidates;

				var x = 0;
				var y = charHeight+1;
				var w = (candidates.Max( s => s.Length ) + 2) * charWidth;
				var h = (candidates.Count() + 1) * charHeight;

				w = Math.Max( w, charWidth * 16 );

				editLayer.Draw( null, x, y, w, h, BackColor );

				int line = 0;
				foreach (var candidate in candidates ) {
					DrawString( editLayer, x + charWidth, y + charHeight * line, candidate, HelpColor );
					line ++;
				}
			}
		}



		/// <summary>
		/// Refreshes edit box.
		/// </summary>
		void RefreshEdit ()
		{
		}


		bool dirty = true;


		void RefreshConsoleLayer ()
		{
			if (!dirty) {
				return;
			}

			var vp		=	Game.GraphicsDevice.DisplayBounds;

			int cols	=	vp.Width / charWidth;
			int rows	=	vp.Height / charHeight / 2;

			int count = 1;

			consoleLayer.Clear();

			//	add small gap below command line...
			consoleLayer.Draw( null, 0,0, vp.Width, vp.Height/2+1, BackColor );

			var lines	=	LogRecorder.GetLines();

			scroll	=	MathUtil.Clamp( scroll, 0, lines.Count() );

			/*var info = Game.GetReleaseInfo();
			consoleFont.DrawString( consoleLayer, info, vp.Width - consoleFont.MeasureString(info).Width, vp.Height/2 - 1 * charHeight, ErrorColor );*/


			foreach ( var line in lines.Reverse().Skip(scroll) ) {

				Color color = Color.Gray;

				switch (line.MessageType) {
					case LogMessageType.Information : color = MessageColor; break;
					case LogMessageType.Error		: color = ErrorColor;   break;
					case LogMessageType.Warning		: color = WarningColor; break;
					case LogMessageType.Verbose		: color = VerboseColor; break;
					case LogMessageType.Debug		: color = DebugColor;   break;
				}
				

				DrawString( consoleLayer, 0, vp.Height/2 - (count+2) * charHeight, line.MessageText, color );
				//consoleFont.DrawString( consoleLayer, line.Message, , color );

				if (count>rows) {
					break;
				}

				count++;
			}

			dirty = false;
		}


		/// <summary>
		/// Refreshes console layer.
		/// </summary>
		void RefreshConsole ()
		{
			dirty	=	true;
		}




		void ExecCmd ()
		{
			try {
				var cmd  = editBox.Text;
				Log.Message("]{0}", cmd);
				Game.Invoker.Push(cmd);
			} catch ( Exception e ) {
				Log.Error(e.Message);
			}
		}


		string AutoComplete ()
		{
			var sw = new Stopwatch();
			sw.Start();
			suggestion = Game.Invoker.AutoComplete( editBox.Text );
			sw.Stop();

			if (suggestion.Candidates.Any()) {
				suggestion.Add("");
				suggestion.Add(string.Format("({0} ms)", sw.Elapsed.TotalMilliseconds));
			}

			return suggestion.CommandLine;
		}



		void TabCmd ()
		{
			editBox.Text = AutoComplete();
		}



		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			//if (e.Key==Keys.OemTilde) {
			//	Show = !Show;
			//	return;
			//}
		}


		void Keyboard_FormKeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.OemTilde) {
				isShown = !isShown;
				return;
			}
			if (!isShown) {
				return;
			}
			switch (e.Key) {
				case Keys.End		: editBox.Move(int.MaxValue/2); break;
				case Keys.Home		: editBox.Move(int.MinValue/2); break;
				case Keys.Left		: editBox.Move(-1); break;
				case Keys.Right		: editBox.Move( 1); break;
				case Keys.Delete	: editBox.Delete(); break;
				case Keys.Up		: editBox.Prev(); break;
				case Keys.Down		: editBox.Next(); break;
				case Keys.PageUp	: scroll += 2; dirty = true; break;
				case Keys.PageDown	: scroll -= 2; dirty = true; break;
			}

			RefreshEdit();
		}

		
		const char Tilde = (char)'`';
		const char Backspace = (char)8;
		const char Enter = (char)13;
		const char Escape = (char)27;
		const char Tab = (char)9;


		void Keyboard_FormKeyPress ( object sender, KeyPressArgs e )
		{
			if (!isShown) {
				return;
			}
			switch (e.KeyChar) {
				case Tilde		: break;
				case Backspace	: editBox.Backspace(); break;
				case Enter		: ExecCmd(); editBox.Enter(); break;
				case Escape		: break;
				case Tab		: TabCmd(); break;
				default			: editBox.TypeChar( e.KeyChar ); break;
			}

			// Run AutoComplete twice on TAB for better results :
			AutoComplete();

			RefreshEdit();
		}


		void TraceRecorder_TraceRecorded ( object sender, EventArgs e )
		{
			RefreshConsole();
			scroll	=	0;
		}


		void GraphicsDevice_DisplayBoundsChanged ( object sender, EventArgs e )
		{
			RefreshConsole();
		}
	}
}

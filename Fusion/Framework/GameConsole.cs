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

namespace Fusion.Framework {
	
	public class GameConsole : GameModule {

		//readonly Game Game;
		[Config]
		public GameConsoleConfig Config { get; set; }


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


		//SpriteFont	consoleFont;
		DiscTexture consoleBackground;
		DiscTexture	consoleFont;
		SpriteLayer consoleLayer;
		SpriteLayer editLayer;
		

		float showFactor = 0;
		string font;
		string conback;

		EditBox	editBox;


		int scroll = 0;

		/// <summary>
		/// Show/Hide console.
		/// </summary>
		public bool Show { get; set; }





		/// <summary>
		/// 
		/// </summary>
		/// <param name="Game"></param>
		/// <param name="font">Font texture. Must be 128x128.</param>
		/// <param name="conback">Console background texture</param>
		/// <param name="speed">Console fall speed</param>
		public GameConsole ( Game Game, string font, string conback ) : base(Game)
		{
			Config			=	new GameConsoleConfig();
			
			this.font		=	font;
			this.conback	=	conback;

			editBox		=	new EditBox(this);
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			editBox.FeedHistory( Config.GetHistory() );

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


		int charHeight { get { return 9; } }
		int charWidth { get { return 8; } }
		/*int charHeight { get { return consoleFont.LineHeight; } }
		int charWidth { get { return consoleFont.SpaceWidth; } }*/


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
			consoleFont			=	Game.Content.Load<DiscTexture>(font);
			consoleBackground	=	Game.Content.Load<DiscTexture>(conback);

			RefreshConsole();
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



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			var vp		=	Game.GraphicsDevice.DisplayBounds;

			RefreshConsoleLayer();

			if (Show) {
				showFactor = MathUtil.Clamp( showFactor + Config.FallSpeed * gameTime.ElapsedSec, 0,1 );
			} else {															   
				showFactor = MathUtil.Clamp( showFactor - Config.FallSpeed * gameTime.ElapsedSec, 0,1 );
			}

			//Log.Message("{0} {1}", showFactor, Show);
			float offset	=	(int)(- (vp.Height / 2+1) * (1 - showFactor));

			consoleLayer.SetTransform( new Vector2(0, offset), Vector2.Zero, 0 );
			editLayer.SetTransform( 0, vp.Height/2 - 8 );

			Color cursorColor = Config.CmdLineColor;
			cursorColor.A = (byte)( cursorColor.A * (0.5 + 0.5 * Math.Cos( 2 * Config.CursorBlinkRate * Math.PI * gameTime.Total.TotalSeconds ) > 0.5 ? 1 : 0 ) );

			editLayer.Clear();

			//consoleFont.DrawString( editLayer, "]" + editBox.Text, 0,0, Config.CmdLineColor );
			//consoleFont.DrawString( editLayer, "_", charWidth + charWidth * editBox.Cursor, 0, cursorColor );
			editLayer.DrawDebugString( consoleFont, 0, 0,										"]" + editBox.Text, Config.CmdLineColor );
			editLayer.DrawDebugString( consoleFont, charWidth + charWidth * editBox.Cursor,	0,  "_", cursorColor );


			var version = Game.GetReleaseInfo();
			editLayer.DrawDebugString( consoleFont, vp.Width - charWidth * version.Length, -charHeight, version, Config.VersionColor);

			var frameRate = string.Format("fps = {0,7:0.00}", gameTime.Fps);
			editLayer.DrawDebugString( consoleFont, vp.Width - charWidth * frameRate.Length, 0, frameRate, Config.VersionColor);
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
			consoleLayer.Draw( consoleBackground, 0,0, vp.Width, vp.Height/2+1, Color.White );

			var lines	=	LogRecorder.GetLines();

			scroll	=	MathUtil.Clamp( scroll, 0, lines.Count() );

			/*var info = Game.GetReleaseInfo();
			consoleFont.DrawString( consoleLayer, info, vp.Width - consoleFont.MeasureString(info).Width, vp.Height/2 - 1 * charHeight, ErrorColor );*/


			foreach ( var line in lines.Reverse().Skip(scroll) ) {

				Color color = Color.Gray;

				switch (line.MessageType) {
					case LogMessageType.Information : color = Config.MessageColor; break;
					case LogMessageType.Error		: color = Config.ErrorColor;   break;
					case LogMessageType.Warning		: color = Config.WarningColor; break;
				}
				

				consoleLayer.DrawDebugString( consoleFont, 0, vp.Height/2 - (count+2) * charHeight, line.MessageText, color );
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



		void TabCmd ()
		{
			editBox.Text = Game.Invoker.AutoComplete( editBox.Text );
		}



		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.OemTilde) {
				Show = !Show;
				return;
			}
		}


		void Keyboard_FormKeyDown ( object sender, KeyEventArgs e )
		{
			if (!Show) {
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

		

		const char Backspace = (char)8;
		const char Enter = (char)13;
		const char Escape = (char)27;
		const char Tab = (char)9;


		void Keyboard_FormKeyPress ( object sender, KeyPressArgs e )
		{
			if (!Show) {
				return;
			}
			if (e.KeyChar=='`') {
				return;
			}
			switch (e.KeyChar) {
				case Backspace	: editBox.Backspace(); break;
				case Enter		: ExecCmd(); editBox.Enter(); break;
				case Escape		: break;
				case Tab		: TabCmd(); break;
				default			: editBox.TypeChar( e.KeyChar ); break;
			}

			//Log.Message("{0}", (int)e.KeyChar);

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

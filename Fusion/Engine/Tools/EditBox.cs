using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;

namespace Fusion.Engine.Tools {
	internal class EditBox {

		GameConsole console;
		StringBuilder text;
		int cursor;

		List<string> history = new List<string>();
		int historyCursor = -1;


		public string Text {
			get {
				return text.ToString();
			}
			set {
				text.Clear();
				text.Append( value );
				cursor = text.Length;
			}
		}


		public int Cursor {
			get {
				return cursor;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		public EditBox ( GameConsole console )
		{
			this.console	=	console;
			text			=	new StringBuilder();
			cursor			=	0;

			history	=	new List<string>( console.GetHistory() );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="history"></param>
		public void FeedHistory ( string[] history )
		{
			this.history.AddRange(history);
			historyCursor = -1;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ch"></param>
		public void TypeChar ( char ch )
		{						
			if (cursor>=text.Length) {
				text.Append(ch);
				cursor++;
			} else {
				text.Insert( cursor, ch );
				cursor++;
			}
		}


		public void Enter ()
		{
			if (Text!="" /*&& history.FirstOrDefault()!=Text*/) {
				history.Insert( 0, Text );
				console.UpdateHistory( history );
				historyCursor = -1;
				Text = "";
			}
		}


		public void Tab ()
		{
		}


		public void Backspace ()
		{
			if (cursor>0) {
				text.Remove(cursor-1,1);
				cursor--;
			}
		}


		public void Delete ()
		{
			if (cursor<text.Length) {
				text.Remove(cursor,1);
			}
		}



		public void Move ( int value )
		{
			cursor += value;
			cursor = MathUtil.Clamp(cursor, 0, text.Length);
		}



		public void Prev ()
		{
			if (!history.Any()) {
				return;
			}
			historyCursor = MathUtil.Clamp( historyCursor + 1, 0, history.Count - 1 );
			Text = history[ historyCursor ];
		}


		public void Next ()
		{
			if (!history.Any()) {
				return;
			}
			historyCursor = MathUtil.Clamp( historyCursor - 1, 0, history.Count - 1 );
			Text = history[ historyCursor ];
		}

	}
}

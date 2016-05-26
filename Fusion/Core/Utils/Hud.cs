using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using System.Threading;

namespace Fusion {

	internal class HudString {
		public readonly Color	Color;
		public readonly Guid	Category;
		public readonly string	Text;

		internal HudString ( Guid category, Color color, string text )
		{
			this.Color		=	color;
			this.Category	=	category;
			this.Text		=	text;
		}
	}


	/// <summary>
	/// Represents class to monitor everything.
	/// </summary>
	internal static class Hud {

		static object lockObj = new object();

		static List<HudString>	lines = new List<HudString>();


		/// <summary>
		/// Clears all lines added from current thread.
		/// </summary>
		static public void Clear ( Guid category )
		{
			lock (lockObj) {
				lines.RemoveAll( line => line.Category == category );
			}
		}

		
		/// <summary>
		/// Adds line from current thread.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="text"></param>
		static public void Add ( Guid category, Color color, string text )
		{
			lock (lockObj) {
				lines.Add( new HudString( category, color, text ) );
			}
		}



		/// <summary>
		/// Adds line from current thread.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="text"></param>
		static public void Add ( Guid category, Color color, string format, params object[] args )
		{
			Add( category, color, string.Format(format, args) );
		}



		/// <summary>
		/// Gets lines.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<HudString> GetLines ()
		{
			lock (lockObj) {
				return lines.OrderBy( line => line.Category ).ToArray();
			}
		}
	}
}

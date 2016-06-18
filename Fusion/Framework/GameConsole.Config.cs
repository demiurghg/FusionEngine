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
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;
using Fusion.Core.Configuration;

namespace Fusion.Framework {
	
	public partial class GameConsole : GameModule {

		[Config] public float FallSpeed { get; set; }

		[Config] public float CursorBlinkRate { get; set; }

		[Config] public Color MessageColor	{ get; set; }
		[Config] public Color ErrorColor		{ get; set; }
		[Config] public Color WarningColor	{ get; set; }
		[Config] public Color CmdLineColor	{ get; set; }
		[Config] public Color VersionColor	{ get; set; }

		[Config] public Color BackColor		{ get; set; }
		[Config] public Color HelpColor		{ get; set; }
		[Config] public Color HintColor		{ get; set; }


		[Config] public string CommandHistory0 { get; set; }
		[Config] public string CommandHistory1 { get; set; }
		[Config] public string CommandHistory2 { get; set; }
		[Config] public string CommandHistory3 { get; set; }
		[Config] public string CommandHistory4 { get; set; }
		[Config] public string CommandHistory5 { get; set; }
		[Config] public string CommandHistory6 { get; set; }
		[Config] public string CommandHistory7 { get; set; }


		void SetDefaults ()
		{
			FallSpeed		=	5;
			
			CursorBlinkRate	=	3;
			
			MessageColor	=	Color.White;
			ErrorColor		=	Color.Red;
			WarningColor	=	Color.Yellow;
			CmdLineColor	=	Color.Orange;
			VersionColor	=	new Color(255,255,255,64);

			BackColor		=	new Color(0,0,0,224);
			HelpColor		=	Color.Gray;
			HintColor		=	new Color(255,255,255,64);

			CommandHistory0	=	"";
			CommandHistory1	=	"";
			CommandHistory2	=	"";
			CommandHistory3	=	"";
			CommandHistory4	=	"";
			CommandHistory5	=	"";
			CommandHistory6	=	"";
			CommandHistory7	=	"";
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="command"></param>
		internal void UpdateHistory ( IEnumerable<string> commands )
		{
			var list = commands
					.Where(s=>!s.StartsWith("quit"))
					.Take(8)
					.ToArray();

			CommandHistory0	=	( list.Length > 0 ) ? list[0] : "";
			CommandHistory1	=	( list.Length > 1 ) ? list[1] : "";
			CommandHistory2	=	( list.Length > 2 ) ? list[2] : "";
			CommandHistory3	=	( list.Length > 3 ) ? list[3] : "";
			CommandHistory4	=	( list.Length > 4 ) ? list[4] : "";
			CommandHistory5	=	( list.Length > 5 ) ? list[5] : "";
			CommandHistory6	=	( list.Length > 6 ) ? list[6] : "";
			CommandHistory7	=	( list.Length > 7 ) ? list[7] : "";
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] GetHistory ()
		{
			return new[]{ 
					CommandHistory0, CommandHistory1, 
					CommandHistory2, CommandHistory3, 
					CommandHistory4, CommandHistory5, 
					CommandHistory6, CommandHistory6 
				}
				.Where( s => !string.IsNullOrWhiteSpace(s) )
				.ToArray();
		}
	}
}

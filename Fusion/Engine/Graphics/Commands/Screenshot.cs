using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;

namespace Fusion.Engine.Graphics.Commands {
	[Command("screenshot", CommandAffinity.Default)]
	public class Screenshot : NoRollbackCommand {


		[CommandLineParser.Name("path")]
		public string Path { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public Screenshot ( Invoker invoker ) : base(invoker) 
		{
			Path	=	null;
		}


		public override void Execute ()
		{
			Game.GraphicsEngine.Screenshot(Path);
		}
	}
}

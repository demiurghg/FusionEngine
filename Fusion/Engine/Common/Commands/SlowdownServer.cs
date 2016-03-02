using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;
using System.Diagnostics;

namespace Fusion.Engine.Common.Commands {

	[Command("slowServer", CommandAffinity.Server)]
	public class SlowdownServer : NoRollbackCommand {

		static int slowFrames = 0;
		static int delay;


		[CommandLineParser.Required()]
		[CommandLineParser.Name("delay")]
		public int Delay { get; set; }


		[CommandLineParser.Required()]
		[CommandLineParser.Name("frames")]
		public int Frames { get; set; }


		public static void SlowTest ()
		{
			if (slowFrames>0) {
				slowFrames --;

				var sw = new Stopwatch();
				var r = new Random();
				sw.Start();

				while(sw.ElapsedMilliseconds<delay) {
					r.Next();
				}

				sw.Stop();

				Log.Message("*SLOW SERVER TEST [{0}]*", slowFrames);
			}
		}

			
		public SlowdownServer ( Invoker invoker ) : base(invoker) 
		{
		}


		public override void Execute ()
		{
			slowFrames	=	Frames;
			delay		=	Delay;
		}
	}
}

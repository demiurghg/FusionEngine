using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Shell {

	public enum TestRequiredEnum {
		Copy,
		Replace,
		Move,
		Delete,
		Link,
		LinkTwice,
		LinkOnce,
	}

	public enum TestOptionsEnum {
		Incremental,
		Cleanup,
	}

	[Command("test-command", CommandAffinity.Default)]
	public class TestCommand : Command {

		//[CommandLineParser.Name("force")]
		//public bool Force { get; set; }

		[CommandLineParser.Required]
		[CommandLineParser.Name("action")]
		public TestRequiredEnum Action { get; set; }

		[CommandLineParser.Name("option")]
		public TestRequiredEnum Option { get; set; }

		[CommandLineParser.Name("optionTeSt")]
		public TestRequiredEnum OptionQQQQ { get; set; }

		[CommandLineParser.Name("optionTeXt")]
		public TestRequiredEnum OptionWWWW { get; set; }

		//[CommandLineParser.Name("num")]
		//public List<int> Numbers { get; set; }

		//[CommandLineParser.Name("project")]
		//public string Project { get; set; }

		//[CommandLineParser.Required]
		//[CommandLineParser.Name("output")]
		//public string Output { get; set; }

		[CommandLineParser.Required]
		[CommandLineParser.Name("input")]
		public List<string> Input { get; set; }

		[CommandLineParser.Name("aabbbccc")]
		public bool aabbbccc { get; set; }

		[CommandLineParser.Name("aaaabbbccc")]
		public bool aaaabbbccc { get; set; }

		[CommandLineParser.Name("aabbbbbbccc")]
		public bool aabbbbbbccc { get; set; }

		public TestCommand(Invoker invoker) : base(invoker)
		{
			//Numbers	=	new List<int>();
			Input = new List<string>();
		}

		public override void Execute ()
		{
			Log.Message("test-command execute");
			//Log.Message("Force   : {0}", Force);
			//Log.Message("Action  : {0}", Action);
			//Log.Message("Force   : {0}", Force);
			//Log.Message("Project : {0}", Project);
			//Log.Message("Output  : {0}", Output);

			//Log.Message("Numbers : {0}", string.Join("-", Numbers.Select( n=>n.ToString() )) );
			//Log.Message("Input   : {0}", string.Join("-", Input ));
		}


		public override void Rollback ()
		{
			Log.Message("test-command rollback");
		}

	}
}

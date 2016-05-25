using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;


namespace Fusion.Core.Shell {
	public partial class Invoker {

		public class Suggestion {

			List<string> candidates = new List<string>(16);

			public string CommandLine { get; set; }

			public IEnumerable<string> Candidates { get { return candidates; } }

			public Suggestion ( string cmdline ) 
			{
				CommandLine = cmdline;
			}

			public void Set ( string cmdline )
			{
				CommandLine = cmdline;
			}

			public void Add ( string candidate ) 
			{
				candidates.Add( candidate );
			}

			public void Clear () 
			{
				candidates.Clear();
			}

			public void AddRange ( IEnumerable<string> more ) 
			{
				candidates.AddRange( more );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="candidates"></param>
		/// <param name="suggestions">Null if no suggestions</param>
		/// <returns></returns>
		public Suggestion AutoComplete ( string input )
		{
			if (string.IsNullOrWhiteSpace(input)) {
				return new Suggestion("");
			}

			var suggestion = new Suggestion(input);

			var args = CommandLineParser.SplitCommandLine( input ).ToArray();

			var cmd  =	args[0];

			var cmdList =	CommandList
							.ToList();

			var varList	=	Game.Config.Variables.Select( a => a.Value )
							.OrderBy( b => b.FullName )
							.ToList();


			string longestCommon = null;
			int count = 0;

			//
			//	search commands :
			//	
			foreach ( var name in cmdList ) {
				if (cmd.ToLower()==name.ToLower()) {
					return AutoCompleteCommand(input, args, name);
				}
				if (name.StartsWith(cmd, StringComparison.OrdinalIgnoreCase)) {
					longestCommon = LongestCommon( longestCommon, name );
					suggestion.Set( longestCommon );
					suggestion.Add( name );
					count++;
				}
			}

			//
			//	search variables :
			//	
			foreach ( var variable in varList ) {
				if (cmd.ToLower()==variable.FullName.ToLower()) {
					return AutoCompleteVariable( input, args, variable );
				}		   
				if (variable.FullName.StartsWith(cmd, StringComparison.OrdinalIgnoreCase)) {
					longestCommon = LongestCommon( longestCommon, variable.FullName );
					suggestion.Set( longestCommon );
					suggestion.Add( string.Format("{0,-30} = {1}", variable.FullName, variable.Get() ) );
					count++;
				}
			}

			if (count==1) {
				suggestion.Set( suggestion.CommandLine + " ");
			}

			return suggestion;
		}
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="suggestions"></param>
		/// <returns></returns>
		Suggestion AutoCompleteCommand ( string input, string[] args, string commandName )
		{
			var suggestion = new Suggestion(input);

			if (args.Length==1) {
				suggestion = new Suggestion(args[0] + " ");
			}

			var cmd = GetCommand(commandName);
			var parser = new CommandLineParser(cmd, commandName);

			suggestion.Add( commandName + " " + string.Join(" ", parser.RequiredUsageHelp ) );
			suggestion.Add( "" );
			suggestion.Add( "options : " );
			suggestion.AddRange( parser.OptionalUsageHelp.Select( opt => "   " + opt ) );
			
			return suggestion;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="args"></param>
		/// <param name="variable"></param>
		/// <param name="suggestions"></param>
		/// <returns></returns>
		Suggestion AutoCompleteVariable ( string input, string[] args, ConfigVariable variable )
		{
			var suggestion = new Suggestion(input);

			var type = variable.TargetProperty.PropertyType;
			var candidates = new string[0];

			//
			//	Gather possible values :
			//
			if (type==typeof(bool)) {
				candidates = new string[]{"True", "False"};
			} else if (type.IsEnum) {
				candidates = Enum.GetNames(type);
			} else {
				candidates = new string[]{variable.Get()};
			}

			//
			//	Only name of the variables is entered.
			//	Just show possible values.
			//	
			if (args.Length==1) {	
				suggestion.Set( args[0] + " ");
				suggestion.AddRange( candidates.Select( c1 => args[0] + " " + c1 ) );
				return suggestion;
			}

			//
			//	Select candidates that starts with entered value.
			//
			candidates = candidates
				.Where( c => c.StartsWith( args[1], StringComparison.OrdinalIgnoreCase) )
				.ToArray();

			var longest = LongestCommon( candidates );


			suggestion.AddRange( candidates.Select( c1 => args[0] + " " + c1 ) );

			//	add quotes if longest common contains spaces :
			if (longest!=null && longest.Any( c => char.IsWhiteSpace(c) )) {
				longest = "\"" + longest;// + "\"";
				if (candidates.Length==1) {
					//	only on suggestion - close quotes.
					longest += "\"";
				}
			} else {
			}

			suggestion.Set( string.Format("{0} {1}", args[0], longest) );

			return suggestion;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		string LongestCommon( IEnumerable<string> values )
		{
			string longest = null;
			foreach ( var value in values ) {
				longest = LongestCommon( longest, value );
			}
			return longest;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		string LongestCommon ( string a, string b )
		{
			/*if (a==null) return b;
			if (b==null) return a;*/
			if (string.IsNullOrEmpty(a)) {
				return b;
			}
			if (string.IsNullOrEmpty(b)) {
				return a;
			}

			int len = Math.Min( a.Length, b.Length );

			StringBuilder sb = new StringBuilder();

			for (int i=0; i<len; i++) {
				if (char.ToLower(a[i])==char.ToLower(b[i])) {
					sb.Append(b[i]);
				} else {
					break;
				}
			}

			return sb.ToString();
		}

	}
}

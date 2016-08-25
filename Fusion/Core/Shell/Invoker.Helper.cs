using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using System.Reflection;


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

			var suggestion	=	new Suggestion(input);
			var args		=	CommandLineParser.SplitCommandLine( input ).ToArray();
			var cmd			=	args[0];
			var cmdList		=	CommandList.ToList();
			var varDict		=	Game.Config.Variables;
			var comparison = StringComparison.OrdinalIgnoreCase;

			ConfigVariable cfgVar;


			if ( cmdList.Any( c => string.Equals(c, cmd, comparison) ) ) {

				return AutoCompleteCommand( input, args, cmd );

			} else if ( varDict.TryGetValue( cmd, out cfgVar ) ) {

				return AutoCompleteVariable( input, args, cfgVar );

			} else {
				
				cmdList.AddRange( varDict.Select( v => v.Value.FullName ).OrderBy( n=>n ) );

				var candidates		=	cmdList.ToArray();
				var longestCommon	=	LongestCommon( cmd, ref candidates );

				if (!string.IsNullOrWhiteSpace(longestCommon)) {
					if (candidates.Length<=1) {
						suggestion.CommandLine	=	longestCommon + " ";
					} else {
						suggestion.CommandLine	=	longestCommon;
					}
				}

				suggestion.AddRange( candidates );

				return suggestion;
			}
		}

		


		void AddCommandSyntax ( Suggestion suggestion, CommandLineParser parser, string commandName )
		{
			suggestion.Add( commandName 
				+ " " + string.Join(" ", parser.RequiredUsageHelp.Select( o => "<" + o.Name + ">" + (o.IsList?"[...]":"")) )
				+ " " + string.Join("", parser.OptionalUsageHelp.Select( o => "[/" + o.Value.Name + "]") ) 
				);
		}



		void DetailedCommandHelp ( Suggestion suggestion, CommandLineParser parser, string commandName )
		{
			suggestion.Add( commandName );
			
			suggestion.Add( "" );
			suggestion.Add( "required : " );

			foreach( var option in parser.RequiredUsageHelp ) {
				suggestion.Add(string.Format("   {0,-20} {1}", "<"+option.Name+">", option.Description));
			}

			
			suggestion.Add( "" );
			suggestion.Add( "options : " );

			foreach( var option in parser.OptionalUsageHelp.Select( p => p.Value ).OrderBy( n=>n.Name ) ) {
				suggestion.Add(string.Format("   /{0,-20} {1}", option.Name, option.Description));
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="suggestions"></param>
		/// <returns></returns>
		Suggestion AutoCompleteCommand ( string input, string[] args, string commandName )
		{
			var suggestion		=	new Suggestion(input);

			var cmd				=	GetCommand(commandName);
			var parser			=	GetParser(commandName);
			var numRequired		=	parser.RequiredUsageHelp.Count;
			var numOptions		=	parser.OptionalUsageHelp.Count;
			var tailingSpace	=	input.Last()==' ';

			//	add fictional empty argument for more clear suggestion:
			if (tailingSpace) {
				Array.Resize<string>( ref args, args.Length + 1 );
				args[ args.Length-1 ] = "";
			}


			//	command without tailing space - add space:
			if (args.Length==1) {
				suggestion.CommandLine = ArgsToString( args ) + " ";
				DetailedCommandHelp( suggestion, parser, commandName );
				return suggestion;
			}

			//	store last argument :
			var lastArg = args.Last();

			//	question?
			if (lastArg=="?") {
				DetailedCommandHelp( suggestion, parser, commandName );
				return suggestion;
			}


			//	add short help :
			AddCommandSyntax( suggestion, parser, commandName );

			if (lastArg.StartsWith("/")) {

				#region OPTIONS
				var name	=   lastArg.Substring(1);
				var index	=	name.IndexOf(':');	

				if (index!=-1) {
					name	=	name.Substring(0, index);
				}

				var candidates = new string[0];
				var options	=	parser.OptionalUsageHelp;
				CommandLineParser.ArgumentInfo pi;

				if ( options.TryGetValue( name, out pi ) ) {
					if (pi.ArgumentType==typeof(bool)) {
		
						suggestion.CommandLine = ArgsToString( args ) + " ";
						return suggestion;
		
					} else {
		
						if (index==-1) {
							suggestion.CommandLine = ArgsToString( args ) + ":";
							return suggestion;
						} else {
							var value = lastArg.Substring(index+2);
							candidates = cmd.Suggest( pi.ArgumentType, name ).ToArray();
							value = LongestCommon( value, ref candidates );
							suggestion.AddRange( candidates );
							suggestion.CommandLine	= ArgsToString( args, "/" + name + ":" + value );
							return suggestion;
						}
					}
				}
				
				candidates = options
					.Select( p => "/" + p.Key )
					.OrderBy( n => n ).ToArray();

				lastArg = LongestCommon( lastArg, ref candidates );

				suggestion.AddRange(candidates);

				suggestion.CommandLine	= ArgsToString( args, lastArg );
				#endregion

			} else {

				#region REQUIRED
				var candidates		=	new string[0];
				int currentIndex	=	Math.Max( 0, args.Skip(1).Count( arg => !arg.StartsWith("/") ) ) - 1;

				if (currentIndex < numRequired) {
					
					var pi		=	parser.RequiredUsageHelp[currentIndex];

					var name	=	pi.Name;
					var type	=	pi.ArgumentType;

					candidates	=	cmd.Suggest( type, name ).ToArray();

					var longest	=	LongestCommon( lastArg, ref candidates );

					suggestion.AddRange(candidates);

					var postFix	=	(lastArg=="" || candidates.Length==1) ? " " : "";

					suggestion.CommandLine	= ArgsToString( args, longest ?? lastArg ) + postFix;

				} 
				#endregion

			}
			
			return suggestion;
		}



		/// <summary>
		///		
		/// </summary>
		/// <param name="args"></param>
		/// <param name="lastArg"></param>
		/// <returns></returns>
		string ArgsToString ( string[] args, string lastArg = null )
		{
			if (lastArg!=null && args.Length>1 ) {
				args[ args.Length-1 ] = lastArg;
			}
			return string.Join( " ", args.Select( arg => arg.Any( ch=> char.IsWhiteSpace(ch) ) ? "\"" + arg + "\"" : arg ) );
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
			var longest = LongestCommon( args[1], ref candidates );


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
		/// <param name="input"></param>
		/// <param name="candidates"></param>
		/// <returns></returns>
		string LongestCommon ( string input, ref string[] candidates )
		{
			candidates = candidates
				.Where( c => c.StartsWith( input, StringComparison.OrdinalIgnoreCase) )
				.ToArray();

			var longest	=	LongestCommon( candidates );
			
			return longest;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		string LongestCommon( IEnumerable<string> values )
		{
			return longestCommonPrefix( values.ToArray() );
			//string longest = null;
			//foreach ( var value in values ) {
			//	longest = LongestCommon( longest, value );
			//}
			//return longest;
		}


		/// <summary>
		/// http://stackoverflow.com/questions/8578349/longest-common-prefix-for-n-string
		/// </summary>
		/// <param name="strs"></param>
		/// <returns></returns>
		public String longestCommonPrefix(String[] strs) {
			if(strs.Length==0) return null;
			String minStr=strs[0];

			for(int i=1;i<strs.Length;i++){
				if(strs[i].Length<minStr.Length)
					minStr=strs[i];
			}
			int end=minStr.Length;
			for(int i=0;i<strs.Length;i++){
				int j;
				for( j=0;j<end;j++){
					if(char.ToLowerInvariant(minStr[j])!=char.ToLowerInvariant(strs[i][j]))
						break;
				}
				if(j<end)
					end=j;
			}
			return minStr.Substring(0,end);
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

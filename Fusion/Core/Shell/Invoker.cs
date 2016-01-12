﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;


namespace Fusion.Core.Shell {
	public partial class Invoker {

		/// <summary>
		/// Game reference.
		/// </summary>
		public Game Game { get; private set; }


		/// <summary>
		/// Invoker's context object to target invoker and commands to particular object.
		/// </summary>
		public object Context { get; private set; }

		Dictionary<string, Type> commands;
		Dictionary<string, ConfigVariable> variables;

		object lockObject = new object();

		Queue<Command> queue	= new Queue<Command>(10000);
		Queue<Command> delayed	= new Queue<Command>(10000);
		Stack<Command> history	= new Stack<Command>(10000);


		/// <summary>
		/// Alphabetically sorted array of command names
		/// </summary>
		public string[] CommandList { get; private set; }

		/// <summary>
		/// Gets dictionary of all available variables.
		/// </summary>
		internal Dictionary<string, ConfigVariable> Variables {
			get {
				return variables;
			}
		}



		/// <summary>
		/// Creates instance of Invoker.
		/// </summary>
		/// <param name="game">Game instance</param>
		public Invoker ( Game game, CommandAffinity affinity )
		{
			Initialize( game, Command.GatherCommands() );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="types"></param>
		void Initialize ( Game game, Type[] types )
		{
			Context		=	null;
			Game		=	game;
			commands	=	types
						.Where( t1 => t1.IsSubclassOf(typeof(Command)) )
						.Where( t2 => t2.HasAttribute<CommandAttribute>() )
						.ToDictionary( t3 => t3.GetCustomAttribute<CommandAttribute>().Name );

			CommandList	=	commands.Select( cmd => cmd.Key ).OrderBy( name => name ).ToArray();
						
			Log.Message("Invoker: {0} commands found", commands.Count);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="name"></param>
		internal void FeedConfigs ( IEnumerable<ConfigVariable> variables )
		{
			lock (lockObject) {
				this.variables	=	variables.ToDictionary( var1 => var1.Prefix + "." + var1.Name );
			}
		}



		/// <summary>
		/// Parses and pushes command to the queue.
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>
		public Command Push ( string commandLine )
		{				  
			var argList	=	CommandLineParser.SplitCommandLine( commandLine ).ToArray();

			if (!argList.Any()) {
				throw new CommandLineParserException("Empty command line.");
			} 



			var cmdName	=	argList[0];
			argList		=	argList.Skip(1).ToArray();
			

			lock (lockObject) {

				ConfigVariable variable;

				if (Variables.TryGetValue( cmdName, out variable )) {
					if (argList.Count()==0) {
						Log.Message("{0} = {1}", variable.FullName, variable.Get() );
						return null;
					} else {
						return Push( string.Format("set {0} \"{1}\"", cmdName, string.Join(" ", argList) ) );
					}
				}
				var command	=	GetCommand( cmdName );

				var parser	=	new CommandLineParser( command, cmdName );

				if (!parser.ParseCommandLine( argList )) {
					throw new CommandLineParserException("Failed to parse command line");
				}

				Push( command );

				return command;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public object PushAndExecute ( string commandLine )
		{
			var cmd = Push( commandLine );
			ExecuteQueue( new GameTime(), CommandAffinity.Default, true );
			return cmd.Result;
		}

		

		/// <summary>
		/// Parse given string and push parsed command to queue.
		/// </summary>
		/// <param name="command"></param>
		void Push ( Command command )
		{
			lock (lockObject) {
				if (queue.Any() && queue.Last().Terminal) {
					Log.Warning("Attempt to push command after terminal one. Ignored.");
					return;
				}
				queue.Enqueue( command );
			}
		}





		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal Command GetCommand ( string name )
		{
			Type cmdType;

			if (commands.TryGetValue( name, out cmdType )) {
				return (Command)Activator.CreateInstance( cmdType, this );
			}
			
			throw new InvalidOperationException(string.Format("Unknown command '{0}'.", name));
		}



		/// <summary>
		/// Executes enqueued commands. Updates delayed commands.
		/// </summary>
		/// <param name="gameTime"></param>
		public void ExecuteQueue ( GameTime gameTime, CommandAffinity affinity, bool forceDelayed = false )
		{
			var delta = (int)gameTime.Elapsed.TotalMilliseconds;

			lock (lockObject) {

				delayed.Clear();

				while (queue.Any()) {
					
					var cmd = queue.Dequeue();

					if ( cmd.Affinity == affinity ) {

						if ( cmd.Delay<=0 || forceDelayed ) {
							//	execute :
							cmd.Execute();

							if (cmd.Result!=null) {
								Log.Message( "// Result: {0} //", cmd.GetStringResult() );
							}

							//	push to history :
							if (!cmd.NoRollback && cmd.Affinity==CommandAffinity.Default) {
								history.Push( cmd );
							}

						} else {

							cmd.Delay -= delta;

							delayed.Enqueue( cmd );

						}

					} else {
						
						delayed.Enqueue( cmd );

					}

				}

				Misc.Swap( ref delayed, ref queue );
			}
		}



		/// <summary>
		/// Undo one command.
		/// </summary>
		public void Undo ()
		{
			lock (lockObject) {

				if (!history.Any()) {
					throw new Exception("No more commands to undo");
				}

				var cmd = history.Pop();
				cmd.Rollback();
			}
		}



		/// <summary>
		/// Purges all history.
		/// </summary>
		public void PurgeHistory ()
		{
			lock (lockObject) {
				history.Clear();
			}
		}
	}
}

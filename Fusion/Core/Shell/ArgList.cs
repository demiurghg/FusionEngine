using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;


namespace Fusion.Core.Shell {
	public class ArgList : IEnumerable {

		public const int BadIndex = -1;

		string[] args;
		readonly string switchPrefix;


		/// <summary>
		/// Creates instance of ArgList
		/// </summary>
		/// <param name="commandLine"></param>
		public ArgList ( string commandLine )
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Creates instance of ArgList
		/// </summary>
		/// <param name="args"></param>
		public ArgList ( string[] args, string switchPrefix = "/" )
		{
			this.switchPrefix	=	switchPrefix;
			this.args			=	args.ToArray();
		}



		/// <summary>
		/// Gets string argument at given position.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public string this [int index] 
		{
			get {
				return args[index];
			}
		}






		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		bool GetOption<T> ( string prefix, out T value )
		{
			if (string.IsNullOrWhiteSpace(prefix)) {
				throw new ArgumentNullException("prefix is null or empty");
			}
			if (!prefix.StartsWith("/")) {
				
			}

			if (typeof(T)==typeof(bool)) {
				
			}
			foreach ( var arg in args ) {
				if (arg.StartsWith(prefix))
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="shortSwitch"></param>
		/// <param name="?"></param>
		/// <returns></returns>
		public int IndexOf( string shortSwitch, string niceSwitch = null )
		{
			if (string.IsNullOrWhiteSpace(shortSwitch)) {
				throw new ArgumentNullException("shortSwitch is null or empty");
			}
			if (!shortSwitch.StartsWith("-")) {
				throw new ArgumentException("shortSwitch should start with '-'");
			}
			if (niceSwitch!=null && !niceSwitch.StartsWith("-")) {
				throw new ArgumentException("shortSwitch should start with '-'");
			}

			var comparison = StringComparison.InvariantCultureIgnoreCase;

			for (int i=0; i<args.Length; i++) {
				if ( args[i].Equals( shortSwitch, comparison ) || args[i].Equals( niceSwitch, comparison ) ) {
					return i;
				}
			}

			return BadIndex;
		}



		/// <summary>
		/// Converts argument to specified type.
		/// Supported types similar to StringConverter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="index"></param>
		/// <returns></returns>
		public T As<T>( int index ) 
		{
			return (T)StringConverter.ConvertFromString( typeof(T), args[index] );
		}



		/// <summary>
		/// Gets enumerator
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
		   return (IEnumerator) GetEnumerator();
		}

		/// <summary>
		/// Gets enumerator
		/// </summary>
		/// <returns></returns>
		public ArgEnumerator GetEnumerator()
		{
			return new ArgEnumerator(args);
		}


		/// <summary>
		/// Enumerator class
		/// </summary>
		public class ArgEnumerator : IEnumerator, IEnumerator<string>
		{
			public string[] args;

			// Enumerators are positioned before the first element
			// until the first MoveNext() call.
			int position = -1;

			public ArgEnumerator(string[] args)
			{
				this.args = args;
			}

			public bool MoveNext()
			{
				position++;
				return (position < args.Length);
			}

			public void Reset()
			{
				position = -1;
			}

			object IEnumerator.Current {
				get {
					return Current;
				}
			}

			public string Current {
				get {
					try	{
						return args[position];
					}
					catch (IndexOutOfRangeException) {
						throw new InvalidOperationException();
					}
				}
			}

			public void Dispose ()
			{
			}
		}
	}
}

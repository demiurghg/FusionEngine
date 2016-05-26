using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Core.Extensions;
using Lidgren.Network;

namespace Fusion.Engine.Common {

	/// <summary>
	/// Represents distributed/remote atoms colleciton.
	/// </summary>
	public class AtomCollection {

		const short MaxAtomIndex = short.MaxValue;

		bool locked = false;

		Dictionary<string,short> dictionary;
		List<string> index;

		readonly object	lockObj = new object();


		/// <summary>
		/// Creates instance of AtomCollection.
		/// </summary>
		public AtomCollection ()
		{
			dictionary	=	new Dictionary<string,short>();
			index		=	new List<string>();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		internal AtomCollection ( BinaryReader reader )
		{
			dictionary	=	new Dictionary<string,short>();
			index		=	new List<string>();

			Read( reader );
			Lock();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		internal AtomCollection ( NetIncomingMessage message )
		{
			dictionary	=	new Dictionary<string,short>();
			index		=	new List<string>();

			Read( message );
			Lock();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="atom"></param>
		/// <returns></returns>
		public short Add ( string atom )
		{
			lock (lockObj) {
				if (locked) {
					throw new InvalidOperationException("Atom collection is locked");
				}

				if (index.Count>=MaxAtomIndex) {
					throw new InvalidOperationException("Too much atoms");
				}

				if (dictionary.ContainsKey(atom)) {
					return dictionary[atom];
				}

				short id = (short)index.Count;
				index.Add( atom );
				dictionary.Add( atom, id );

				return id;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="atom"></param>
		/// <returns></returns>
		public void Clear ()
		{
			lock (lockObj) {
				if (locked) {
					throw new InvalidOperationException("Atom collection is locked");
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns>Null if index is bad.</returns>
		public string this[ short id ] {
			get {
				if (id<0 || id>=index.Count) {
					return null;
				}
				return index[id];
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="atom"></param>
		/// <returns>Negative value, if atom does not exist.</returns>
		public short this[ string atom ] {
			get {
				short id;
				if (dictionary.TryGetValue(atom, out id)) {
					return id;
				} else {
					return -1;
				}
			}
		} 



		/// <summary>
		/// Locks table.
		/// </summary>
		internal void Lock ()
		{
			locked = true;
		}



		/// <summary>
		/// Writes table to outgoing message
		/// </summary>
		/// <param name="message"></param>
		internal void Write ( NetOutgoingMessage message )
		{
			message.Write( index.Count );

			for ( short i=0; i<index.Count; i++ ) {
				message.Write( i );
				message.Write( index[i] );
			}
		}



		/// <summary>
		/// Read table from incoming message.
		/// </summary>
		/// <param name="message"></param>
		internal void Read ( NetIncomingMessage message )
		{
			//	count:
			int count	=	message.ReadInt32();

			for ( short i=0; i<count; i++) {
				short idA = message.ReadInt16();

				short idB = Add( message.ReadString() );

				if (idA!=idB) {
					throw new IOException("Bad ATOM table.");
				}
			}
		}



		/// <summary>
		/// Writes collection using binary writer.
		/// </summary>
		/// <param name="writer"></param>
		internal void Write ( BinaryWriter writer )
		{
			writer.WriteFourCC("ATOM");

			writer.Write( index.Count );

			for ( short i=0; i<index.Count; i++ ) {
				writer.Write( i );
				writer.Write( index[i] );
			}
		}



		/// <summary>
		/// Reads collection using binary reader.
		/// </summary>
		/// <param name="reader"></param>
		internal void Read ( BinaryReader reader )
		{
			if (reader.ReadFourCC()!="ATOM") {
				throw new IOException("Bad FourCC. ATOM is expected.");
			}

			//	count:
			int count	=	reader.ReadInt32();

			for ( short i=0; i<count; i++) {
				short idA = reader.ReadInt16();

				short idB = Add( reader.ReadString() );

				if (idA!=idB) {
					throw new IOException("Bad ATOM table.");
				}
			}
		}
	}
}

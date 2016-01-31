using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Fusion.Engine.Graphics;
using Fusion.Engine.Audio;
using Fusion.Engine.Input;

namespace ShooterDemo {

	/// <summary>
	/// Represents collection of game entities.
	/// </summary>
	class GameEntityCollection : ICollection<GameEntity> {

		readonly HashSet<GameEntity> entities;
		bool clear = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="capacity"></param>
		public GameEntityCollection ()
		{
			entities	=	new HashSet<GameEntity>();
		}
			


		/// <summary>
		/// Gets the number of elements contained in the GameEntityCollection.
		/// </summary>
		public int Count {
			get {
				return entities.Count;
			}
		}



		/// <summary>
		/// Gets a value indicating whether the GameEntityCollection is read-only.
		/// </summary>
		public bool IsReadOnly {
			get {
				return false;
			}
		}


		
		/// <summary>
		/// 
		/// This method is deferred. To apply call Commit.
		/// </summary>
		/// <param name="entity"></param>
		public void Add ( GameEntity entity )
		{
			entities.Add( entity );
		}



		/// <summary>
		/// Removes all items from the GameEntityCollection.
		/// This method is deferred. To apply call Commit.
		/// </summary>
		public void Clear ()
		{
			entities.Clear();
		}



		/// <summary>
		/// Removes the first occurrence of a specific object from the GameEntityCollection.
		/// This method is deferred. To apply call Commit.
		/// </summary>
		/// <param name="entity"></param>
		public bool Remove ( GameEntity entity )
		{
			return entities.Remove( entity );
		}



		/// <summary>
		/// Determines whether the GameEntityCollection contains a specific value.
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool Contains ( GameEntity entity )
		{
			return entities.Contains( entity );
		}



		/// <summary>
		/// Copies the elements of the GameEntityCollection to an System.Array, starting at a particular System.Array index.
		///</summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo ( GameEntity[] array, int arrayIndex )
		{
			entities.CopyTo( array, arrayIndex );
		}



		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<GameEntity> GetEnumerator ()
		{
			return entities.GetEnumerator();
		}



		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return entities.GetEnumerator();
		}

	}
}

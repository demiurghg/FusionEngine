using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Build.Mapping {

	/// <summary>
	/// http://stackoverflow.com/questions/754233/is-it-there-any-lru-implementation-of-idictionary
	/// </summary>
	/// <typeparam name="Key"></typeparam>
	/// <typeparam name="Value"></typeparam>
    public class LRUCache<Key,Value>
    {
        private int capacity;
        private Dictionary<Key, LinkedListNode<LRUCacheItem>> cacheMap = new Dictionary<Key, LinkedListNode<LRUCacheItem>>();
        private LinkedList<LRUCacheItem> lruList = new LinkedList<LRUCacheItem>();


		class LRUCacheItem {
			public LRUCacheItem(Key key, Value value)
			{
				this.key = key;
				this.value = value;
			}
			public Key key;
			public Value value;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="capacity"></param>
        public LRUCache(int capacity)
        {
            this.capacity = capacity;
        }



		/// <summary>
		/// Clears entire cache
		/// </summary>
		public void Clear ()
		{
			cacheMap.Clear();
			lruList.Clear();
		}



		/// <summary>
		/// Gets cache values
		/// </summary>
		public IEnumerable<Value> GetValues () 
		{
			return lruList.Select( item => item.value ).ToArray();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Value this [ Key key ] {
			get {
				Value value;
				if (!TryGetValue(key, out value)) {
					throw new KeyNotFoundException();
				}
				return value;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
        public bool TryGetValue( Key key, out Value value )
        {
            LinkedListNode<LRUCacheItem> node;
            
			if (cacheMap.TryGetValue(key, out node)) {
                value = node.Value.value;
                lruList.Remove(node);
                lruList.AddLast(node);
                return true;
            } else {
				value = default(Value);
				return false;
			}
        }

        
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		public void Add(Key key, Value value)
        {
			Key k;
			Value v;
			AddDiscard( key, value, out k, out v );
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="discaredKey"></param>
		/// <param name="discardedValue"></param>
		/// <returns></returns>
		public bool AddDiscard ( Key key, Value value, out Key discaredKey, out Value discardedValue )
		{
			bool retValue	=	false;

            if (cacheMap.Count >= capacity) {
				retValue		=	true;
                Discard( out discaredKey, out discardedValue );
            } else {
				retValue		=	false;
				discardedValue	=	default(Value);
				discaredKey		=	default(Key);
			}

            var cacheItem = new LRUCacheItem(key, value);
            var node = new LinkedListNode<LRUCacheItem>(cacheItem);
            lruList.AddLast(node);
            cacheMap.Add(key, node);

			return retValue;
		}



		/// <summary>
		/// Dicards element from cache.
		/// Returns True if element was discareded successfully.
		/// Returns False if cache is empty.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Discard ( out Key key, out Value value )
        {
			if (!lruList.Any()) {
				value	=	default(Value);
				key		=	default(Key);
				return false;
			}

            // Remove from LRUPriority
            var node = lruList.First;
            lruList.RemoveFirst();

            // Remove from cache
            cacheMap.Remove(node.Value.key);

			//	
			key		=	node.Value.key;
			value	=	node.Value.value;

			return true;
        }



		/// <summary>
		/// Discards element
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Discard ( out Value value )
		{
			Key dummy;
			return Discard( out dummy, out value );
		}
    }



}

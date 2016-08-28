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

		class LRUCacheItem
		{
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
		public void Add(Key key, Value val)
        {
            if (cacheMap.Count >= capacity) {
                RemoveFirst();
            }

            var cacheItem = new LRUCacheItem(key, val);
            var node = new LinkedListNode<LRUCacheItem>(cacheItem);
            lruList.AddLast(node);
            cacheMap.Add(key, node);
        }

        
		/// <summary>
		/// 
		/// </summary>
		private void RemoveFirst()
        {
            // Remove from LRUPriority
            var node = lruList.First;
            lruList.RemoveFirst();

            // Remove from cache
            cacheMap.Remove(node.Value.key);
        }
    }



}

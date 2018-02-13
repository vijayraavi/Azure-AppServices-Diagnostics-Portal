using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Diagnostics.ScriptHost
{
    public class CompilationCache<K, V> : ICacheService<K, V>
    {
        private ConcurrentDictionary<K, V> _collection;

        public CompilationCache()
        {
            _collection = new ConcurrentDictionary<K, V>();
        }

        public void AddOrUpdate(K key, V value)
        {
            _collection.AddOrUpdate(key, value, (existingKey, oldValue) => value);
        }

        public bool TryGetValue(K key, out V value)
        {
            return _collection.TryGetValue(key, out value);
        }

        public bool RemoveValue(K key, out V value)
        {
            return _collection.TryRemove(key, out value);
        }

        public IEnumerable<V> GetAll()
        {
            return _collection.Values;
        }
    }
}

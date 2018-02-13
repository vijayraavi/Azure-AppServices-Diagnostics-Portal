using System.Collections.Generic;

namespace Diagnostics.ScriptHost
{
    public interface ICacheService<K, V>
    {
        void AddOrUpdate(K key, V value);

        bool TryGetValue(K key, out V value);

        bool RemoveValue(K key, out V value);

        IEnumerable<V> GetAll();
    }
}

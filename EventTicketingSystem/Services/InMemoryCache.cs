using System.Collections.Concurrent;
using System.Threading;

public class InMemoryCache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _store = new();
    
    private readonly ReaderWriterLockSlim _lock = new();

    private class CacheItem<T>
    {
        public T Value { get; }
        public DateTime Expiry { get; }

        public CacheItem(T value, TimeSpan? ttl = null)
        {
            Value = value;
            Expiry = ttl.HasValue ? DateTime.UtcNow.AddMinutes(ttl.Value.TotalMinutes) : DateTime.MaxValue;
        }
        public bool IsExpired => DateTime.UtcNow > Expiry;
    }

    public IEnumerable<TValue> GetExpiredValues()
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(item => item.IsExpired).Select(item => item.Value).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        _lock.EnterReadLock();
        try
        {
            if (_store.TryGetValue(key, out var item) && !item.IsExpired)
            {
                value = item.Value;
                return true;
            }

            value = default;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Set(TKey key, TValue value, TimeSpan? ttl = null)
    {
        _lock.EnterWriteLock();
        try
        {
            var item = new CacheItem<TValue>(value, ttl);
            _store[key] = item;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Remove(TKey key)
    {
        _lock.EnterWriteLock();
        try
        {
            return _store.TryRemove(key, out _);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void ClearExpired()
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (var key in _store.Keys)
            {
                if (_store.TryGetValue(key, out var item) && item.IsExpired)
                {
                    _store.TryRemove(key, out _);
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}

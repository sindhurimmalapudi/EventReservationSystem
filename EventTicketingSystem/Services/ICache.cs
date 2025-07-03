public interface ICache<TKey, TValue>
{
    IEnumerable<TValue> GetExpiredValues();
    bool TryGet(TKey key, out TValue? value);
    void Set(TKey key, TValue value, TimeSpan? ttl = null);
    bool Remove(TKey key);
    void ClearExpired();
}
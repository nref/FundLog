using System.Collections.Concurrent;

namespace FundLog.Model.Extensions;

public static class FluentCollections
{
  public static TDict AddRange<TDict, TKey, TValue>(this TDict dict, 
      IEnumerable<TValue> ts, 
      Func<TValue, TKey> key) 
    where TDict : IDictionary<TKey, TValue>
  {
    foreach (var t in ts)
    {
      dict[key(t)] = t;
    }
    return dict;
  }

  public static TDict Add<TDict, TKey, TValue>(this TDict dict, TValue t, Func<TValue, TKey> key)
    where TDict : IDictionary<TKey, TValue>
  {
    dict[key(t)] = t;
    return dict;
  }

  public static ConcurrentDictionary<Guid, Transaction> Remove(this ConcurrentDictionary<Guid, Transaction> dict, Guid id)
  {
    dict.TryRemove(id, out _);
    return dict;
  }
}

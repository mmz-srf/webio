namespace WebIO.Extensions;

using System.Collections.Generic;

public static class DictionaryExtensions
{
  public static TValue? ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
  {
    if (!dictionary.TryGetValue(key, out var value))
    {
      value = default;
    }

    return value;
  }
}

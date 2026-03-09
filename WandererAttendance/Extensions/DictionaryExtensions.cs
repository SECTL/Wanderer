using System.Collections.Generic;

namespace WandererAttendance.Extensions;

public static class DictionaryExtensions
{
    public static IDictionary<TKey, TValue> AddRange<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        IEnumerable<KeyValuePair<TKey, TValue>> from) where TKey : notnull
    {
        foreach (var kvp in from)
        {
            dictionary.Add(kvp.Key, kvp.Value);
        }

        return dictionary;
    }
}
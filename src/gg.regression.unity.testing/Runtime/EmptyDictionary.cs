using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RegressionGames.Unity
{
    internal static class EmptyDictionary
    {
        public static IReadOnlyDictionary<TKey, TValue> Of<TKey, TValue>() => EmptyDict<TKey, TValue>.Instance;

        class EmptyDict<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        {
            public static readonly EmptyDict<TKey, TValue> Instance = new();

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();

            public int Count => 0;
            public TValue this[TKey key] => throw new KeyNotFoundException("The given key was not present in the dictionary.");
            public IEnumerable<TKey> Keys => Enumerable.Empty<TKey>();
            public IEnumerable<TValue> Values => Enumerable.Empty<TValue>();

            public bool ContainsKey(TKey key) => false;
            public bool TryGetValue(TKey key, out TValue value)
            {
                value = default;
                return false;
            }
        }
    }
}

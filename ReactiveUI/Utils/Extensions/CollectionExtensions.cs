using System;
using System.Collections.Generic;
using System.Linq;

namespace Reactive {
    public static class CollectionExtensions {
        public static void RemoveValue<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TValue value
        ) {
            var item = dictionary.First(x => x.Value?.Equals(value) ?? false);
            dictionary.Remove(item.Key);
        }

        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> range) {
            foreach (var item in range) list.Add(item);
        }

        public static void RemoveRange<T>(this ICollection<T> list, IEnumerable<T> range) {
            foreach (var item in range) list.Remove(item);
        }

        public static bool TryPop<TValue>(this Stack<TValue> stack, out TValue? value) {
            value = default;
            if (stack.Count >= 1) {
                value = stack.Pop();
                return true;
            }
            return false;
        }
        
        public static void Deconstruct<T1, T2>(this Tuple<T1, T2> tuple, out T1 var1, out T2 var2) {
            var1 = tuple.Item1;
            var2 = tuple.Item2;
        }
        
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> tuple, out TKey key, out TValue value) {
            key = tuple.Key;
            value = tuple.Value;
        }
    }
}
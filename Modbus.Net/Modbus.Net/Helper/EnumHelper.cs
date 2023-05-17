using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net
{
#if NET462
#pragma warning disable 1591
    public static partial class EnumearbleExtensions
    {
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            if (null == source)
                throw new ArgumentNullException(nameof(source));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (0 == count)
                yield break;

            // Optimization (see JonasH's comment)
            if (source is ICollection<T>)
            {
                foreach (T item in source.Skip(((ICollection<T>)source).Count - count))
                    yield return item;

                yield break;
            }

            if (source is IReadOnlyCollection<T>)
            {
                foreach (T item in source.Skip(((IReadOnlyCollection<T>)source).Count - count))
                    yield return item;

                yield break;
            }

            // General case, we have to enumerate source
            Queue<T> result = new Queue<T>();

            foreach (T item in source)
            {
                if (result.Count == count)
                    result.Dequeue();

                result.Enqueue(item);
            }

            foreach (T item in result)
                yield return result.Dequeue();
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T item)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection should not be null");
            }

            return collection.Concat(Enumerable.Repeat(item, 1));
        }
    }
#pragma warning restore 1591
#endif
}

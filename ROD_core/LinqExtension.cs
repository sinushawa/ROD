using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core
{
    public static class LinqExtension
    {
        public static IEnumerable<T> FindPreviousItem<T>(this IEnumerable<T> items, Predicate<T> matchFilling)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            if (matchFilling == null)
                throw new ArgumentNullException("matchFilling");

            return FindPreviousItemImpl(items, matchFilling);
        }

        private static IEnumerable<T> FindPreviousItemImpl<T>(IEnumerable<T> items, Predicate<T> matchFilling)
        {
            using (var iter = items.GetEnumerator())
            {
                T previous = default(T);
                while (iter.MoveNext())
                {
                    if (matchFilling(iter.Current))
                    {
                        yield return previous;
                        yield return iter.Current;
                        if (iter.MoveNext())
                            yield return iter.Current;
                        else
                            yield return default(T);
                        yield break;
                    }
                    previous = iter.Current;
                }
            }
            // If we get here nothing has been found so return three default values
            yield return default(T); // Previous
            yield return default(T); // Current
        }
    }
}

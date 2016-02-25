using System;
using System.Collections.Generic;

namespace FourToolkit.Esent.Extensions
{
    public static class EnumerableExt
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action.Invoke(item);
            }
        }
    }
}

using System;
using System.Collections.Generic;

public static class EnumerableEx
{
    public static void forEach<TSource>(this IEnumerable<TSource> source, Action<TSource, int> selector)
    {
        var i = 0;
        foreach (var s in source)
        {
            selector(s, i);
            ++i;
        }
    }

    public static void forEach<TSource>(this IEnumerable<TSource> source, Action<TSource> selector)
    {
        foreach (var s in source)
        {
            selector(s);
        }
    }
}


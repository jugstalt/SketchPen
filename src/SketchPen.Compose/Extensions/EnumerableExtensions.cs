using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Compose.Extensions;

static internal class EnumerableExtensions
{
    static public IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? list)
    {
        if (list == null)
        {
            return Array.Empty<T>();
        }

        return list;
    }

    static public IEnumerable<T> OrSingle<T>(this IEnumerable<T>? list, T singleValue)
    {
        if(list == null || list.Count()==0) 
        {
            return new T[] { singleValue };
        }

        return list;
    }
}

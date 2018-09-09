using System;
using System.Collections.Generic;

namespace SchemaCompare.SchemaEngine.Extensions
{
    public static class ArrayExtension
    {
        public static IEnumerable<T> GetEnumerable<T>(this T[] array) => array;

        public static bool IsEmpty<T>(this T[] array) => array == null || array.Length == 0;
        public static bool HasValues<T>(this T[] array) => array != null && array.Length > 0;
    }
}
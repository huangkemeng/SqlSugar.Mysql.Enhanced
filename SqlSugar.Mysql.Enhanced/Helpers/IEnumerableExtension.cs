using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqsugar.Mysql.Enhanced.Helpers
{
    public static class IEnumerableExtension
    {
        public static List<T> ConcatWith<T, T1>(this List<T> t, IEnumerable<T1> t1, Func<T1, bool> where, Action<T, T1> action) where T : new()
        {
            if (t1 != null && t1.Any(where))
            {
                foreach (var item in t1.Where(where))
                {
                    T newObj = new T();
                    action.Invoke(newObj, item);
                    t.Add(newObj);
                }
            }
            return t;
        }

        public static List<T> ConcatWith<T, T1>(this List<T> t, IEnumerable<T1> t1, Func<T1, bool> where, Action<List<T>, T1> action) where T : new()
        {
            if (t1 != null && t1.Any(where))
            {
                foreach (var item in t1.Where(where))
                {
                    List<T> newObj = new List<T>();
                    action.Invoke(newObj, item);
                    t.AddRange(newObj);
                }
            }
            return t;
        }
    }
}

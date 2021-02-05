using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;

namespace Sqsugar.Mysql.Enhanced.Helpers
{
    public static class MtTypeConverter
    {
        public static JArray ToJarray(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<JArray>(json);
        }
        public static JArray ToJarray(this DataTable dataTable)
        {
            if (dataTable == null)
            {
                return null;
            }
            if (dataTable.Rows.Count == 0)
            {
                return new JArray();
            }
            return JArray.FromObject(dataTable.Rows[0].Table);
        }
        public static JObject ToJObject(this string json)
        {
            if (json == null)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<JObject>(json);
        }
        public static JArray ToJarray<T>(this IEnumerable<T> list) where T : class
        {
            if (list == null)
            {
                return null;
            }
            return JArray.FromObject(list);
        }
        public static JObject ToJObject<T>(this T obj) where T : class
        {
            try
            {
                if (obj == null)
                {
                    return null;
                }
                return JObject.FromObject(obj);
            }
            catch (Exception ex)
            {
                throw ex.GetInnnerException();
            }
        }
        public static string ToJson<T>(this T Obj) where T : class
        {
            if (Obj == null)
            {
                return null;
            }
            if (typeof(T) == typeof(string) ||
                typeof(T) == typeof(JArray) ||
                typeof(T) == typeof(JObject) ||
                typeof(T) == typeof(JToken))
            {
                return Obj.ToString();
            }
            if (Obj is DataTable dt)
            {
                if (dt.Rows.Count > 0)
                {
                    return JsonConvert.SerializeObject(dt.Rows[0].Table);
                }
                return "[]";
            }
            return JsonConvert.SerializeObject(Obj);
        }
        public static List<T> ToEntity<T>(this DataTable dataTable) where T : class, new()
        {
            if (dataTable == null)
            {
                return default;
            }
            List<T> list = JsonConvert.DeserializeObject<List<T>>(dataTable.ToJson());
            return list;
        }
    }
}

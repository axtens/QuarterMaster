using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuarterMaster.Serialization
{
    public static class Properties
    {
        public static Dictionary<string, object> GetObjectProperties(object obj, int max = 5)
        {
            var results = new Dictionary<string, object>();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                string name = descriptor.Name;
                object value;
                if (descriptor.GetValue(obj) != null && !IsSimple(descriptor.GetValue(obj).GetType()) && max != 0)
                {
                    try
                    {
                        value = GetObjectProperties(descriptor.GetValue(obj), max - 1);
                    }
                    catch (Exception)
                    {
                        value = descriptor.GetValue(obj);
                    }
                }
                else
                {
                    value = descriptor.GetValue(obj);
                }
                results[name] = value;
            }
            return results;
        }

        private static bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }

        public static string DumpObjectPropertiesDictionaryToXML(Dictionary<string, object> dict)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> kvp in dict)
            {
                string k = kvp.Key;
                object v = kvp.Value;
                if (v != null)
                {
                    sb.Append(Markup.Tag(k, v.GetType().Equals(typeof(Dictionary<string, object>))
                        ? DumpObjectPropertiesDictionaryToXML((Dictionary<string, object>)v)
                        : v));
                }
                else
                {
                    sb.Append(Markup.Tag(k, v));
                }
            }
            return sb.ToString();
        }
    }
}

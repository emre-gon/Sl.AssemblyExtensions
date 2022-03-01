using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace Sl.AssemblyExtensions
{
    public static class LinqExtensions
    {
        public static T PrimitiveClone<T>(this T obj) where T : class
        {
            T newObj = (T)Activator.CreateInstance(obj.GetType());


            foreach (var prop in obj.GetType().GetFields())
            {
                var propType = prop.FieldType;
                if (propType.IsPrivitiveType())
                {
                    prop.SetValue(newObj, prop.GetValue(obj));
                }

            }

            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.GetGetMethod() != null && prop.GetSetMethod() != null)
                {
                    var propType = prop.PropertyType;
                    if (propType.IsPrivitiveType())
                    {
                        prop.SetValue(newObj, prop.GetValue(obj, null), null);
                    }
                }
            }

            return newObj;
        }

        public static bool IsOrdered(this IQueryable Data)
        {
            string query = Data.ToString();

            int pIndex = query.LastIndexOf(')');

            if (pIndex == -1)
                pIndex = 0;

            if (query.IndexOf("ORDER BY", pIndex) != -1)
            {
                return true;
            }
            return false;
        }
    }
}

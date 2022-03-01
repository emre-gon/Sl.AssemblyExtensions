using Sl.Extensions.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sl.AssemblyExtensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetEnums<T>() => Enum.GetValues(typeof(T)).Cast<T>().ToList();

        public static T GetAttribute<T>(this Enum value) where T : Attribute
            => (T)value.GetType().GetField(value.ToString(), BindingFlags.Static | BindingFlags.Public)
                    .GetCustomAttributes<T>(false).FirstOrDefault();
        
        public static string DisplayName(this Enum value)
            => !value.IsValidEnum() ? null : value.GetFieldValue<EnumDisplayNameAttribute>() ?? value.ToString();

        public static string GetFieldValue<T>(this Enum value) where T : EnumFieldAttribute
            => !value.IsValidEnum() ? null : value.GetAttribute<T>()?.FieldValue;

        public static bool IsValidEnum(this Enum value)
            => value == null ? false : value.GetType().GetMember(value.ToString()).Length != 0;

        public class EnumNode
        {
            public int Value { get; set; }
            public string DisplayName { get; set; }
        }
                
        public static IEnumerable<EnumNode> GetEnumDisplayNamePair<T>()
            => (from z in GetEnums<T>()
                select new EnumNode
                {
                    Value = (int)(object)z,
                    DisplayName = ((Enum)(object)z).DisplayName()
                });

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Columns">"Örnek: "DisplayName", "Value"</param>
        /// <returns></returns>
        public static IEnumerable<dynamic> GetEnumList<T>(IEnumerable<string> Columns)
        {
            if (Columns.Any(f => f.Contains(" ")))
                throw new Exception("Enum field name (Name of the EnumFieldAttribute) cannot contain spaces.");

            if (Columns.Any(f => string.IsNullOrEmpty(f)))
                throw new Exception("Enum field name (Name of the EnumFieldAttribute) cannot be null.");
                        
            var enums = GetEnums<T>();
            List<dynamic> toBeReturned = new List<dynamic>();

            var type = typeof(T);

            foreach (var en in enums)
            {
                var retObj = new ExpandoObject() as IDictionary<string, Object>;
                toBeReturned.Add(retObj);
                foreach (var col in Columns)
                {
                    if (col == "Value")
                    {
                        retObj[col] = (int)(object)en;
                    }
                    else
                    {
                        var field = type.GetField(en.ToString(), BindingFlags.Static | BindingFlags.Public);
                        var attributes = field.GetCustomAttributes<EnumFieldAttribute>()
                                                .Where(f => f.GetType().Name == col || f.GetType().Name == col + "Attribute");
                        if (!attributes.Any()) {
                            if (col == "DisplayName")
                                retObj[col] = en.ToString();
                            else
                                retObj[col] = null;
                        }                         
                        else
                            retObj[col] = ((EnumFieldAttribute)attributes.First()).FieldValue;
                    }
                }
            }
            return toBeReturned;
        }



        public static T Parse<T>(string value)
        {
            try
            {
                if (Enum.IsDefined(typeof(T), value))
                    return (T)Enum.Parse(typeof(T), value);

                if (string.IsNullOrEmpty(value))
                {
                    value = "0";
                }

                return (T)Enum.ToObject(typeof(T), int.Parse(value));
            }
            catch (FormatException)
            {
                throw new Exception($"No enum named: {value} exists in type {typeof(T).FullName}");
            }
        }

        public static T ParseFromDisplayName<T>(string value)
        {
            var node = GetEnumDisplayNamePair<T>().Where(f => f.DisplayName == value);

            if (!node.Any())
                throw new Exception($"No enums found with DisplayName: '{value}' in type: {typeof(T).FullName}");
            else if (node.Count() > 1)
                throw new Exception($"Multiple Enums found with DisplayName: '{value}' in type: {typeof(T).FullName}");
            else
                return (T)(object)node.Single().Value;
        }
    }


}
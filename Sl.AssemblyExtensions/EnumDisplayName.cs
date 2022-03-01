using System;
using System.Collections.Generic;
using System.Text;

namespace Sl.AssemblyExtensions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public abstract class EnumFieldAttribute : Attribute
    {
        public EnumFieldAttribute(string FieldValue)
        {
            this.FieldValue = FieldValue;
        }

        public string FieldValue { get; }
    }


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EnumDisplayNameAttribute : EnumFieldAttribute
    {
        public EnumDisplayNameAttribute(string displayName)
            : base(displayName)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sl.AssemblyExtensions
{
    public class ObjectProperty<TProp>
    {
        public ObjectProperty(object ContainerObject, PropertyInfo Property)
        {
            if (Property.PropertyType != typeof(TProp))
            {
                throw new Exception($"Propety type of '{Property.Name}' is not {typeof(TProp).FullName}." +
                    $" It is ${Property.PropertyType.FullName}");
            }



            this.ContainerObject = ContainerObject;
            this.Property = Property;
        }
        public object ContainerObject { get; }
        public PropertyInfo Property { get; }


        public TProp GetValue()
        {
            if (ContainerObject == null)
                return default;

            return (TProp)Property.GetValue(ContainerObject);
        }

        public void SetValue(TProp NewValue)
        {
            Property.SetValue(ContainerObject, NewValue);
        }
    }
}

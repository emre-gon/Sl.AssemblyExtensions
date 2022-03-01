using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sl.AssemblyExtensions
{
    public static class AssemblyExtensions
    {
        #region type lists
        public readonly static Type[] NumericTypes = {
                                               typeof(byte),
                                               typeof(byte?),
                                               typeof(sbyte),
                                               typeof(sbyte?),
                                               typeof(short),
                                               typeof(short?),
                                               typeof(ushort),
                                               typeof(ushort?),
                                               typeof(int),
                                               typeof(int?),
                                               typeof(uint),
                                               typeof(uint?),
                                               typeof(long),
                                               typeof(long?),
                                               typeof(ulong),
                                               typeof(ulong?)
        };

        public readonly static Type[] DecimalTypes = {
                                               typeof(double),
                                               typeof(double?),
                                               typeof(decimal),
                                               typeof(decimal?),
                                               typeof(float),
                                               typeof(float?)};

        internal static HashSet<Type> Primitive_Types = new HashSet<Type>()
        {
                                               typeof(bool),
                                               typeof(bool?),
                                               typeof(char),
                                               typeof(char?),
                                               typeof(byte),
                                               typeof(byte?),
                                               typeof(sbyte),
                                               typeof(sbyte?),
                                               typeof(short),
                                               typeof(short?),
                                               typeof(ushort),
                                               typeof(ushort?),
                                               typeof(int),
                                               typeof(int?),
                                               typeof(uint),
                                               typeof(uint?),
                                               typeof(long),
                                               typeof(long?),
                                               typeof(ulong),
                                               typeof(ulong?),
                                               typeof(double),
                                               typeof(double?),
                                               typeof(decimal),
                                               typeof(decimal?),
                                               typeof(float),
                                               typeof(float?),
                                               typeof(Guid),
                                               typeof(Guid?),
                                               typeof(DateTime),
                                               typeof(DateTime?),
                                               typeof(string),
                                               typeof(byte[]),
        };
        #endregion

        #region type flags
        public static bool IsNullable(this Type t)
        {
            return t.IsNullablePrimitive() || !t.IsPrivitiveType();
        }

        public static bool IsNullablePrimitive(this Type t)
        {
            return t.IsGenericType
                                && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsEnumOrNullableEnum(this Type t)
        {
            if (t.IsEnum)
                return true;

            Type u = Nullable.GetUnderlyingType(t);
            return (u != null) && u.IsEnum;
        }

        public static bool IsPrivitiveType(this Type type)
        {
            return Primitive_Types.Contains(type);
        }

        public static bool IsSimpleType(this Type type)
        {
            if (type.IsPrivitiveType())
                return true;

            if (type.IsEnumOrNullableEnum())
                return true;

            return false;
        }
        #endregion


        #region common expressions
        public static ObjectProperty<TProp> GetPropertyFromExpression<T, TProp>(this T Obj,
            Expression<Func<T, TProp>> exp, bool IsConstantExpression = false)
        {
            Stack<PropertyInfo> props = new Stack<PropertyInfo>();
            var memberSelectorExpression = exp.Body as MemberExpression;
            do
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                props.Push(property);
                memberSelectorExpression = memberSelectorExpression.Expression as MemberExpression;

            } while (memberSelectorExpression != null);

            if (exp.IsConstantExpression())
            {
                props.Pop(); // ilk prop constant exp'e ait. Zaten çekildi
            }
            var currentObject = (object)Obj;
            do
            {
                PropertyInfo prop = props.Pop();
                if (props.Any())
                {
                    currentObject = prop.GetValue(currentObject);
                }
                else
                {
                    return new ObjectProperty<TProp>(currentObject, prop);
                }

            } while (props.Any());

            return null;
        }
        #endregion

        #region standart lambda expressions
        public static TProp GetValueFromExpression<T, TProp>(this T Obj, Expression<Func<T, TProp>> exp)
        {
            return Obj.GetPropertyFromExpression(exp).GetValue();
        }

        public static void SetValueFromExpression<T, TProp>(this T Obj, Expression<Func<T, TProp>> exp, TProp Value)
        {
            Obj.GetPropertyFromExpression(exp).SetValue(Value);
        }
        #endregion

        #region constant expressions
        public static bool IsConstantExpression<T, TProp>(this Expression<Func<T, TProp>> exp)
        {
            var mex = exp.Body as MemberExpression;
            var constMember = mex.Expression as MemberExpression;
            var constExpression = constMember.Expression as ConstantExpression;

            return constExpression != null;
        }

        public static T GetConstValueFromConstExpression<T, TProp>(this Expression<Func<T, TProp>> exp)
        {
            if (!exp.IsConstantExpression())
                throw new Exception("Expression is not a constant expression");

            var mex = exp.Body as MemberExpression;

            var constMember = mex.Expression as MemberExpression;
            var constProperty = constMember.Member as PropertyInfo;

            var constExpression = constMember.Expression as ConstantExpression;
            var containerObject = constExpression.Value;

            var obj = constProperty.GetValue(containerObject);
            return (T)obj;

        }

        public static TProp GetValueFromConstExpression<T, TProp>(this Expression<Func<T, TProp>> exp)
        {
            T ConstValue = exp.GetConstValueFromConstExpression();
            return ConstValue.GetPropertyFromExpression(exp).GetValue();
        }


        public static void SetValueFromConstExpression<T, TProp>(this Expression<Func<T, TProp>> exp, TProp Value)
        {
            T ConstValue = exp.GetConstValueFromConstExpression();
            ConstValue.SetValueFromExpression(exp, Value);
        }
        #endregion



    }
}


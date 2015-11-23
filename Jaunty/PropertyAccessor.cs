using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Jaunty
{
    // Greedily stolen without shame or remorse from http://thibaud60.blogspot.no/2010/10/fast-property-accessor-without-dynamic.html

    internal interface IPropertyAccessor
    {
        PropertyInfo PropertyInfo { get; }
        object GetValue(object source);
        void SetValue(object source, object value);
    }

    internal static class PropertyInfoHelper
    {
        private static ConcurrentDictionary<PropertyInfo, IPropertyAccessor> _cache =
            new ConcurrentDictionary<PropertyInfo, IPropertyAccessor>();

        public static IPropertyAccessor GetAccessor(PropertyInfo propertyInfo)
        {
            IPropertyAccessor result = null;
            if (!_cache.TryGetValue(propertyInfo, out result))
            {
                result = CreateAccessor(propertyInfo);
                _cache.TryAdd(propertyInfo, result); ;
            }
            return result;
        }

        public static IPropertyAccessor CreateAccessor(PropertyInfo PropertyInfo)
        {
            var GenType = typeof(PropertyWrapper<,>)
                .MakeGenericType(PropertyInfo.DeclaringType, PropertyInfo.PropertyType);
            return (IPropertyAccessor)Activator.CreateInstance(GenType, PropertyInfo);
        }
    }

    internal class PropertyWrapper<TObject, TValue> : IPropertyAccessor where TObject : class
    {
        private Func<TObject, TValue> Getter;
        private Action<TObject, TValue> Setter;

        public PropertyWrapper(PropertyInfo PropertyInfo)
        {
            this.PropertyInfo = PropertyInfo;

            MethodInfo GetterInfo = PropertyInfo.GetGetMethod();
            MethodInfo SetterInfo = PropertyInfo.GetSetMethod();

            // 2015-11-21 Modified by me. It's not at all certain that the properies have both a getter and a setter
            // for instance in the case of an anonymous type. So I added a couple if's

            if (GetterInfo != null)
            {
                Getter = (Func<TObject, TValue>)Delegate.CreateDelegate
                        (typeof(Func<TObject, TValue>), GetterInfo);
            }

            if (SetterInfo != null)
            {
                Setter = (Action<TObject, TValue>)Delegate.CreateDelegate
                        (typeof(Action<TObject, TValue>), SetterInfo);
            }
        }

        object IPropertyAccessor.GetValue(object source)
        {
            return Getter(source as TObject);
        }

        void IPropertyAccessor.SetValue(object source, object value)
        {
            Setter(source as TObject, (TValue)value);
        }

        public PropertyInfo PropertyInfo { get; private set; }
    }
}

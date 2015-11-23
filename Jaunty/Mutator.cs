using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jaunty
{
    public static class Mutator
    {
        /// <summary>
        /// Creates a mutator which monitors which properties gets set by the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="obj">Object to apply modifications to</param>
        /// <param name="properties">Object containing the properties to modify</param>
        /// <returns>A mutator containing the changes needed</returns>
        public static IMutator<T> SetProperties<T, U>(this T obj, U properties)
        {
            return new Mutator<T, U>(obj, properties).Apply();
        }
    }


    internal sealed class Mutator<T, U> : IMutator<T, U>
    {
        readonly T _obj;
        readonly U _properties;
        readonly List<string> _changeSet;

        public Mutator(T obj, U propertiesToSet)
        {
            _obj = obj;
            _properties = propertiesToSet;
            _changeSet = new List<string>();
        }


        /// <summary>
        /// Applies changes to the object
        /// </summary>
        /// <returns>A mutator containing the new changes</returns>
        public IMutator<T> Apply()
        {
            var t = typeof(U);
            var st = typeof(T);


            // Iterate through all properties and copy them to the destination while remembering the name of each one
            foreach (var property in t.GetProperties())
            {

                // If the source has set the NotMappedAttribute on the property
                // We'll assume it's because it shouldn't be copied to the destination
                if (property.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;

                // Property for writing
                var targetProp = st.GetProperty(property.Name);
                // Accessor for reading the source property value
                var readAccessor = PropertyInfoHelper.GetAccessor(property);
                
                // Accessor for writing to the destination property
                var writeAccessor = PropertyInfoHelper.GetAccessor(targetProp);

                var newValue = readAccessor.GetValue(_properties);

                var propertyType = readAccessor.PropertyInfo.PropertyType;

                // If they are of the same type, make sure that the property is not actually set
                if(t == st)
                {
                    var defaultValue = GetDefaultValue(propertyType);

                    if (object.Equals(defaultValue, newValue))
                        continue;
                }

                writeAccessor.SetValue(_obj, newValue);

                // Remember the modified property
                _changeSet.Add(property.Name);
            }

            // In order to make it easier to use, return the instance.
            // obj.SetProperties(new { ... }).Apply() will return the mutator
            return this;


        }

        private readonly Dictionary<Type, Func<object>> _typeDefaults = new Dictionary<Type, Func<object>>();

        private object GetDefaultValue(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if(_typeDefaults.ContainsKey(type))
            {
                return _typeDefaults[type]();
            }

            var tdefault = Expression.Lambda<Func<object>>(
                Expression.Convert(
                    Expression.Default(type), typeof(object)
                )
            ).Compile();

            _typeDefaults.Add(type, tdefault);
            
            return tdefault();

        }


        /// <summary>
        /// Gets the properties that has had their values altered
        /// </summary>
        public IEnumerable<string> Changeset => _changeSet;

        /// <summary>
        /// Gets the target that have been modified
        /// </summary>
        public T Target => _obj;

    }


}

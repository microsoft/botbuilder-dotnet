using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Expressions
{
    /// <summary>
    /// Get the value of a property of an object
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate object GetValueDelegate(object instance, object key);

    /// <summary>
    /// Look up identifier binding from environment scope
    /// </summary>
    public static class PropertyBinder
    {
        /// <summary>
        /// Automatically use reflection or dictionary based on instance
        /// </summary>
        public static GetValueDelegate Auto = (object instance, object property) =>
        {
            if (instance == null)
            {
                return null;
            }

            if (instance is IDictionary<string, object> || instance is IDictionary)
            {
                return Dictionary(instance, property);
            }
            else if (instance.GetType().IsArray)
            {
                return ((Array)instance).GetValue((int)property);
            }
            return Reflection(instance, property);
        };

        /// <summary>
        /// Use reflection to bind to properties of instance object
        /// </summary>
        public static GetValueDelegate Reflection = (object instance, object property) =>
        {
            var propInfo = instance.GetType().GetProperty((string)property);
            if (propInfo != null)
            {
                return propInfo.GetValue(instance);
            }
            return null;
        };

        /// <summary>
        /// Use IDictionary<string, object> to get acces to properties of instance object</string>
        /// </summary>
        public static GetValueDelegate Dictionary = (object instance, object property) =>
        {
            object result = null;
            var dictionary = instance as IDictionary;
            if (dictionary != null)
            {
                if (dictionary.Contains(property))
                {
                    result = dictionary[property];
                }
                return result;
            }

            ((IDictionary<string, object>)instance).TryGetValue((string)property, out result);
            return result;
        };
    }
}

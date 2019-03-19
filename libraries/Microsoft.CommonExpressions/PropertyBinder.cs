using System;
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
            if (instance is IDictionary<string, object>)
            {
                return Dictionary(instance, property);
            }
            else if (instance.GetType().IsArray)
            {
                return ((Array)instance).GetValue((int)property);
            }
            try
            {
                return Reflection(instance, property);
            }
            catch (Exception)
            {
                throw new Exception($"Sorry, instance {instance} does not have property {property}");
            }
            
        };

        /// <summary>
        /// Use reflection to bind to properties of instance object
        /// </summary>
        public static GetValueDelegate Reflection = (object instance, object property) => instance.GetType().GetProperty((string)property).GetValue(instance);

        /// <summary>
        /// Use IDictionary<string, object> to get acces to properties of instance object</string>
        /// </summary>
        public static GetValueDelegate Dictionary = (object instance, object property) => ((IDictionary<string, object>)instance)[(string)property];
    }
}

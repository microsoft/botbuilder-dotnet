using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
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
                throw new Exception($"getting property {property.ToString()} on null");
            }

            if (instance is IDictionary<string, object> || instance is IDictionary)
            {
                return Dictionary(instance, property);
            }
            else if (instance is JObject)
            {
                var jObj = instance as JObject;
                return jObj[property] ?? null;
            }
            else if (instance is IList list)
            {
                    // this also covers JArray
                    // this must be after JObject, because JObject is also IList<JToken>
                    return list[(int)property];
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

    /// <summary>
    /// Wrap a GetMethodDelegate, returns a new delegate that throw the right exceptions
    /// 1st and 3rd party GetMethodDelegate needs to be wrapped into this, to work best with the rest
    /// </summary>
    class GetValueDelegateWrapper
    {
        // this is a wrapper to help throw proper exceptions
        private readonly GetValueDelegate _getValue = null;
        public GetValueDelegateWrapper(GetValueDelegate getValue)
        {
            _getValue = getValue;
        }

        public object GetValue(object instance, object property)
        {
            try
            {
                return _getValue(instance, property);
            }
            catch (Exception e)
            {
                throw new Exception($"Can't get property {property.ToString()} on {instance}, error: {e.Message}");
            }
        }
    }
}

// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Extension method on object <see cref="object"/>.
    /// </summary>
    public static partial class DictionaryExtensions
    {
        /// <summary>
        /// Extension Method on object to cast to type T to support TypeNameHandling.None during storage serialization.
        /// </summary>
        /// <param name="dict">object to cast.</param>
        /// <param name="property">property name.</param>
        /// <typeparam name="T">type to which object should be casted.</typeparam>
        /// <returns>T.</returns>
        public static T GetTypedValue<T>(this IDictionary<string, object> dict, string property)
        {
            if (dict.TryGetTypedValue(property, out T result))
            {
                return result;
            }

            return default(T);
        }

        /// <summary>
        /// Extension Method on object to cast to type T to support TypeNameHandling.None during storage serialization.
        /// </summary>
        /// <param name="dict">object to cast.</param>
        /// <param name="property">property name.</param>
        /// <param name="result">result.</param>
        /// <typeparam name="T">type to which object should be casted.</typeparam>
        /// <returns>T.</returns>
        public static bool TryGetTypedValue<T>(this IDictionary<string, object> dict, string property, out T result)
        {
            result = default(T);

            if (dict.TryGetValue(property, out object obj))
            {
                if (obj is T asT)
                {
                    result = asT;
                    return true;
                }

                if (obj is JObject asJobject)
                {
                    // If types are not used by storage serialization, and Newtonsoft is the serializer
                    // the item found can be a JObject.
                    result = asJobject.ToObject<T>();
                    dict[property] = result;
                    return true;
                }
                else if (obj is JArray asJarray)
                {
                    // If types are not used by storage serialization, and Newtonsoft is the serializer
                    // the item found can be a JArray.
                    result = asJarray.ToObject<T>();
                    dict[property] = result;
                    return true;
                }
                else if (obj is JValue asJValue)
                {
                    // If types are not used by storage serialization, and Newtonsoft is the serializer
                    // the item found can be a JValue.
                    result = asJValue.ToObject<T>();
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(byte))
                {
                    result = (T)(object)Convert.ToByte(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(short))
                {
                    result = (T)(object)Convert.ToInt16(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(ushort))
                {
                    result = (T)(object)Convert.ToUInt16(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(int))
                {
                    result = (T)(object)Convert.ToInt32(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(uint))
                {
                    result = (T)(object)Convert.ToUInt32(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(long))
                {
                    result = (T)(object)Convert.ToInt64(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(ulong))
                {
                    result = (T)(object)Convert.ToUInt64(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(float))
                {
                    result = (T)(object)Convert.ToSingle(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
                else if (typeof(T) == typeof(double))
                {
                    result = (T)(object)Convert.ToDouble(obj, CultureInfo.InvariantCulture);
                    dict[property] = result;
                    return true;
                }
            }

            return false;
        }
    }
}

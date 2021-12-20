// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Newtonsoft.Json.Linq;

namespace AuthenticationBot
{
    /// <summary>
    /// Extension method on object <see cref="object"/>.
    /// </summary>
    internal static partial class Extensions
    {
        /// <summary>
        /// Extension Method on object to cast to type T to support TypeNameHandling.None during storage serialization.
        /// </summary>
        /// <param name="obj">object to cast.</param>
        /// <typeparam name="T">type to which object should be casted.</typeparam>
        /// <returns>T.</returns>
        public static T CastTo<T>(this object obj)
        {
            if (obj is T asT)
            {
                return asT;
            }
            else if (obj is JObject asJobject)
            {
                // If types are not used by storage serialization, and Newtonsoft is the serializer
                // the item found can be a JObject.
                return asJobject.ToObject<T>();
            }
            else if (obj is JArray asJarray)
            {
                // If types are not used by storage serialization, and Newtonsoft is the serializer
                // the item found can be a JArray.
                return asJarray.ToObject<T>();
            }
            else if (obj == null)
            {
                return default(T);
            }
            else
            {
                throw new InvalidOperationException("Data is not in the correct format for casting.");
            }
        }
    }
}

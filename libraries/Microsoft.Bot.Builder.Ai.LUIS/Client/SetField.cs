// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    public static class SetField
    {
        public static void NotNull<T>(out T field, string name, T value) where T : class
        {
            CheckNull(name, value);
            field = value;
        }

        public static void CheckNull<T>(string name, T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void NotNullFrom<T>(out T field, string name, SerializationInfo info) where T : class
        {
            var value = (T)info.GetValue(name, typeof(T));
            NotNull(out field, name, value);
        }

        public static void From<T>(out T field, string name, SerializationInfo info)
        {
            var value = (T)info.GetValue(name, typeof(T));
            field = value;
        }
    }
}
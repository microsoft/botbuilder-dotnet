#pragma warning disable SA1402 // File may only contain a single type
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public class TypeRegistration
    {
        public TypeRegistration()
        {
        }

        public TypeRegistration(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        /// <summary>
        /// gets or sets the declarative id for this type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// gets or sets the type for this registration.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets an optional custom deserializer for this type.
        /// </summary>
        public ICustomDeserializer CustomDeserializer { get; set; }
    }

    public class TypeRegistration<T> : TypeRegistration
    {
        public TypeRegistration()
        {
            this.Type = typeof(T);
        }

        public TypeRegistration(string name)
        {
            this.Name = name;
            Type = typeof(T);
        }
    }
}

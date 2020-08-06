// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1402 // File may only contain a single type

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// DeclarativeType object which is $kind => type.
    /// </summary>
    public class DeclarativeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarativeType"/> class.
        /// </summary>
        public DeclarativeType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarativeType"/> class.
        /// </summary>
        /// <param name="kind">$kind.</param>
        /// <param name="type">type.</param>
        public DeclarativeType(string kind, Type type)
        {
            this.Kind = kind;
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets the declarative id for this type.
        /// </summary>
        /// <value>
        /// The declarative id for this type.
        /// </value>
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the type for this registration.
        /// </summary>
        /// <value>
        /// The type for this registration.
        /// </value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets an optional custom deserializer for this type.
        /// </summary>
        /// <value>
        /// An optional custom deserializer for this type.
        /// </value>
        public ICustomDeserializer CustomDeserializer { get; set; }
    }

    /// <summary>
    /// TypeRegistration of Kind => type using generic.
    /// </summary>
    /// <typeparam name="T">type.</typeparam>
    public class DeclarativeType<T> : DeclarativeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarativeType{T}"/> class.
        /// </summary>
        /// <param name="kind">$kind.</param>
        public DeclarativeType(string kind)
            : base(kind, typeof(T))
        {
        }
    }
}

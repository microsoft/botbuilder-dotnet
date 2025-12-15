// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// An implementation of the <see cref="DefaultSerializationBinder"/>,
    /// capable of allowing only desired <see cref="Type"/>s to be serialized and deserialized.
    /// </summary>
    public class AllowedTypesSerializationBinder : DefaultSerializationBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedTypesSerializationBinder"/> class.
        /// </summary>
        /// <param name="allowedTypes">A list of types to allow when the binder assign them upon deserialization.</param>
        public AllowedTypesSerializationBinder(IList<Type> allowedTypes = default)
        {
            AllowedTypes = allowedTypes ?? new List<Type>();
        }

        /// <summary>
        /// Gets the collection of the allowed types.
        /// </summary>
        /// <value>
        /// A <see cref="IList{T}"/> of allowed <see cref="Type"/> classes.
        /// </value>
        public IList<Type> AllowedTypes { get; }

        private IList<Type> DeniedTypes { get; } = new List<Type>();

        /// <summary>
        /// <para>
        /// Given the <paramref name="serializedType"/> parameter,
        /// it evaluates if the <see cref="Type"/> is allowed by this SerializationBinder.
        /// </para>
        /// <para>
        /// Either allowed or not allowed, it will output the name of the <see cref="Type"/> through the <paramref name="typeName"/> parameter.
        /// </para>
        /// <para>
        /// When allowed, it will add the <see cref="Type"/> to the <see cref="AllowedTypes"/> collection.
        /// </para>
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType?.AssemblyQualifiedName;

            if (serializedType == null)
            {
                return;
            }

            if (IsTypeAllowed(serializedType))
            {
                AllowType(serializedType);
            }
            else
            {
                DenyType(serializedType);
            }
        }

        /// <summary>
        /// Given the <paramref name="assemblyName"/> and <paramref name="typeName"/> parameters,
        /// it validates if the resulted <see cref="Type"/> is found in the <see cref="AllowedTypes"/> collection, and returns its value.
        /// <para>
        /// When found, it will add the <see cref="Type"/> to the <see cref="AllowedTypes"/> collection if it doesn't exist.
        /// </para>
        /// </summary>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        /// <returns>The resulted <see cref="Type"/> from the provided <paramref name="assemblyName"/> and <paramref name="typeName"/> parameters.</returns>
        public override Type BindToType(string assemblyName, string typeName) // CodeQL [SM05220] This behavior cannot be changed without breaking all type binding. Entire project due to be archived in Dec 31, 2025 in part due to this design. Newer SDK avoids this. 
        {
            var resolvedTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", typeName, assemblyName);
            var type = Type.GetType(resolvedTypeName);

            if (IsTypeAllowed(type))
            {
                AllowType(type);
            }
            else
            {
                DenyType(type);
            }

            return type;
        }

        /// <summary>
        /// Verifies if there are types that are not allowed.
        /// <para>
        /// When not allowed, it will throw an <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">Exception thrown when there are types that are not allowed.</exception>
        public void Verify()
        {
            if (DeniedTypes.Any())
            {
                ThrowDeniedTypesError(DeniedTypes);
            }
        }

        private Func<Type, bool> IsTypeEqualTo(Type second) => (Type first) => first.AssemblyQualifiedName == second.AssemblyQualifiedName;

        private void ThrowDeniedTypesError(IList<Type> types)
        {
            var items = types.Select(type => $"  - {type.AssemblyQualifiedName}");
            var typeOfs = types.Select(type => $"typeof({type.Name}),");
            var message = $"Unable to find the following types in the '{nameof(AllowedTypes)}' collection." + Environment.NewLine +
                        string.Join(Environment.NewLine, items) + Environment.NewLine +
                        Environment.NewLine +
                        $"Please provide the '{nameof(AllowedTypesSerializationBinder)}' in the custom '{nameof(JsonSerializerSettings)}' instance, with the list of types to allow." + Environment.NewLine +
                        Environment.NewLine +
                        "Example:" + Environment.NewLine +
                        "    new JsonSerializerSettings" + Environment.NewLine +
                        "    {" + Environment.NewLine +
                        "        SerializationBinder = new AllowedTypesSerializationBinder(" + Environment.NewLine +
                        "            new List<Type>" + Environment.NewLine +
                        "            {" + Environment.NewLine +
                        $"                {string.Join(Environment.NewLine + "                ", typeOfs)}" + Environment.NewLine +
                        "            })," + Environment.NewLine +
                        "    }";
            throw new InvalidOperationException(message);
        }

        private bool IsTypeAllowed(Type serializedType)
        {
            if (serializedType == null)
            {
                return false;
            }

            // Return when Type is found.
            var typeFound = AllowedTypes.FirstOrDefault(IsTypeEqualTo(serializedType));
            if (typeFound != null)
            {
                return true;
            }

            // Return when the Type is inside another Type.
            if (serializedType.IsNested)
            {
                if (IsTypeAllowed(serializedType.ReflectedType))
                {
                    return true;
                }
            }
            
            // Return when the Type is represented as a generic, e.g.: List<T>.
            var arguments = serializedType.GetGenericArguments();
            var argumentsFound = AllowedTypes.FirstOrDefault(t => arguments.Any(IsTypeEqualTo(t)));
            if (argumentsFound != null)
            {
                return true;
            }

            // Return when the Type has Interfaces.
            var interfaces = serializedType.GetInterfaces();
            var interfaceFound = AllowedTypes.FirstOrDefault(t => interfaces.Any(IsTypeEqualTo(t)));
            if (interfaceFound != null)
            {
                return true;
            }

            return false;
        }

        private void AllowType(Type type)
        {
            if (type == null)
            {
                return;
            }

            if (!AllowedTypes.Any(IsTypeEqualTo(type)))
            {
                AllowedTypes.Add(type);
            }
        }

        private void DenyType(Type type)
        {
            if (type == null)
            {
                return;
            }

            var typeToRemove = AllowedTypes.FirstOrDefault(IsTypeEqualTo(type));
            AllowedTypes.Remove(typeToRemove);

            if (!DeniedTypes.Any(IsTypeEqualTo(type)))
            {
                DeniedTypes.Add(type);
            }
        }
    }
}

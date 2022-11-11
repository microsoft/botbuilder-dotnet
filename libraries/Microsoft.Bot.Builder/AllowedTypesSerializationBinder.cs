// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        /// <param name="allowedTypes">A list of types to allow this binder to assign upon deserialization.</param>
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

            if (serializedType == null || AllowedTypes.Count == 0)
            {
                return;
            }
            
            if (IsTypeAllowed(serializedType))
            {
                AllowType(serializedType);
            }
        }

        /// <summary>
        /// Given the <paramref name="assemblyName"/> and <paramref name="typeName"/> parameters,
        /// it validates if the resulted <see cref="Type"/> is found in the <see cref="AllowedTypes"/> collection.
        /// <para>
        /// When found, it will return the serialized <see cref="Type"/>.
        /// </para>
        /// <para>
        /// When not found, it will throw an <see cref="InvalidOperationException"/>.
        /// </para>
        /// </summary>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        /// <returns>The <see cref="Type"/> found in the <see cref="AllowedTypes"/> collection.</returns>
        /// <exception cref="InvalidOperationException">When the resulted <see cref="Type"/>
        /// from the <paramref name="assemblyName"/> and <paramref name="typeName"/> parameters
        /// is not found in the <see cref="AllowedTypes"/> collection.</exception>
        public override Type BindToType(string assemblyName, string typeName)
        {
            var resolvedTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", typeName, assemblyName);
            var type = Type.GetType(resolvedTypeName);

            // Preload related type.
            if (IsTypeAllowed(type))
            {
                AllowType(type);
            }

            if (!AllowedTypes.Contains(type))
            {
                ThrowDisallowedTypesError(new List<Type> { type });
            }

            return type;
        }

        /// <summary>
        /// Finds and remove all the '$type' properties within the provided <paramref name="json"/> parameter,
        /// that are not included in the <see cref="AllowedTypes"/> collection.
        /// </summary>
        /// <param name="json">A JSON object.</param>
        public void CleanupTypes(JContainer json)
        {
            if (AllowedTypes.Count == 0)
            {
                return;
            }

            var disallowedTypes = new List<Type>();
            var allowedTypes = AllowedTypes.ToDictionary(e => e.AssemblyQualifiedName);

            // Remove AllowedTypes nested types from the object.
            json.Descendants()
                .OfType<JProperty>()
                .Where(attr => attr.Name == "$type")
                .Select(attr => new
                {
                    Property = attr,
                    Type = Type.GetType(attr.Value.ToString())
                })
                .Where(attr =>
                {
                    if (AllowedTypes.Contains(attr.Type))
                    {
                        return false;
                    }

                    var shouldRemove = attr.Type == null || ShouldRemoveType(attr.Type, attr.Property.Parent, allowedTypes);
                    if (shouldRemove)
                    {
                        return true;
                    }

                    disallowedTypes.Add(attr.Type);
                    return false;
                })
                .ToList()
                .ForEach(attr => attr.Property.Remove());

            if (disallowedTypes.Count > 0)
            {
                ThrowDisallowedTypesError(disallowedTypes);
            }
        }

        private void ThrowDisallowedTypesError(List<Type> types)
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

        private bool ShouldRemoveType(Type child, JContainer parent, IDictionary<string, Type> memory)
        {
            if (parent == null)
            {
                return false;
            }

            var parentTypeName = parent.Type == JTokenType.Object ? parent?.Value<string>("$type") : string.Empty;
            var parentType = Type.GetType(parentTypeName);
            if (parentType == null)
            {
                return ShouldRemoveType(child, parent.Parent, memory);
            }

            if (memory.ContainsKey(parentType.AssemblyQualifiedName))
            {
                var reference = $"{parentType.AssemblyQualifiedName} / {child.AssemblyQualifiedName}";
                if (!memory.ContainsKey(reference))
                {
                    memory.Add(reference, child);
                }

                return true;
            }

            return ShouldRemoveType(parentType, parent.Parent, memory);
        }

        private bool IsTypeAllowed(Type serializedType)
        {
            if (serializedType == null)
            {
                return false;
            }

            // Return Type when found.
            var typeFound = AllowedTypes.FirstOrDefault(e => e.AssemblyQualifiedName == serializedType.AssemblyQualifiedName);
            if (typeFound != null)
            {
                return true;
            }

            // Return Type when it's inside another Type.
            if (serializedType.IsNested)
            {
                if (IsTypeAllowed(serializedType.ReflectedType))
                {
                    return true;
                }
            }

            // Return Type when it has Interfaces.
            var interfaces = serializedType.GetInterfaces();
            var interfaceFound = AllowedTypes.FirstOrDefault(t => interfaces.Any(e => e.AssemblyQualifiedName == t.AssemblyQualifiedName));
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

            if (!AllowedTypes.Contains(type))
            {
                AllowedTypes.Add(type);
            }
        }
    }
}

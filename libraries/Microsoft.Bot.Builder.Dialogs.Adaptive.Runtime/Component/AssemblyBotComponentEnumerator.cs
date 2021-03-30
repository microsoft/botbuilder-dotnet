// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Component
{
    /// <summary>
    /// Retrieve available registered bot components.
    /// </summary>
    internal class AssemblyBotComponentEnumerator : IBotComponentEnumerator
    {
        private readonly AssemblyLoadContext _loadContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyBotComponentEnumerator"/> class.
        /// </summary>
        /// <param name="loadContext">
        /// Context used to discover and load assemblies available to the application runtime.
        /// </param>
        public AssemblyBotComponentEnumerator(AssemblyLoadContext loadContext)
        {
            _loadContext = loadContext ?? throw new ArgumentNullException(nameof(loadContext));
        }

        /// <summary>
        /// Get available bot components.
        /// </summary>
        /// <param name="componentName">Bot component identifier used to retrieve applicable components.</param>
        /// <returns>A collection of available bot componentsfor the specified component name.</returns>
        public IEnumerable<BotComponent> GetComponents(string componentName)
        {
            if (string.IsNullOrWhiteSpace(componentName))
            {
                throw new ArgumentNullException(nameof(componentName));
            }

            var assemblyName = new AssemblyName(componentName);
            Assembly componentAssembly = _loadContext.LoadFromAssemblyName(assemblyName);

            foreach (Type type in componentAssembly.GetTypes())
            {
                // Ensure that the type is non-nested, public and can be assigned to the BotComponent base class.

                if (!typeof(BotComponent).IsAssignableFrom(type) ||
                    !type.IsPublic ||
                    type.IsNested ||
                    type.IsAbstract)
                {
                    continue;
                }

                // Ensure that the type has a public, parameterless constructor.
                ConstructorInfo constructor = type.GetConstructor(
                    bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);

                if (constructor == null)
                {
                    continue;
                }

                // Construct and return the bot component instance.
                if (Activator.CreateInstance(type) is BotComponent component)
                {
                    yield return component;
                }
            }
        }
    }
}

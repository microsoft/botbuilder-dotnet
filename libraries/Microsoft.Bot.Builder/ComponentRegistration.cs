// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// ComponentRegistration is a placeholder class for discovering assets from components. 
    /// </summary>
    /// <remarks>
    /// To make your components available to the system you derive from ComponentRegistration and implement the 
    /// interfaces which definethe components.  These components then are consumed in appropriate places by the 
    /// systems that need them. For example, to add declarative types to the system you simply add class that 
    /// implements IComponentDeclarativeTypes.
    /// <code>
    /// public class MyComponentRegistration : IComponentDeclarativeTypes
    /// {
    ///     public IEnumerable&lt;DeclarativeType&gt;()
    ///     {  
    ///          yield return new DeclarativeType&lt;MyType&gt;("Contoso.MyType");
    ///          ...
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public class ComponentRegistration
    {
        /// <summary>
        /// Gets list of all classes in process which are derived from ComponentRegistration.
        /// </summary>
        /// <remarks>
        /// This is a lazy list because we only want to do this work once.  This calculates this by
        /// using Reflection over all of the assemblies in the AppDomain (and their references) to discover
        /// all of the ComponentRegistrationClasses and build a list of them.
        /// </remarks>
        /// <value>
        /// A list of ComponentRegistration objects.
        /// </value>
        public static Lazy<List<ComponentRegistration>> Components { get; private set; } = new Lazy<List<ComponentRegistration>>(() =>
        {
            void LoadReferencedAssembly(Assembly assembly)
            {
                foreach (AssemblyName name in assembly.GetReferencedAssemblies().Where(a => !a.Name.StartsWith("System")))
                {
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
                    {
                        try
                        {
                            LoadReferencedAssembly(Assembly.Load(name));
                        }
                        catch (Exception err)
                        {
                            Trace.TraceInformation(err.Message);
                        }
                    }
                }
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                LoadReferencedAssembly(assembly);
            }

            var components = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                {
                    try
                    {
                        // verify it's an assembly we can get types from.
                        assembly.GetTypes();
                        return !assembly.GetName().Name.StartsWith("System");
                    }
                    catch (System.Reflection.ReflectionTypeLoadException)
                    {
                        return false;
                    }
                })
                .SelectMany(x => x.GetTypes())
                .Where(type => typeof(ComponentRegistration).IsAssignableFrom(type))
                .Select(t => (ComponentRegistration)Activator.CreateInstance(t))
                .ToList<ComponentRegistration>();

            return components;
        });
    }
}

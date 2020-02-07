using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Bot.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public class ComponentRegistration
    {
        /// <summary>
        /// Gets component registrations.
        /// </summary>
        /// <value>
        /// Component registrations object which implement IComponentRegistration or derived interface.
        /// </value>
        public static Lazy<List<ComponentRegistration>> Registrations { get; private set; } = new Lazy<List<ComponentRegistration>>(() =>
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

            // register custom functions 
            foreach (var component in components.OfType<IComponentExpressionFunctions>())
            {
                foreach (var function in component.GetExpressionEvaluators())
                {
                    ExpressionFunctions.Functions[function.Type] = function;
                }
            }

            return components;
        });
    }
}

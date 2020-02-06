using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Bot.Builder
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

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                    .Where(type => typeof(ComponentRegistration).IsAssignableFrom(type))
                    .Select(t => (ComponentRegistration)Activator.CreateInstance(t))
                    .ToList<ComponentRegistration>();
        });

        ///// <summary>
        ///// Get external resource by key (or by type fullname if no key is provided).
        ///// </summary>
        ///// <typeparam name="T">type of object.</typeparam>
        ///// <param name="dependencies">dependencies.</param>
        ///// <param name="key">key (default is fullname of T).</param>
        ///// <param name="factory">function to create default value, add it and return it as the result.</param>
        ///// <returns>instance of T if it is there or default(T) if not.</returns>
        //public T GetDependency<T>(IDictionary<string, object> dependencies, string key = null, Func<T> factory = null)
        //{
        //    if (key == null)
        //    {
        //        key = typeof(T).FullName;
        //    }

        //    if (dependencies.TryGetValue(key, out object val))
        //    {
        //        return (T)val;
        //    }

        //    if (factory != null)
        //    {
        //        var result = factory();
        //        dependencies.Add(key, result);
        //        return result;
        //    }

        //    return default(T);
        //}
    }
}

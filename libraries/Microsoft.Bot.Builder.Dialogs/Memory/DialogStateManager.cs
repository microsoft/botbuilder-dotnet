// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// The DialogStateManager manages memory scopes and pathresolvers
    /// MemoryScopes are named root level objects, which can exist either in the dialogcontext or off of turn state
    /// PathResolvers allow for shortcut behavior for mapping things like $foo -> dialog.foo
    /// </summary>
    public class DialogStateManager : IDictionary<string, object>
    {
        private static IPathResolver defaultPathResolver = new DefaultPathResolver();

        private readonly DialogContext dialogContext;

        public DialogStateManager(DialogContext dc)
        {
            this.dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
        }

        /// <summary>
        /// Gets the path resolvers used to evaluate memory paths
        /// </summary>
        /// <remarks>
        /// The built in path resolvers are $,#,@,@@,%.  Additional ones can be added here to handle path resolvers around additional scopes
        /// </remarks>
        public static List<IPathResolver> PathResolvers { get; private set; } = new List<IPathResolver>()
        {
            new DollarPathResolver(),
            new HashPathResolver(),
            new AtAtPathResolver(),
            new AtPathResolver(),
            new PercentPathResolver(),
            new DefaultPathResolver()
        };

        /// <summary>
        /// Gets the supported memory scopes for the dialog state manager.  
        /// </summary>
        /// <remarks>
        /// components can extend valid scopes by adding to this list, for example to add top level scopes such as Company, Team, etc.
        /// </remarks>
        public static List<MemoryScope> MemoryScopes { get; private set; } = new List<MemoryScope>()
        {
             new MemoryScope(ScopePath.USER),
             new MemoryScope(ScopePath.CONVERSATION),
             new MemoryScope(ScopePath.TURN),
             new MemoryScope(ScopePath.SETTINGS),
             new DialogMemoryScope()
        };

        public ICollection<string> Keys => MemoryScopes.Select(ms => ms.Name).ToList();

        public ICollection<object> Values => MemoryScopes.Cast<object>().ToList();

        public int Count => MemoryScopes.Count;

        public bool IsReadOnly => true;

        public object this[string key] { get => this.GetValue<object>(key, () => null); set => this.SetValue(key, value); }

        /// <summary>
        /// Get MemoryScope by name
        /// </summary>
        /// <param name="name">name of scope</param>
        /// <returns>memoryscope</returns>
        public static MemoryScope GetMemoryScope(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return MemoryScopes.FirstOrDefault(ms => String.Compare(ms.Name, name, ignoreCase: true) == 0);
        }

        /// <summary>
        /// Get the value from memory using path expression (NOTE: This always returns clone of value)
        /// </summary>
        /// <remarks>This always returns a CLONE of the memory, any modifications to the result of this will not be affect memory</remarks>
        /// <typeparam name="T">the value type to return</typeparam>
        /// <param name="pathExpression">path expression to use</param>
        /// <param name="value">value</param>
        /// <returns>true if found, false if not</returns>
        public bool TryGetValue<T>(string pathExpression, out T value)
        {
            return this.FindResolver(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)))
                .TryGetValue<T>(this.dialogContext, pathExpression, out value);
        }

        /// <summary>
        /// Get the value from memory using path expression (NOTE: This always returns clone of value)
        /// </summary>
        /// <remarks>This always returns a CLONE of the memory, any modifications to the result of this will not be affect memory</remarks>
        /// <typeparam name="T">the value type to return</typeparam>
        /// <param name="pathExpression">path expression to use</param>
        /// <param name="defaultValue">function to give default value if there is none (OPTIONAL)</param>
        /// <returns>result or null if the path is not valid</returns>
        public T GetValue<T>(string pathExpression, Func<T> defaultValue = null)
        {
            if (this.TryGetValue<T>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out T value))
            {
                return value;
            }

            return defaultValue != null ? defaultValue() : default(T);
        }

        /// <summary>
        /// Get a int value from memory using a path expression
        /// </summary>
        /// <param name="pathExpression">path expression</param>
        /// <param name="defaultValue">default value if the value doesn't exist</param>
        /// <returns>value or null if path is not valid</returns>
        public int GetIntValue(string pathExpression, int defaultValue = 0)
        {
            if (this.TryGetValue<int>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out int value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get a bool value from memory using a path expression
        /// </summary>
        /// <param name="pathExpression">pathExpression</param>
        /// <param name="defaultValue">default value if the value doesn't exist</param>
        /// <returns>bool or null if path is not valid</returns>
        public bool GetBoolValue(string pathExpression, bool defaultValue = false)
        {
            if (this.TryGetValue<bool>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out bool value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Set memory to value
        /// </summary>
        /// <param name="pathExpression">path to memory</param>
        /// <param name="value">object to set</param>
        public void SetValue(string pathExpression, object value)
        {
            if (value is Task)
            {
                throw new Exception($"{pathExpression} = You can't pass an unresolved Task to SetValue");
            }

            this.FindResolver(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)))
                .SetValue(this.dialogContext, pathExpression, value);
        }

        /// <summary>
        /// Remove property from memory
        /// </summary>
        /// <param name="pathExpression">path to remove the leaf property</param>
        public void RemoveValue(string pathExpression)
        {
            this.FindResolver(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)))
                .RemoveValue(this.dialogContext, pathExpression);
        }

        /// <summary>
        /// Gets all memoryscopes suitable for logging
        /// </summary>
        /// <returns>object which represents all memory scopes</returns>
        public JObject GetMemorySnapshot()
        {
            var result = new JObject();

            foreach (var scope in DialogStateManager.MemoryScopes)
            {
                result[scope.Name] = JToken.FromObject(scope.GetMemory(this.dialogContext));
            }

            return result;
        }

        public void Add(string key, object value)
        {
            this.SetValue(key, value);
        }

        public bool ContainsKey(string key)
        {
            return DialogStateManager.MemoryScopes.Any(ms => ms.Name.ToLower() == key.ToLower());
        }

        public bool Remove(string key)
        {
            this.RemoveValue(key);
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            return this.TryGetValue<object>(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.SetValue(item.Key, item.Value);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var ms in MemoryScopes)
            {
                array[arrayIndex++] = new KeyValuePair<string, object>(ms.Name, ms);
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var ms in MemoryScopes)
            {
                yield return new KeyValuePair<string, object>(ms.Name, ms);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var ms in MemoryScopes)
            {
                yield return new KeyValuePair<string, object>(ms.Name, ms);
            }
        }

        private IPathResolver FindResolver(string path)
        {
            return PathResolvers.FirstOrDefault(resolver => resolver.Matches(path)) ?? defaultPathResolver;
        }
    }
}

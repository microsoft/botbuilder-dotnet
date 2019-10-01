// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// The DialogStateManager manages memory scopes and pathresolvers
    /// MemoryScopes are named root level objects, which can exist either in the dialogcontext or off of turn state
    /// PathResolvers allow for shortcut behavior for mapping things like $foo -> dialog.foo
    /// </summary>
    public class DialogStateManager : IDictionary<string, object>
    {
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
             new MemoryScope(ScopePath.SETTINGS, isReadOnly: true),
             new DialogMemoryScope(),
             new ClassMemoryScope(),
             new ThisMemoryScope()
        };

        public ICollection<string> Keys => MemoryScopes.Select(ms => ms.Name).ToList();

        public ICollection<object> Values => MemoryScopes.Select(ms => ms.GetMemory(this.dialogContext)).ToList();

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

            return MemoryScopes.FirstOrDefault(ms => string.Compare(ms.Name, name, ignoreCase: true) == 0);
        }

        /// <summary>
        /// ResolveMemoryScope will find the MemoryScope for root part of path and adjust path to be subpath
        /// </summary>
        /// <param name="path">incoming path will be resolved to scope and adjusted to subpath</param>
        /// <returns>memoryscope</returns>
        public virtual MemoryScope ResolveMemoryScope(ref string path)
        {
            string scope = path;
            var index = path.IndexOf(".");
            if (index > 0)
            {
                scope = path.Substring(0, index);
                var memoryScope = DialogStateManager.GetMemoryScope(scope);
                if (memoryScope != null)
                {
                    path = path.Substring(index + 1);
                    return memoryScope;
                }
            }

            // could be User[foo] path 
            index = path.IndexOf("[");
            if (index > 0)
            {
                scope = path.Substring(0, index);
                path = path.Substring(index);
                return DialogStateManager.GetMemoryScope(scope) ?? throw new ArgumentOutOfRangeException(GetBadScopeMessage(path));
            }
            else
            {
                path = string.Empty;
                return DialogStateManager.GetMemoryScope(scope) ?? throw new ArgumentOutOfRangeException(GetBadScopeMessage(path));
            }
        }

        /// <summary>
        /// Transform the path using the registered PathTransformers
        /// </summary>
        /// <param name="path">path</param>
        /// <returns>transformed paths</returns>
        public virtual string TransformPath(string path)
        {
            foreach (var pathResolver in PathResolvers)
            {
                path = pathResolver.TransformPath(path);
            }

            return path;
        }

        /// <summary>
        /// Get the value from memory using path expression (NOTE: This always returns clone of value)
        /// </summary>
        /// <remarks>This always returns a CLONE of the memory, any modifications to the result of this will not be affect memory</remarks>
        /// <typeparam name="T">the value type to return</typeparam>
        /// <param name="path">path expression to use</param>
        /// <param name="value">value</param>
        /// <returns>true if found, false if not</returns>
        public bool TryGetValue<T>(string path, out T value)
        {
            path = this.TransformPath(path ?? throw new ArgumentNullException(nameof(path)));
            var memoryScope = this.ResolveMemoryScope(ref path);
            var memory = memoryScope.GetMemory(this.dialogContext);
            return ObjectPath.TryGetPathValue<T>(memory, path, out value);
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
        /// <param name="path">path to memory</param>
        /// <param name="value">object to set</param>
        public void SetValue(string path, object value)
        {
            if (value is Task)
            {
                throw new Exception($"{path} = You can't pass an unresolved Task to SetValue");
            }

            path = this.TransformPath(path ?? throw new ArgumentNullException(nameof(path)));
            var memoryScope = this.ResolveMemoryScope(ref path);
            if (path == string.Empty)
            {
                memoryScope.SetMemory(this.dialogContext, value);
            }
            else
            {
                var memory = memoryScope.GetMemory(this.dialogContext);
                ObjectPath.SetPathValue(memory, path, value);
            }
        }

        /// <summary>
        /// Remove property from memory
        /// </summary>
        /// <param name="path">path to remove the leaf property</param>
        public void RemoveValue(string path)
        {
            path = this.TransformPath(path ?? throw new ArgumentNullException(nameof(path)));
            var memoryScope = this.ResolveMemoryScope(ref path);
            var memory = memoryScope.GetMemory(this.dialogContext);
            ObjectPath.RemovePathValue(memory, path);
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
                var memory = scope.GetMemory(this.dialogContext);
                if (memory != null)
                {
                    result[scope.Name] = JToken.FromObject(memory);
                }
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
                array[arrayIndex++] = new KeyValuePair<string, object>(ms.Name, ms.GetMemory(this.dialogContext));
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
                yield return new KeyValuePair<string, object>(ms.Name, ms.GetMemory(this.dialogContext));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var ms in MemoryScopes)
            {
                yield return new KeyValuePair<string, object>(ms.Name, ms.GetMemory(this.dialogContext));
            }
        }

        private static string GetBadScopeMessage(string path)
        {
            return $"'{path}' does not match memory scopes:{string.Join(",", DialogStateManager.MemoryScopes.Select(ms => ms.Name))}";
        }
    }
}

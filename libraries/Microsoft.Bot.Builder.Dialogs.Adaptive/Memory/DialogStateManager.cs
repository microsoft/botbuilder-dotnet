// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Expressions.Memory;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// The DialogStateManager manages memory scopes and pathresolvers
    /// MemoryScopes are named root level objects, which can exist either in the dialogcontext or off of turn state
    /// PathResolvers allow for shortcut behavior for mapping things like $foo -> dialog.foo.
    /// </summary>
    public class DialogStateManager : IDictionary<string, object>, IMemory
    {
        /// <summary>
        /// Information for tracking when path was last modified.
        /// </summary>
        private const string PathTracker = "dialog._tracker.paths";

        private static readonly char[] Separators = { ',', '[' };

        private readonly DialogContext dialogContext;
        private int version = 0;

        public DialogStateManager(DialogContext dc)
        {
            dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
            this.Configuration = dc.Context.TurnState.Get<DialogStateManagerConfiguration>() ?? CreateStandardConfiguration();
        }

        public DialogStateManagerConfiguration Configuration { get; set; }

        public ICollection<string> Keys => Configuration.MemoryScopes.Select(ms => ms.Name).ToList();

        public ICollection<object> Values => Configuration.MemoryScopes.Select(ms => ms.GetMemory(dialogContext)).ToList();

        public int Count => Configuration.MemoryScopes.Count;

        public bool IsReadOnly => true;

        public object this[string key]
        {
            get => GetValue<object>(key, () => null);
            set
            {
                if (key.IndexOfAny(Separators) == -1)
                {
                    // Root is handled by SetMemory rather than SetValue
                    var scope = GetMemoryScope(key) ?? throw new ArgumentOutOfRangeException(GetBadScopeMessage(key));
                    scope.SetMemory(this.dialogContext, value);
                }
                else
                {
                    SetValue(key, value);
                }
            }
        }

        public static DialogStateManagerConfiguration CreateStandardConfiguration(ConversationState conversationState = null, UserState userState = null)
        {
            var result = new DialogStateManagerConfiguration()
            {
                PathResolvers = new List<IPathResolver>
                {
                    new DollarPathResolver(),
                    new HashPathResolver(),
                    new AtAtPathResolver(),
                    new AtPathResolver(),
                    new PercentPathResolver()
                },
                MemoryScopes = new List<MemoryScope>
                {
                    new TurnMemoryScope(),
                    new SettingsMemoryScope(),
                    new DialogMemoryScope(),
                    new DialogClassMemoryScope(),
                    new ClassMemoryScope(),
                    new ThisMemoryScope()
                }
            };

            if (conversationState != null)
            {
                result.MemoryScopes.Add(new ConversationMemoryScope(conversationState));
            }

            if (userState != null)
            {
                result.MemoryScopes.Add(new UserMemoryScope(userState));
            }

            return result;
        }

        /// <summary>
        /// Get MemoryScope by name.
        /// </summary>
        /// <param name="name">Name of scope.</param>
        /// <returns>A memory scope.</returns>
        public MemoryScope GetMemoryScope(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return Configuration.MemoryScopes.FirstOrDefault(ms => string.Compare(ms.Name, name, ignoreCase: true) == 0);
        }

        /// <summary>
        /// Version help caller to identify the updates and decide cache or not.
        /// </summary>
        /// <returns>Current version.</returns>
        string IMemory.Version()
        {
            return version.ToString();
        }

        /// <summary>
        /// ResolveMemoryScope will find the MemoryScope for and return the remaining path.
        /// </summary>
        /// <param name="path">Incoming path to resolve to scope and remaining path.</param>
        /// <param name="remainingPath">Remaining subpath in scope.</param>
        /// <returns>The memory scope.</returns>
        public virtual MemoryScope ResolveMemoryScope(string path, out string remainingPath)
        {
            var scope = path;
            var dot = path.IndexOf(".");
            if (dot > 0)
            {
                scope = path.Substring(0, dot);
                var memoryScope = GetMemoryScope(scope);
                if (memoryScope != null)
                {
                    remainingPath = path.Substring(dot + 1);
                    return memoryScope;
                }
            }

            remainingPath = string.Empty;
            return GetMemoryScope(scope) ?? throw new ArgumentOutOfRangeException(GetBadScopeMessage(path));
        }

        /// <summary>
        /// Transform the path using the registered PathTransformers.
        /// </summary>
        /// <param name="path">Path to transform.</param>
        /// <returns>The transformed path.</returns>
        public virtual string TransformPath(string path)
        {
            foreach (var pathResolver in Configuration.PathResolvers)
            {
                path = pathResolver.TransformPath(path);
            }

            return path;
        }

        /// <summary>
        /// Get the value from memory using path expression (NOTE: This always returns clone of value).
        /// </summary>
        /// <remarks>This always returns a CLONE of the memory, any modifications to the result of this will not be affect memory.</remarks>
        /// <typeparam name="T">the value type to return.</typeparam>
        /// <param name="path">path expression to use.</param>
        /// <param name="value">Value out parameter.</param>
        /// <returns>True if found, false if not.</returns>
        public bool TryGetValue<T>(string path, out T value)
        {
            value = default;
            path = TransformPath(path ?? throw new ArgumentNullException(nameof(path)));

            var memoryScope = ResolveMemoryScope(path, out var remainingPath);
            if (memoryScope == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(remainingPath))
            {
                var memory = memoryScope.GetMemory(this.dialogContext);
                if (memory == null)
                {
                    return false;
                }

                value = ObjectPath.MapValueTo<T>(memory);
                return true;
            }

            // HACK to support .First() retrieval on turn.recognized.entities.foo, replace with Expressions once expression ship
            var iFirst = path.ToLower().LastIndexOf(".first()");
            if (iFirst >= 0)
            {
                path = path.Substring(0, iFirst);
                memoryScope = ResolveMemoryScope(path, out remainingPath);
                var memory = memoryScope.GetMemory(dialogContext);
                return TryGetFirstNestedValue(ref value, ref remainingPath, memory);
            }

            return ObjectPath.TryGetPathValue(this, path, out value);
        }

        /// <summary>
        /// Get the value from memory using path expression (NOTE: This always returns clone of value).
        /// </summary>
        /// <remarks>This always returns a CLONE of the memory, any modifications to the result of this will not be affect memory.</remarks>
        /// <typeparam name="T">The value type to return.</typeparam>
        /// <param name="pathExpression">Path expression to use.</param>
        /// <param name="defaultValue">Function to give default value if there is none (OPTIONAL).</param>
        /// <returns>Result or null if the path is not valid.</returns>
        public T GetValue<T>(string pathExpression, Func<T> defaultValue = null)
        {
            if (TryGetValue<T>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out var value))
            {
                return value;
            }

            return defaultValue != null ? defaultValue() : default;
        }

        /// <summary>
        /// Get a int value from memory using a path expression.
        /// </summary>
        /// <param name="pathExpression">Path expression.</param>
        /// <param name="defaultValue">Default value if the value doesn't exist.</param>
        /// <returns>Value or null if path is not valid.</returns>
        public int GetIntValue(string pathExpression, int defaultValue = 0)
        {
            if (TryGetValue<int>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get a bool value from memory using a path expression.
        /// </summary>
        /// <param name="pathExpression">The path expression.</param>
        /// <param name="defaultValue">Default value if the value doesn't exist.</param>
        /// <returns>Bool or null if path is not valid.</returns>
        public bool GetBoolValue(string pathExpression, bool defaultValue = false)
        {
            if (TryGetValue<bool>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Set memory to value.
        /// </summary>
        /// <param name="path">Path to memory.</param>
        /// <param name="value">Object to set.</param>
        public void SetValue(string path, object value)
        {
            if (value is Task)
            {
                throw new Exception($"{path} = You can't pass an unresolved Task to SetValue");
            }

            path = this.TransformPath(path ?? throw new ArgumentNullException(nameof(path)));
            if (TrackChange(path, value))
            {
                ObjectPath.SetPathValue(this, path, value);
            }

            // Every set will increase version
            version++;
        }

        /// <summary>
        /// Remove property from memory.
        /// </summary>
        /// <param name="path">Path to remove the leaf property.</param>
        public void RemoveValue(string path)
        {
            path = this.TransformPath(path ?? throw new ArgumentNullException(nameof(path)));
            if (TrackChange(path, null))
            {
                ObjectPath.RemovePathValue(this, path);
            }
        }

        /// <summary>
        /// Gets all memoryscopes suitable for logging.
        /// </summary>
        /// <returns>object which represents all memory scopes.</returns>
        public JObject GetMemorySnapshot()
        {
            var result = new JObject();

            foreach (var scope in Configuration.MemoryScopes.Where(ms => ms.IncludeInSnapshot == true))
            {
                var memory = scope.GetMemory(dialogContext);
                if (memory != null)
                {
                    result[scope.Name] = JToken.FromObject(memory);
                }
            }

            return result;
        }

        /// <summary>
        /// Load all of the scopes.
        /// </summary>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>Task.</returns>
        public async Task LoadAllScopesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var scope in this.Configuration.MemoryScopes)
            {
                await scope.LoadAsync(this.dialogContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Save all changes for all scopes.
        /// </summary>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>Task.</returns>
        public async Task SaveAllChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var scope in this.Configuration.MemoryScopes)
            {
                await scope.SaveChangesAsync(this.dialogContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Delete the memory for a scope.
        /// </summary>
        /// <param name="name">name of the scope.</param>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>Task.</returns>
        public async Task DeleteScopesMemoryAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            name = name.ToLower();
            var scope = this.Configuration.MemoryScopes.SingleOrDefault(s => s.Name.ToLower() == name);
            if (scope != null)
            {
                await scope.DeleteAsync(this.dialogContext, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Add(string key, object value)
        {
            throw new NotSupportedException();
        }

        public bool ContainsKey(string key)
        {
            return Configuration.MemoryScopes.Any(ms => ms.Name.ToLower() == key.ToLower());
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            value = default;
            if (this.TryGetValue<object>(key, out var result))
            {
                value = result;
            }

            return true;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var ms in Configuration.MemoryScopes)
            {
                array[arrayIndex++] = new KeyValuePair<string, object>(ms.Name, ms.GetMemory(dialogContext));
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var ms in Configuration.MemoryScopes)
            {
                yield return new KeyValuePair<string, object>(ms.Name, ms.GetMemory(dialogContext));
            }
        }

        /// <summary>
        /// Track when specific paths are changed.
        /// </summary>
        /// <param name="paths">Paths to track.</param>
        /// <returns>Normalized paths to pass to <see cref="AnyPathChanged"/>.</returns>
        public List<string> TrackPaths(IEnumerable<string> paths)
        {
            var allPaths = new List<string>();
            foreach (var path in paths)
            {
                var tpath = TransformPath(path);

                // Track any path that resolves to a constant path
                if (ObjectPath.TryResolvePath(this, tpath, out var segments))
                {
                    var npath = string.Join("_", segments);
                    SetValue(PathTracker + "." + npath, 0);
                    allPaths.Add(npath);
                }
            }

            return allPaths;
        }

        /// <summary>
        /// Check to see if any path has changed since watermark.
        /// </summary>
        /// <param name="counter">Time counter to compare to.</param>
        /// <param name="paths">Paths from <see cref="TrackPaths"/> to check.</param>
        /// <returns>True if any path has changed since counter.</returns>
        public bool AnyPathChanged(uint counter, IEnumerable<string> paths)
        {
            var found = false;
            foreach (var path in paths)
            {
                if (GetValue<uint>(PathTracker + "." + path) > counter)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var ms in Configuration.MemoryScopes)
            {
                yield return new KeyValuePair<string, object>(ms.Name, ms.GetMemory(dialogContext));
            }
        }

        private string GetBadScopeMessage(string path)
        {
            return $"'{path}' does not match memory scopes:{string.Join(",", Configuration.MemoryScopes.Select(ms => ms.Name))}";
        }

        private bool TrackChange(string path, object value)
        {
            var hasPath = false;
            if (ObjectPath.TryResolvePath(this, path, out var segments))
            {
                var root = segments.Count() > 1 ? segments[1] as string : string.Empty;

                // Skip _* as first scope, i.e. _adaptive, _tracker, ...
                if (!root.StartsWith("_"))
                {
                    // Convert to a simple path with _ between segments
                    var pathName = string.Join("_", segments);
                    var trackedPath = $"{PathTracker}.{pathName}";
                    uint? counter = null;
                    void Update()
                    {
                        if (TryGetValue<uint>(trackedPath, out var lastChanged))
                        {
                            if (!counter.HasValue)
                            {
                                counter = GetValue<uint>(DialogPath.EventCounter);
                            }

                            SetValue(trackedPath, counter.Value);
                        }
                    }

                    Update();
                    if (value is object obj)
                    {
                        // For an object we need to see if any children path are being tracked
                        void CheckChildren(string property, object value)
                        {
                            // Add new child segment
                            trackedPath += "_" + property.ToLower();
                            Update();
                            if (value is object child)
                            {
                                ObjectPath.ForEachProperty(child, CheckChildren);
                            }

                            // Remove added child segment
                            trackedPath = trackedPath.Substring(0, trackedPath.LastIndexOf('_'));
                        }

                        ObjectPath.ForEachProperty(obj, CheckChildren);
                    }
                }

                hasPath = true;
            }

            return hasPath;
        }

        private bool TryGetFirstNestedValue<T>(ref T value, ref string remainingPath, object memory)
        {
            if (ObjectPath.TryGetPathValue<JArray>(memory, $"{remainingPath}", out var array))
            {
                if (array != null && array.Count > 0)
                {
                    var first = array[0] as JArray;
                    if (first != null)
                    {
                        if (first.Count > 0)
                        {
                            var second = first[0];
                            value = ObjectPath.MapValueTo<T>(second);
                            return true;
                        }

                        return false;
                    }

                    value = ObjectPath.MapValueTo<T>(array[0]);
                    return true;
                }
            }

            return false;
        }
    }
}

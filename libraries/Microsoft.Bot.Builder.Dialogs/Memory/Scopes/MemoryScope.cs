// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// MemoryScope represents a named memory scope stored in TurnState["MemoryScopes"]
    /// It is responsible for using the DialogContext to bind to the object for it. 
    /// The default MemoryScope is stored in TurnState[MEMORYSCOPESKEY][Name]
    /// Example: User memory scope is tracked in dc.Context.TurnState.MemoryScopes.User
    /// </summary>
    public class MemoryScope
    {
        public const string EVENTCOUNTER = "dialog.eventCounter";

        public const string TRACKER = "dialog.tracker";

        private const string MEMORYSCOPESKEY = "MemoryScopes";

        public MemoryScope(string name, bool isReadOnly = false)
        {
            this.IsReadOnly = isReadOnly;
            this.Name = name;
        }

        /// <summary>
        /// Delegate for when a path in memory is written to.
        /// </summary>
        /// <param name="dc">Context being modified.</param>
        /// <param name="path">Path that changed.</param>
        /// <param name="newValue">New value of path.</param>
        public delegate void PathChanged(DialogContext dc, string path, object newValue);

        /// <summary>
        /// Gets listener for trackning changes to memory scope.
        /// </summary>
        public PathChanged ChangeListener { get; private set; }

        /// <summary>
        /// Gets or sets name of the scope
        /// </summary>
        public string Name { get; set; }

        protected string ChangePath => ScopePath.TURN + ".listener." + Name;

        public static List<string> Track(DialogContext dc, IEnumerable<string> paths)
        {
            var allPaths = new List<string>();
            var count = dc.State.GetValue<uint>(EVENTCOUNTER);
            foreach (var path in paths)
            {
                var tpath = dc.State.TransformPath(path);
                var parts = tpath.ToLower().Split('.');
                // Don't track root memory scope
                var npath = parts[0];
                for (var i = 1; i < parts.Length; ++i)
                {
                    npath += "_" + parts[i];
                    dc.State.SetValue(TRACKER + "." + npath, count);
                    allPaths.Add(npath);
                }
            }

            return allPaths;
        }

        public static void OnPathChanged(DialogContext dc, string path, object value)
        {
            var pathName = TRACKER + "." + path.ToLower().Replace('.', '_');
            if (dc.State.ContainsKey(pathName))
            {
                dc.State.SetValue(pathName, dc.State.GetValue<uint>(EVENTCOUNTER));
            }
        }

        public static bool AnyChanged(DialogStateManager state, uint count, IEnumerable<string> paths)
        {
            var found = false;
            foreach (var path in paths)
            {
                if (state.GetValue<uint>(TRACKER + "." + path) > count)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this memory scope mutable.
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        /// <summary>
        /// Get the backing memory for this scope
        /// </summary>
        /// <param name="dc">dc</param>
        /// <returns>memory for the scope</returns>
        public virtual object GetMemory(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            var scopesMemory = GetScopesMemory(dc.Context);
            if (!scopesMemory.TryGetValue(this.Name, out object memory))
            {
                memory = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                scopesMemory[this.Name] = memory;
            }

            return memory;
        }

        /// <summary>
        /// Changes the backing object for the memory scope
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="memory">memory</param>
        public virtual void SetMemory(DialogContext dc, object memory)
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException("You cannot set the memory for a readonly memory scope");
            }

            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (memory == null)
            {
                throw new ArgumentNullException(nameof(memory));
            }

            var namedScopes = GetScopesMemory(dc.Context);
            namedScopes[this.Name] = memory;
        }

        public virtual void RaiseChange(DialogContext dc, string path, object value)
        {
            var pathName = TRACKER + "." + path.ToLower().Replace('.', '_');
            if (dc.State.TryGetValue<uint>(pathName, out var lastChanged))
            {
                dc.State.SetValue(pathName, dc.State.GetValue<uint>(EVENTCOUNTER));
            }
        }

        /// <summary>
        /// Get Turn Scopes memory
        /// </summary>
        /// <param name="context">turn context</param>
        /// <returns>scopes object</returns>
        internal static Dictionary<string, object> GetScopesMemory(ITurnContext context)
        {
            Dictionary<string, object> namedScopes;
            if (!context.TurnState.TryGetValue(MEMORYSCOPESKEY, out object val))
            {
                namedScopes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                context.TurnState[MEMORYSCOPESKEY] = namedScopes;
            }
            else
            {
                namedScopes = (Dictionary<string, object>)val;
            }

            return namedScopes;
        }
    }
}

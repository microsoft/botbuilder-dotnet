// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

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
        private const string MEMORYSCOPESKEY = "MemoryScopes";

        public MemoryScope(string name, bool isReadOnly=false)
        {
            this.IsReadOnly = isReadOnly;
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets name of the scope
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets whether this memory scope settable
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

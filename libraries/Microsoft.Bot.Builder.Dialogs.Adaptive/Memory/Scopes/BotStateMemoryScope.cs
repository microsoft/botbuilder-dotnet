// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory.Scopes
{
    /// <summary>
    /// BotStateMemoryScope represents a BotState scoped memory.
    /// </summary>
    /// <remarks>This relies on the BotState object being accessible from turnContext.TurnState.Get&lt;T&gt;().</remarks>
    /// <typeparam name="T">BotState type.</typeparam>
    public class BotStateMemoryScope<T> : MemoryScope
        where T : BotState
    {
        private string propertyName;
        private IStatePropertyAccessor<JObject> property;
        private JObject state;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotStateMemoryScope{T}"/> class.
        /// </summary>
        /// <param name="name">name of the property.</param>
        /// <param name="propertyName">optional propertyname.</param>
        public BotStateMemoryScope(string name, string propertyName = null)
            : base(name, includeInSnapshot: true)
        {
            this.propertyName = propertyName ?? name ?? throw new ArgumentNullException("BotStateMemoryScope must have name or propertyname");
        }

        /// <summary>
        /// Get the backing memory for this scope.
        /// </summary>
        /// <param name="dialogContext">dc.</param>
        /// <returns>memory for the scope.</returns>
        public override object GetMemory(DialogContext dialogContext)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException($"{nameof(dialogContext)} is null");
            }

            return this.state;
        }

        /// <summary>
        /// Changes the backing object for the memory scope.
        /// </summary>
        /// <param name="dialogContext">dc.</param>
        /// <param name="memory">memory.</param>
        public override void SetMemory(DialogContext dialogContext, object memory)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException($"{nameof(dialogContext)} is null");
            }

            this.state = JObject.FromObject(memory);
        }

        public override async Task LoadAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default)
        {
            EnsurePropertyAccessor(dialogContext);
            if (this.property != null)
            {
                this.state = await this.property.GetAsync(dialogContext.Context, () => new JObject(), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.state = new JObject();
            }
        }

        public override async Task SaveChangesAsync(DialogContext dialogContext, bool force = false, CancellationToken cancellationToken = default)
        {
            EnsurePropertyAccessor(dialogContext);

            if (this.property != null)
            {
                await this.property.SetAsync(dialogContext.Context, this.state, cancellationToken).ConfigureAwait(false);
            }
        }

        public override async Task DeleteAsync(DialogContext dialogContext, CancellationToken cancellationToken = default)
        {
            EnsurePropertyAccessor(dialogContext);

            if (this.property != null)
            {
                await this.property.DeleteAsync(dialogContext.Context, cancellationToken).ConfigureAwait(false);
            }
        }

        private void EnsurePropertyAccessor(DialogContext dialogContext)
        {
            if (this.property == null)
            {
                var botState = dialogContext.Context.TurnState.Get<T>();
                if (botState != null)
                {
                    this.property = botState.CreateProperty<JObject>(this.propertyName);
                }
            }
        }
    }
}

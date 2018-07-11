// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class RootFrame : IFrame
    {
        private readonly string _cacheKey = $"RootFrame-{Guid.NewGuid()}";
        private readonly FrameDefinition _definition;

        public RootFrame(IStorage storage, FrameDefinition definition)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            _definition = definition ?? throw new ArgumentNullException(nameof(definition));

            if (string.IsNullOrWhiteSpace(definition.NameSpace))
            {
                throw new ArgumentException(nameof(definition.NameSpace));
            }

            if (string.IsNullOrWhiteSpace(definition.Scope))
            {
                throw new ArgumentException(nameof(definition.Scope));
            }

            if (definition.SlotDefinitions != null)
            {
                foreach (var slotDefinition in definition.SlotDefinitions)
                {
                    // Todo: This is strange code. Refactor.
                    new Slot(this, slotDefinition);
                }
            }
        }

        public IFrame Parent => null;

        public string Scope => _definition.Scope;

        public string Namespace => _definition.NameSpace;

        public void AddSlot(IReadWriteSlot slot)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(TurnContext context, bool accessed = false)
        {
            throw new NotImplementedException();
        }

        public Task SlotValueChangedAsync(TurnContext context, string[] tags, object value)
        {
            throw new NotImplementedException();
        }
    }
}

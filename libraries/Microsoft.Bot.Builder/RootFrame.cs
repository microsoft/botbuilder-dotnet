// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class RootFrame : IFrame
    {
        private readonly string _cacheKey = $"RootFrame-{Guid.NewGuid()}";
        private readonly FrameDefinition _definition;
        private readonly Dictionary<string, IReadWriteSlot> _slots = new Dictionary<string, IReadWriteSlot>();

        public RootFrame(FrameDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (string.IsNullOrWhiteSpace(definition.NameSpace))
            {
                throw new ArgumentException(nameof(definition.NameSpace));
            }

            if (string.IsNullOrWhiteSpace(definition.Scope))
            {
                throw new ArgumentException(nameof(definition.Scope));
            }

            _definition = definition;

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
            if (slot == null)
            {
                throw new ArgumentNullException(nameof(slot));
            }

            if (slot.Definition == null)
            {
                throw new ArgumentNullException(nameof(slot.Definition));
            }

            if (string.IsNullOrWhiteSpace(slot.Definition.Name))
            {
                throw new ArgumentNullException(nameof(slot.Definition.Name));
            }

            if (slot.Frame != this)
            {
                throw new InvalidOperationException($"RootFrame.addSlot(): The slot named '{slot.Definition.Name}' has already been added to a different frame.");
            }

            if (_slots.ContainsKey(slot.Definition.Name))
            {
                throw new InvalidOperationException($"RootFrame.addSlot(): A slot named '{slot.Definition.Name}' has already been added to the current frame.");
            }

            _slots[slot.Definition.Name] = slot;
        }

        public IReadWriteSlot GetSlot(string slotName)
        {
            if (string.IsNullOrWhiteSpace(slotName))
            {
                throw new ArgumentNullException(nameof(slotName));
            }

            if (_slots.TryGetValue(slotName, out var slot))
            {
                return slot;
            }
            else
            {
                throw new KeyNotFoundException($"RootFrame.getSlot(): A slot named '{slotName}' couldn't be found.");
            }
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

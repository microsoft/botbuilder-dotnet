// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Component;

namespace Microsoft.Bot.Builder.Runtime.Tests.Components
{
    public class TestBotComponentEnumerator : IBotComponentEnumerator
    {
        private readonly IDictionary<string, ICollection<BotComponent>> _components;

        public TestBotComponentEnumerator(IDictionary<string, ICollection<BotComponent>> components = null)
        {
            _components = components ?? new Dictionary<string, ICollection<BotComponent>>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<BotComponent> GetComponents(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                throw new ArgumentNullException(nameof(componentName));
            }

            if (!_components.TryGetValue(componentName, out ICollection<BotComponent> matchingComponents))
            {
                yield break;
            }

            foreach (BotComponent component in matchingComponents)
            {
                yield return component;
            }
        }
    }
}

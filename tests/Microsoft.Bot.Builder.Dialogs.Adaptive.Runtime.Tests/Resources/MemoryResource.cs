// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Runtime.Tests.Resources
{
    public abstract class MemoryResource : Resource
    {
        protected MemoryResource(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            this.FullName = id;
            this.Id = id;
        }
    }
}

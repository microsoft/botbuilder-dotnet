// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Microsoft.Bot.Builder.Integration.Runtime;

namespace Microsoft.Bot.Builder.Runtime.Tests.Fixtures
{
    public class ComponentRegistrationsFixture : IDisposable
    {
        public ComponentRegistrationsFixture()
        {
            ComponentRegistrations.Add();
        }

        public void Dispose()
        {
        }
    }
}

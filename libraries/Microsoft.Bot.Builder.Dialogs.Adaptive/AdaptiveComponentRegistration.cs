// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// <see cref="ComponentRegistration"/> implementation for adaptive components.
    /// </summary>
    [Obsolete("Use `AdaptiveBotComponent` instead.")]
    public class AdaptiveComponentRegistration : DeclarativeComponentRegistrationBridge<AdaptiveBotComponent>
    {
    }
}

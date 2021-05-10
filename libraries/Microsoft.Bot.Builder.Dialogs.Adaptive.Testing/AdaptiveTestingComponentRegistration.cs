// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Component registration for AdaptiveTesting resources.
    /// </summary>
    /// <remarks>This should be the last registration since it may add testing overrides for other components.</remarks>
    [Obsolete("Use `AdaptiveTestingBotComponent`.")]
    public class AdaptiveTestingComponentRegistration : DeclarativeComponentRegistrationBridge<AdaptiveTestingBotComponent>
    {
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Registers declarative kinds.
    /// </summary>
    [Obsolete("Use `DeclarativeBotComponent`.")]
    public class DeclarativeComponentRegistration : DeclarativeComponentRegistrationBridge<DeclarativeBotComponent>
    {
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Class which contains registration of components for QnAMaker.
    /// </summary>
    [Obsolete("Use `QnaMakerBotComponent`.")]
    public class QnAMakerComponentRegistration : DeclarativeComponentRegistrationBridge<QnAMakerBotComponent>
    {
    }
}

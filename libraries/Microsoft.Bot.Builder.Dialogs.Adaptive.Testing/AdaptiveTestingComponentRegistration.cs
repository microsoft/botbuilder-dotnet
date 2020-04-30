// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    public class AdaptiveTestingComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            // Action
            yield return new DeclarativeType<AssertCondition>(AssertCondition.Kind);

            // test actions
            yield return new DeclarativeType<TestScript>(TestScript.Kind);
            yield return new DeclarativeType<UserSays>(UserSays.Kind);
            yield return new DeclarativeType<UserTyping>(UserTyping.Kind);
            yield return new DeclarativeType<UserConversationUpdate>(UserConversationUpdate.Kind);
            yield return new DeclarativeType<UserActivity>(UserActivity.Kind);
            yield return new DeclarativeType<UserDelay>(UserDelay.Kind);
            yield return new DeclarativeType<AssertReply>(AssertReply.Kind);
            yield return new DeclarativeType<AssertReplyOneOf>(AssertReplyOneOf.Kind);
            yield return new DeclarativeType<AssertReplyActivity>(AssertReplyActivity.Kind);
        }

        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new InterfaceConverter<TestAction>(resourceExplorer, sourceContext);
        }
    }
}

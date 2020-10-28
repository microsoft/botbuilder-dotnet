// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.HttpRequestMocks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.UserTokenMocks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing
{
    /// <summary>
    /// Component registration for AdaptiveTesting resources.
    /// </summary>
    /// <remarks>This should be the last registration since it may add testing overrides for other components.</remarks>
    public class AdaptiveTestingComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <inheritdoc/>
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            // Actions for within normal bot flow
            yield return new DeclarativeType<AssertCondition>(AssertCondition.Kind);

            // Test script actions
            yield return new DeclarativeType<TestScript>(TestScript.Kind);
            yield return new DeclarativeType<UserSays>(UserSays.Kind);
            yield return new DeclarativeType<UserTyping>(UserTyping.Kind);
            yield return new DeclarativeType<UserConversationUpdate>(UserConversationUpdate.Kind);
            yield return new DeclarativeType<UserActivity>(UserActivity.Kind);
            yield return new DeclarativeType<UserDelay>(UserDelay.Kind);
            yield return new DeclarativeType<AssertReply>(AssertReply.Kind);
            yield return new DeclarativeType<AssertReplyOneOf>(AssertReplyOneOf.Kind);
            yield return new DeclarativeType<AssertReplyActivity>(AssertReplyActivity.Kind);
            yield return new DeclarativeType<AssertNoActivity>(AssertNoActivity.Kind); 
            yield return new DeclarativeType<MemoryAssertions>(MemoryAssertions.Kind);
            yield return new DeclarativeType<HttpRequestSequenceMock>(HttpRequestSequenceMock.Kind);
            yield return new DeclarativeType<UserTokenBasicMock>(UserTokenBasicMock.Kind);
            yield return new DeclarativeType<SetProperties>(SetProperties.Kind);
            yield return new DeclarativeType<CustomEvent>(CustomEvent.Kind);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new InterfaceConverter<TestAction>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<HttpRequestMock>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<UserTokenMock>(resourceExplorer, sourceContext);
        }
    }
}

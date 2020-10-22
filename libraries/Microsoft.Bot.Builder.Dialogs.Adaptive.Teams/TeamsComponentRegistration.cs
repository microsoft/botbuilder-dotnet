// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams
{
    public class TeamsComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            // Conditionals
            yield return new DeclarativeType<OnTeamsAppBasedLinkQuery>(OnTeamsAppBasedLinkQuery.Kind);
            yield return new DeclarativeType<OnTeamsCardAction>(OnTeamsCardAction.Kind);
            yield return new DeclarativeType<OnTeamsChannelCreated>(OnTeamsChannelCreated.Kind);
            yield return new DeclarativeType<OnTeamsChannelDeleted>(OnTeamsChannelDeleted.Kind);
            yield return new DeclarativeType<OnTeamsChannelRenamed>(OnTeamsChannelRenamed.Kind);
            yield return new DeclarativeType<OnTeamsChannelRestored>(OnTeamsChannelRestored.Kind);
            yield return new DeclarativeType<OnTeamsFileConsent>(OnTeamsFileConsent.Kind);
            yield return new DeclarativeType<OnTeamsMessagingExtensionCardButtonClicked>(OnTeamsMessagingExtensionCardButtonClicked.Kind);
            yield return new DeclarativeType<OnTeamsMessagingExtensionConfigurationQuerySettingUrl>(OnTeamsMessagingExtensionConfigurationQuerySettingUrl.Kind);
            yield return new DeclarativeType<OnTeamsMessagingExtensionConfigurationSetting>(OnTeamsMessagingExtensionConfigurationSetting.Kind);
            yield return new DeclarativeType<OnTeamsMessagingExtensionFetchTask>(OnTeamsMessagingExtensionFetchTask.Kind);
            yield return new DeclarativeType<OnTeamsMessagingExtensionQuery>(OnTeamsMessagingExtensionQuery.Kind);
            yield return new DeclarativeType<OnTeamsMessagingExtensionSelectItem>(OnTeamsMessagingExtensionSelectItem.Kind);
            yield return new DeclarativeType<OnTeamsMessagingExtensionSubmitAction>(OnTeamsMessagingExtensionSubmitAction.Kind);
            yield return new DeclarativeType<OnTeamsO365ConnectorCardAction>(OnTeamsO365ConnectorCardAction.Kind);
            yield return new DeclarativeType<OnTeamsTaskModuleFetch>(OnTeamsTaskModuleFetch.Kind);
            yield return new DeclarativeType<OnTeamsTaskModuleSubmit>(OnTeamsTaskModuleSubmit.Kind);
            yield return new DeclarativeType<OnTeamsTeamArchived>(OnTeamsTeamArchived.Kind);
            yield return new DeclarativeType<OnTeamsTeamDeleted>(OnTeamsTeamDeleted.Kind);
            yield return new DeclarativeType<OnTeamsTeamHardDeleted>(OnTeamsTeamHardDeleted.Kind);
            yield return new DeclarativeType<OnTeamsTeamRenamed>(OnTeamsTeamRenamed.Kind);
            yield return new DeclarativeType<OnTeamsTeamRestored>(OnTeamsTeamRestored.Kind);
            yield return new DeclarativeType<OnTeamsTeamUnarchived>(OnTeamsTeamUnarchived.Kind);

            // Actions
            yield return new DeclarativeType<GetMeetingParticipant>(GetMeetingParticipant.Kind);
        }

        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }
    }
}

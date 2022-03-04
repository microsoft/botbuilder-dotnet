// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    /// <summary>
    /// Defines values for O365 Cards' ContentTypes.
    /// </summary>
    public static class O365ContentTypes
    {
        /// <summary>
        /// Content type for <see cref="O365ConnectorCard"/>.
        /// </summary>
        public const string O365ConnectorCard = "application/vnd.microsoft.teams.card.o365connector";

        /// <summary>
        /// Content type for <see cref="O365ConnectorCardViewAction"/>.
        /// </summary>
        public const string O365ConnectorCardViewAction = "ViewAction";

        /// <summary>
        /// Content type for <see cref="O365ConnectorCardOpenUri"/>.
        /// </summary>
        public const string O365ConnectorCardOpenUri = "OpenUri";

        /// <summary>
        /// Content type for <see cref="O365ConnectorCardHttpPOST"/>.
        /// </summary>
        public const string O365ConnectorCardHttpPOST = "HttpPOST";

        /// <summary>
        /// Content type for <see cref="O365ConnectorCardActionCard"/>.
        /// </summary>
        public const string O365ConnectorCardActionCard = "ActionCard";

        /// <summary>
        /// Content type for <see cref="O365ConnectorCardTextInput"/>.
        /// </summary>
        public const string O365ConnectorCardTextInput = "TextInput";

        /// <summary>
        /// Content type for <see cref="O365ConnectorCardDateInput"/>.
        /// </summary>
        public const string O365ConnectorCardDateInput = "DateInput";

        /// <summary>
        /// Content type for <see cref="O365ConnectorCardMultichoiceInput"/>.
        /// </summary>
        public const string O365ConnectorCardMultichoiceInput = "MultichoiceInput";

        /// <summary>
        /// Content type for <see cref="FileConsentCard"/>.
        /// </summary>
        public const string FileConsentCard = "application/vnd.microsoft.teams.card.file.consent";

        /// <summary>
        /// Content type for <see cref="FileDownloadInfo"/>.
        /// </summary>
        public const string FileDownloadInfo = "application/vnd.microsoft.teams.file.download.info";

        /// <summary>
        /// Content type for <see cref="FileConsentCard"/>.
        /// </summary>
        public const string FileInfoCard = "application/vnd.microsoft.teams.card.file.info";
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the value of the invoke activity sent when the user acts on
    /// a file consent card.
    /// </summary>
    public partial class FileConsentCardResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileConsentCardResponse"/> class.
        /// </summary>
        public FileConsentCardResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileConsentCardResponse"/> class.
        /// </summary>
        /// <param name="action">The action the user took. Possible values
        /// include: 'accept', 'decline'.</param>
        /// <param name="context">The context associated with the
        /// action.</param>
        /// <param name="uploadInfo">If the user accepted the file, contains
        /// information about the file to be uploaded.</param>
        public FileConsentCardResponse(string action = default, object context = default, FileUploadInfo uploadInfo = default)
        {
            Action = action;
            Context = context;
            UploadInfo = uploadInfo;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the action the user took. Possible values include:
        /// 'accept', 'decline'.
        /// </summary>
        /// <value>The action the user took.</value>
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the context associated with the action.
        /// </summary>
        /// <value>The context associated with the action.</value>
        [JsonProperty(PropertyName = "context")]
        public object Context { get; set; }

        /// <summary>
        /// Gets or sets if the user accepted the file, contains information
        /// about the file to be uploaded.
        /// </summary>
        /// <value>The information about the file to be uploaded.</value>
        [JsonProperty(PropertyName = "uploadInfo")]
        public FileUploadInfo UploadInfo { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}

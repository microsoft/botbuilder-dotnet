// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// File consent card attachment.
    /// </summary>
    public partial class FileConsentCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileConsentCard"/> class.
        /// </summary>
        public FileConsentCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileConsentCard"/> class.
        /// </summary>
        /// <param name="description">File description.</param>
        /// <param name="sizeInBytes">Size of the file to be uploaded in
        /// Bytes.</param>
        /// <param name="acceptContext">Context sent back to the Bot if user
        /// consented to upload. This is free flow schema and is sent back in
        /// Value field of Activity.</param>
        /// <param name="declineContext">Context sent back to the Bot if user
        /// declined. This is free flow schema and is sent back in Value field
        /// of Activity.</param>
        public FileConsentCard(string description = default(string), long? sizeInBytes = default(long?), object acceptContext = default(object), object declineContext = default(object))
        {
            Description = description;
            SizeInBytes = sizeInBytes;
            AcceptContext = acceptContext;
            DeclineContext = declineContext;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets file description.
        /// </summary>
        /// <value>The file description.</value>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets size of the file to be uploaded in Bytes.
        /// </summary>
        /// <value>The size of the file to be uploaded in bytes.</value>
        [JsonProperty(PropertyName = "sizeInBytes")]
        public long? SizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets context sent back to the Bot if user consented to
        /// upload. This is free flow schema and is sent back in Value field of
        /// Activity.
        /// </summary>
        /// <value>The context to send back if user consented to upload.</value>
        [JsonProperty(PropertyName = "acceptContext")]
        public object AcceptContext { get; set; }

        /// <summary>
        /// Gets or sets context sent back to the Bot if user declined. This is
        /// free flow schema and is sent back in Value field of Activity.
        /// </summary>
        /// <value>The context to send back to the Bot if user declined.</value>
        [JsonProperty(PropertyName = "declineContext")]
        public object DeclineContext { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}

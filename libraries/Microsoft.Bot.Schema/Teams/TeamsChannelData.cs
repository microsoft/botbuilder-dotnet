﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Channel data specific to messages received in Microsoft Teams.
    /// </summary>
    public partial class TeamsChannelData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsChannelData"/> class.
        /// </summary>
        public TeamsChannelData()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsChannelData"/> class.
        /// </summary>
        /// <param name="channel">Information about the channel in which the message was sent.</param>
        /// <param name="eventType">Type of event.</param>
        /// <param name="team">Information about the team in which the message was sent.</param>
        /// <param name="notification">Notification settings for the message.</param>
        /// <param name="tenant">Information about the tenant in which the
        /// message was sent.</param>
        public TeamsChannelData(ChannelInfo channel = default, string eventType = default, TeamInfo team = default, NotificationInfo notification = default, TenantInfo tenant = default)
        {
            Channel = channel;
            EventType = eventType;
            Team = team;
            Notification = notification;
            Tenant = tenant;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets information about the channel in which the message was
        /// sent.
        /// </summary>
        /// <value>The channel information.</value>
        [JsonProperty(PropertyName = "channel")]
        public ChannelInfo Channel { get; set; }

        /// <summary>
        /// Gets or sets type of event.
        /// </summary>
        /// <value>The type of event.</value>
        [JsonProperty(PropertyName = "eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets information about the team in which the message was
        /// sent.
        /// </summary>
        /// <value>The information about the team.</value>
        [JsonProperty(PropertyName = "team")]
        public TeamInfo Team { get; set; }

        /// <summary>
        /// Gets or sets notification settings for the message.
        /// </summary>
        /// <value>The notification settings for the user.</value>
        [JsonProperty(PropertyName = "notification")]
        public NotificationInfo Notification { get; set; }

        /// <summary>
        /// Gets or sets information about the tenant in which the message was
        /// sent.
        /// </summary>
        /// <value>The information about the tenant.</value>
        [JsonProperty(PropertyName = "tenant")]
        public TenantInfo Tenant { get; set; }

        /// <summary>
        /// Gets or sets information about the meeting in which the message was
        /// sent.
        /// </summary>
        /// <value>The information about the meeting.</value>
        [JsonProperty(PropertyName = "meeting")]
        public TeamsMeetingInfo Meeting { get; set; }

        /// <summary>
        /// Gets or sets properties that are not otherwise defined by the <see cref="TeamsChannelData"/> type but that
        /// might appear in the REST JSON object.
        /// </summary>
        /// <value>The extended properties for the object.</value>
        /// <remarks>With this, properties not represented in the defined type are not dropped when
        /// the JSON object is deserialized, but are instead stored in this property. Such properties
        /// will be written to a JSON object when the instance is serialized.</remarks>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, object> AdditionalProperties { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}

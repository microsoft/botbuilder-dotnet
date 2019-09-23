// Copyright (c) Microsoft Corporation.All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Thrzn41.WebexTeams;
using Thrzn41.WebexTeams.Version1;

namespace Microsoft.Bot.Builder.Adapters.Webex
{
    public class WebexClientWrapper
    {
        private const string WebhookUrl = "https://api.ciscospark.com/v1/webhooks";
        private const string MessageUrl = "https://api.ciscospark.com/v1/messages";
        private const string ActionsUrl = "https://api.ciscospark.com/v1/attachment/actions";

        private readonly TeamsAPIClient _api;
        private readonly WebexAdapterOptions _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebexClientWrapper"/> class.
        /// Creates a Webex Client Wrapper. See <see cref="WebexAdapterOptions"/> for a full definition of the allowed parameters.
        /// </summary>
        /// <param name="config">An object containing API credentials, a webhook verification token and other options.</param>
        public WebexClientWrapper(WebexAdapterOptions config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(_config.AccessToken))
            {
                throw new ArgumentException(nameof(config.AccessToken));
            }

            if (_config.PublicAddress == null)
            {
                throw new ArgumentException(nameof(config.PublicAddress));
            }

            _api = TeamsAPI.CreateVersion1Client(_config.AccessToken);
        }

        /// <summary>
        /// Lists all webhook subscriptions currently associated with this application.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A list of webhook subscriptions.</returns>
        public async Task<WebhookList> ListWebhookSubscriptionsAsync(CancellationToken cancellationToken)
        {
            return await ListWebhooksAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears out and resets the list of webhook subscriptions.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ResetWebhookSubscriptionsAsync(CancellationToken cancellationToken)
        {
            var webhookList = await ListWebhooksAsync(cancellationToken).ConfigureAwait(false);

            for (var i = 0; i < webhookList.ItemCount; i++)
            {
                await DeleteWebhookAsync(webhookList.Items[i], cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Register webhook subscriptions to start receiving message events and adaptive cards events.
        /// </summary>
        /// <param name="webhookPath">The path of the webhook endpoint like '/api/messages'.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>An array of registered <see cref="Webhook"/>.</returns>
        public async Task<Webhook[]> RegisterWebhookSubscriptionsAsync(string webhookPath = "api/messages", CancellationToken cancellationToken = default)
        {
            var webHookName = string.IsNullOrWhiteSpace(_config.WebhookName) ? "Webex Firehose" : _config.WebhookName;
            var webHookCardsName = string.IsNullOrWhiteSpace(_config.WebhookName) ? "Webex AttachmentActions" : $"{_config.WebhookName}_AttachmentActions)";

            var webhookList = await ListWebhooksAsync(cancellationToken).ConfigureAwait(false);

            string webhookId = null;
            string webhookCardsId = null;

            for (var i = 0; i < webhookList.ItemCount; i++)
            {
                if (webhookList.Items[i].Name == webHookName)
                {
                    webhookId = webhookList.Items[i].Id;
                }
                else if (webhookList.Items[i].Name == webHookCardsName)
                {
                    webhookCardsId = webhookList.Items[i].Id;
                }
            }

            var webhookUrl = new Uri(_config.PublicAddress + webhookPath);

            var webhook = await RegisterWebhookSubscriptionAsync(webhookId, webHookName, webhookUrl, cancellationToken).ConfigureAwait(false);
            var cardsWebhook = await RegisterAdaptiveCardsWebhookSubscriptionAsync(webhookCardsId, webHookCardsName, webhookUrl, cancellationToken).ConfigureAwait(false);

            return new[] { webhook, cardsWebhook };
        }

        /// <summary>
        /// Register a webhook subscription with Webex Teams to start receiving message events.
        /// </summary>
        /// <param name="hookId">The id of the webhook to be registered.</param>
        /// <param name="webHookName">The name of the webhook to be registered.</param>
        /// <param name="webhookUrl">The Uri of the webhook.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The registered <see cref="Webhook"/>.</returns>
        public async Task<Webhook> RegisterWebhookSubscriptionAsync(string hookId, string webHookName, Uri webhookUrl, CancellationToken cancellationToken)
        {
            Webhook webhook;

            if (hookId != null)
            {
                webhook = await UpdateWebhookAsync(hookId, webHookName, webhookUrl, _config.Secret, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                webhook = await CreateWebhookAsync(webHookName, webhookUrl, EventResource.All, EventType.All, null, _config.Secret, cancellationToken).ConfigureAwait(false);
            }

            return webhook;
        }

        /// <summary>
        /// Register a webhook subscription with Webex Teams to start receiving events related to adaptive cards.
        /// </summary>
        /// <param name="hookId">The id of the webhook to be registered.</param>
        /// <param name="webHookName">The name of the webhook to be registered.</param>
        /// <param name="webhookUrl">The Uri of the webhook.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Webhook> RegisterAdaptiveCardsWebhookSubscriptionAsync(string hookId, string webHookName, Uri webhookUrl, CancellationToken cancellationToken)
        {
            Webhook webhook;

            if (hookId != null)
            {
                webhook = await UpdateAdaptiveCardsWebhookAsync(hookId, webHookName, webhookUrl, _config.Secret, _config.AccessToken, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                webhook = await CreateAdaptiveCardsWebhookAsync(webHookName, webhookUrl, EventType.All, _config.Secret, _config.AccessToken, cancellationToken).ConfigureAwait(false);
            }

            return webhook;
        }

        /// <summary>
        /// Validates the local secret against the one obtained from the request header.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> with the signature.</param>
        /// <param name="json">The serialized payload to be use for comparison.</param>
        /// <returns>The result of the comparison between the signature in the request and hashed json.</returns>
        public virtual bool ValidateSignature(HttpRequest request, string json)
        {
            var signature = request.Headers.ContainsKey("x-spark-signature")
                ? request.Headers["x-spark-signature"].ToString().ToUpperInvariant()
                : throw new Exception("HttpRequest is missing \"x-spark-signature\"");

#pragma warning disable CA5350 // Webex API uses SHA1 as cryptographic algorithm.
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_config.Secret)))
            {
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
                var hash = BitConverter.ToString(hashArray).Replace("-", string.Empty).ToUpperInvariant();

                return signature == hash;
            }
#pragma warning restore CA5350 // Webex API uses SHA1 as cryptographic algorithm.
        }

        /// <summary>
        /// Wraps Webex API's CreateDirectMessageAsync method.
        /// </summary>
        /// <param name="toPersonOrEmail">Id or email of message recipient.</param>
        /// <param name="text">Text of the message.</param>
        /// <param name="files">List of files attached to the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created message id.</returns>
        public virtual async Task<string> CreateMessageAsync(string toPersonOrEmail, string text, IList<Uri> files = null, CancellationToken cancellationToken = default)
        {
            var webexResponse = await _api.CreateDirectMessageAsync(toPersonOrEmail, text, files, cancellationToken: cancellationToken).ConfigureAwait(false);

            return webexResponse.Data.Id;
        }

        /// <summary>
        /// Wraps Webex API's DeleteMessageAsync method.
        /// </summary>
        /// <param name="messageId">The id of the message to be deleted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeleteMessageAsync(string messageId, CancellationToken cancellationToken)
        {
            await _api.DeleteMessageAsync(messageId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a message with attachments.
        /// </summary>
        /// <param name="toPersonOrEmail">Id or email of message recipient.</param>
        /// <param name="text">Text of the message.</param>
        /// <param name="attachments">List of attachments attached to the message.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created message id.</returns>
        public virtual async Task<string> CreateMessageWithAttachmentsAsync(string toPersonOrEmail, string text, IList<Attachment> attachments, CancellationToken cancellationToken)
        {
            Message result;
            var url = MessageUrl;

            var attachmentsContent = new List<object>();

            foreach (var attach in attachments)
            {
                attachmentsContent.Add(attach.Content);
            }

            var request = new WebexMessageRequest
            {
                ToPersonId = toPersonOrEmail,
                Text = text ?? string.Empty,
                Attachments = attachmentsContent.Count > 0 ? attachmentsContent : null,
            };

            var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            http.PreAuthenticate = true;
            http.Headers.Add("Authorization", "Bearer " + _config.AccessToken);
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";

            var parsedContent = JsonConvert.SerializeObject(request);
            var encoding = new ASCIIEncoding();
            var bytes = encoding.GetBytes(parsedContent);

            var newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = await http.GetResponseAsync().ConfigureAwait(false);

            var stream = response.GetResponseStream();

            using (var sr = new StreamReader(stream))
            {
                var content = sr.ReadToEnd();
                result = JsonConvert.DeserializeObject<Message>(content);
            }

            return result.Id;
        }

        /// <summary>
        /// Shows details for a attachment action, by ID.
        /// </summary>
        /// <param name="actionId">A unique identifier for the attachment action.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The attachment action details.</returns>
        public virtual async Task<Message> GetAttachmentActionAsync(string actionId, CancellationToken cancellationToken)
        {
            Message result;

            var url = $"{ActionsUrl}/{actionId}";

            var http = (HttpWebRequest)WebRequest.Create(new Uri(url));
            http.PreAuthenticate = true;
            http.Headers.Add("Authorization", "Bearer " + _config.AccessToken);
            http.Method = "GET";

            var response = await http.GetResponseAsync().ConfigureAwait(false);

            var stream = response.GetResponseStream();

            using (var sr = new StreamReader(stream))
            {
                var content = sr.ReadToEnd();
                result = JsonConvert.DeserializeObject<Message>(content);
            }

            return result;
        }

        /// <summary>
        /// Wraps Webex API's GetMeAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The <see cref="Person"/> object associated with the bot.</returns>
        public virtual async Task<Person> GetMeAsync(CancellationToken cancellationToken)
        {
            var resultPerson = await _api.GetMeAsync(cancellationToken).ConfigureAwait(false);
            return resultPerson.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetMeFromCacheAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The <see cref="Person"/> object associated with the bot, from cache.</returns>
        public virtual async Task<CachedPerson> GetMeFromCacheAsync(CancellationToken cancellationToken)
        {
            var resultPerson = await _api.GetMeFromCacheAsync(cancellationToken).ConfigureAwait(false);

            return resultPerson.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetMessageAsync method.
        /// </summary>
        /// <param name="messageId">Id of the message to be recovered.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The message's data.</returns>
        public virtual async Task<Message> GetMessageAsync(string messageId, CancellationToken cancellationToken)
        {
            var message = await _api.GetMessageAsync(messageId, cancellationToken).ConfigureAwait(false);

            return message.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ActivateWebhookAsync method.
        /// </summary>
        /// <param name="webhook"><see cref="Webhook"/> to be activated.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The Activated <see cref="Webhook"/>.</returns>
        public virtual async Task<Webhook> ActivateWebhookAsync(Webhook webhook, CancellationToken cancellationToken)
        {
            var resultWebhook = await _api.ActivateWebhookAsync(webhook, cancellationToken).ConfigureAwait(false);

            return resultWebhook.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListWebhooksAsync method.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A list of Webhooks associated with the application.</returns>
        public virtual async Task<WebhookList> ListWebhooksAsync(CancellationToken cancellationToken)
        {
            var webhookList = await _api.ListWebhooksAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return webhookList.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's CreateWebhookAsync method.
        /// </summary>
        /// <param name="name">Name for the webhook.</param>
        /// <param name="targetUri">Uri of the webhook.</param>
        /// <param name="resource">Event resource associated with the webhook.</param>
        /// <param name="type">Event type associated with the webhook.</param>
        /// <param name="filters">Filters for the webhook.</param>
        /// <param name="secret">Secret used to validate the webhook.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created <see cref="Webhook"/>.</returns>
        public virtual async Task<Webhook> CreateWebhookAsync(string name, Uri targetUri, EventResource resource, EventType type, IEnumerable<EventFilter> filters, string secret, CancellationToken cancellationToken)
        {
            var resultWebhook = await _api.CreateWebhookAsync(name, targetUri, resource, type, null, secret, cancellationToken: cancellationToken).ConfigureAwait(false);

            return resultWebhook.GetData(false);
        }

        /// <summary>
        /// Creates a Webhook subscription to handle Adaptive cards messages.
        /// </summary>
        /// <param name="name">Name for the webhook.</param>
        /// <param name="targetUri">Uri of the webhook.</param>
        /// <param name="type">Event type associated with the webhook.</param>
        /// <param name="secret">Secret used to validate the webhook.</param>
        /// <param name="token">Access Token for authorization.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created <see cref="Webhook"/>.</returns>
        public virtual async Task<Webhook> CreateAdaptiveCardsWebhookAsync(string name, Uri targetUri, EventType type, string secret, string token, CancellationToken cancellationToken)
        {
            var url = WebhookUrl;

            var data = new NameValueCollection
            {
                ["name"] = name,
                ["targetUrl"] = targetUri.AbsoluteUri,
                ["resource"] = "attachmentActions",
                ["event"] = "all",
                ["secret"] = secret,
            };

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;

                var response = await client.UploadValuesTaskAsync(new Uri(url), "POST", data).ConfigureAwait(false);

                var result = JsonConvert.DeserializeObject<Webhook>(Encoding.ASCII.GetString(response));

                return result;
            }
        }

        /// <summary>
        /// Updates a Webhook subscription to handle Adaptive cards messages.
        /// </summary>
        /// <param name="webhookId">Id of the webhook to be updated.</param>
        /// <param name="name">Name for the webhook.</param>
        /// <param name="targetUri">Uri of the webhook.</param>
        /// <param name="secret">Secret used to validate the webhook.</param>
        /// <param name="token">Access Token for authorization.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created <see cref="Webhook"/>.</returns>
        public virtual async Task<Webhook> UpdateAdaptiveCardsWebhookAsync(string webhookId, string name, Uri targetUri, string secret, string token, CancellationToken cancellationToken)
        {
            Webhook result;

            var url = $"{WebhookUrl}/{webhookId}";
            var data = new NameValueCollection
            {
                ["name"] = name,
                ["targetUrl"] = targetUri.AbsoluteUri,
                ["resource"] = "attachmentActions",
                ["event"] = "all",
                ["secret"] = secret,
            };

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;

                var response = await client.UploadValuesTaskAsync(new Uri(url), "PUT", data).ConfigureAwait(false);

                result = JsonConvert.DeserializeObject<Webhook>(Encoding.ASCII.GetString(response));

                return result;
            }
        }

        /// <summary>
        /// Wraps Webex API's GetWebhookAsync method.
        /// </summary>
        /// <param name="webhookId">The id of the Webhook to get.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The requested Webhook.</returns>
        public virtual async Task<Webhook> GetWebhookAsync(string webhookId, CancellationToken cancellationToken)
        {
            var resultWebhook = await _api.GetWebhookAsync(webhookId, cancellationToken).ConfigureAwait(false);

            return resultWebhook.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's DeleteWebhookAsync method.
        /// </summary>
        /// <param name="id">Id of the webhook to be deleted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeleteWebhookAsync(Webhook id, CancellationToken cancellationToken)
        {
            await _api.DeleteWebhookAsync(id, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Webex API's UpdateWebhookAsync method.
        /// </summary>
        /// <param name="webhookId">Id of the webhook to be updated.</param>
        /// <param name="name">Name for the webhook.</param>
        /// <param name="targetUri">Uri of the webhook.</param>
        /// <param name="secret">Secret used to validate the webhook.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The updated <see cref="Webhook"/>.</returns>
        public virtual async Task<Webhook> UpdateWebhookAsync(string webhookId, string name, Uri targetUri, string secret, CancellationToken cancellationToken)
        {
            var resultWebhook = await _api.UpdateWebhookAsync(webhookId, name, targetUri, secret, cancellationToken: cancellationToken).ConfigureAwait(false);

            return resultWebhook.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's CreateSpaceAsync method.
        /// </summary>
        /// <param name="title">Space title.</param>
        /// <param name="teamId">The ID for the team with which this room is associated.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The space created.</returns>
        public virtual async Task<Space> CreateSpaceAsync(string title, string teamId = null, CancellationToken cancellationToken = default)
        {
            var resultSpace = await _api.CreateSpaceAsync(title, teamId, cancellationToken).ConfigureAwait(false);

            return resultSpace.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's CreateSpaceMembershipAsync method.
        /// </summary>
        /// <param name="spaceId">The space ID.</param>
        /// <param name="personIdOrEmail">The person ID or Email.</param>
        /// <param name="isModerator">True for moderator persons.</param>
        /// <param name="personIdType"><see cref="PersonIdType"/> for personIdOrEmail parameter.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The resulting space membership.</returns>
        public virtual async Task<SpaceMembership> CreateSpaceMembershipAsync(string spaceId, string personIdOrEmail, bool? isModerator = null, PersonIdType personIdType = PersonIdType.Detect, CancellationToken cancellationToken = default)
        {
            var resultSpaceMembership = await _api.CreateSpaceMembershipAsync(spaceId, personIdOrEmail, isModerator, personIdType, cancellationToken).ConfigureAwait(false);

            return resultSpaceMembership.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's DeleteSpaceAsync method.
        /// </summary>
        /// <param name="spaceId">The id of the space to be deleted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeleteSpaceAsync(string spaceId, CancellationToken cancellationToken)
        {
            await _api.DeleteSpaceAsync(spaceId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Webex API's DeleteSpaceMembershipAsync method.
        /// </summary>
        /// <param name="membershipId">The id of the membership to be deleted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeleteSpaceMembershipAsync(string membershipId, CancellationToken cancellationToken)
        {
            await _api.DeleteSpaceMembershipAsync(membershipId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Webex API's GetSpaceAsync method.
        /// </summary>
        /// <param name="spaceId">The id of the space to be gotten.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The space requested.</returns>
        public virtual async Task<Space> GetSpaceAsync(string spaceId, CancellationToken cancellationToken)
        {
            var resultSpace = await _api.GetSpaceAsync(spaceId, cancellationToken).ConfigureAwait(false);
            return resultSpace.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetSpaceMembershipAsync method.
        /// </summary>
        /// <param name="membershipId">The id of the membership to get.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The requested space membership.</returns>
        public virtual async Task<SpaceMembership> GetSpaceMembershipAsync(string membershipId, CancellationToken cancellationToken)
        {
            var resultSpaceMembership = await _api.GetSpaceMembershipAsync(membershipId, cancellationToken).ConfigureAwait(false);

            return resultSpaceMembership.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListSpacesAsync method.
        /// </summary>
        /// <param name="teamId">Limit the rooms to those associated with a team, by ID.</param>
        /// <param name="type"><see cref="SpaceType.Direct"/> returns all 1-to-1 rooms. <see cref="SpaceType.Group"/> returns all group rooms.If not specified or values are not matched, will return all room types.</param>
        /// <param name="sortBy">Sort results by space ID(<see cref="SpaceSortBy.Id"/>), most recent activity(<see cref="SpaceSortBy.LastActivity"/>), or most recently created(<see cref="SpaceSortBy.Created"/>).</param>
        /// <param name="max">Limit the maximum number of messages in the response.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A list of the spaces.</returns>
        public virtual async Task<SpaceList> ListSpacesAsync(string teamId = null, SpaceType type = null, SpaceSortBy sortBy = null, int? max = null, CancellationToken cancellationToken = default)
        {
            var resultSpaceList = await _api.ListSpacesAsync(teamId, type, sortBy, max, cancellationToken).ConfigureAwait(false);

            return resultSpaceList.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListSpaceMembershipsAsync method.
        /// </summary>
        /// <param name="spaceId">Limit results to a specific space, by ID.</param>
        /// <param name="personIdOrEmail">Limit results to a specific person, by ID or Email.</param>
        /// <param name="max">Limit the maximum number of items in the response.</param>
        /// <param name="personIdType"><see cref="PersonIdType"/> for personIdOrEmail parameter.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The space memberships list.</returns>
        public virtual async Task<SpaceMembershipList> ListSpaceMembershipsAsync(string spaceId = null, string personIdOrEmail = null, int? max = null, PersonIdType personIdType = PersonIdType.Detect, CancellationToken cancellationToken = default)
        {
            var resultSpaceMembershipsList = await _api.ListSpaceMembershipsAsync(spaceId, personIdOrEmail, max, personIdType, cancellationToken).ConfigureAwait(false);

            return resultSpaceMembershipsList.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's UpdateSpaceAsync method.
        /// </summary>
        /// <param name="spaceId">Space id to be updated.</param>
        /// <param name="title">A user-friendly name for the space.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The updated space.</returns>
        public virtual async Task<Space> UpdateSpaceAsync(string spaceId, string title, CancellationToken cancellationToken)
        {
            var resultSpace = await _api.UpdateSpaceAsync(spaceId, title, cancellationToken).ConfigureAwait(false);

            return resultSpace.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's UpdateSpaceMembershipAsync method.
        /// </summary>
        /// <param name="membershipId">Membership id to be updated.</param>
        /// <param name="isModerator">Set to true to make the person a space moderator.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The updated space membership.</returns>
        public virtual async Task<SpaceMembership> UpdateSpaceMembershipAsync(string membershipId, bool isModerator, CancellationToken cancellationToken)
        {
            var resultSpaceMembership = await _api.UpdateSpaceMembershipAsync(membershipId, isModerator, cancellationToken).ConfigureAwait(false);

            return resultSpaceMembership.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's CreateTeamAsync method.
        /// </summary>
        /// <param name="name">A user-friendly name for the team.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The created team.</returns>
        public virtual async Task<Team> CreateTeamAsync(string name, CancellationToken cancellationToken)
        {
            var resultTeam = await _api.CreateTeamAsync(name, cancellationToken).ConfigureAwait(false);

            return resultTeam.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's CreateTeamMembershipAsync method.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <param name="personIdOrEmail">The person ID or Email.</param>
        /// <param name="isModerator">Set to true to make the person a room moderator.</param>
        /// <param name="personIdType"><see cref="PersonIdType"/> for personIdOrEmail parameter.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The team membership created.</returns>
        public virtual async Task<TeamMembership> CreateTeamMembershipAsync(string teamId, string personIdOrEmail, bool? isModerator = null, PersonIdType personIdType = PersonIdType.Detect, CancellationToken cancellationToken = default)
        {
            var resultTeamMembership = await _api.CreateTeamMembershipAsync(teamId, personIdOrEmail, isModerator, personIdType, cancellationToken).ConfigureAwait(false);

            return resultTeamMembership.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's DeleteTeamAsync method.
        /// </summary>
        /// <param name="teamId">Team id to be deleted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeleteTeamAsync(string teamId, CancellationToken cancellationToken)
        {
            await _api.DeleteTeamAsync(teamId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Webex API's DeleteTeamMembershipAsync method.
        /// </summary>
        /// <param name="membershipId">Team Membership id to be deleted.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task DeleteTeamMembershipAsync(string membershipId, CancellationToken cancellationToken)
        {
            await _api.DeleteTeamMembershipAsync(membershipId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps Webex API's GetTeamAsync method.
        /// </summary>
        /// <param name="teamId">Team id that the detail info is gotten.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The requested team.</returns>
        public virtual async Task<Team> GetTeamAsync(string teamId, CancellationToken cancellationToken)
        {
            var resultTeam = await _api.GetTeamAsync(teamId, cancellationToken).ConfigureAwait(false);

            return resultTeam.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetTeamMembershipAsync method.
        /// </summary>
        /// <param name="membershipId">Team Membership id that the detail info is gotten.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The requested team membership.</returns>
        public virtual async Task<TeamMembership> GetTeamMembershipAsync(string membershipId, CancellationToken cancellationToken)
        {
            var resultTeamMembership = await _api.GetTeamMembershipAsync(membershipId, cancellationToken).ConfigureAwait(false);

            return resultTeamMembership.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListTeamMembershipsAsync method.
        /// </summary>
        /// <param name="teamId">List team memberships for a team, by ID.</param>
        /// <param name="max">Limit the maximum number of items in the response.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A list of team memberships.</returns>
        public virtual async Task<TeamMembershipList> ListTeamMembershipsAsync(string teamId, int? max = null, CancellationToken cancellationToken = default)
        {
            var resultTeamMembershipList = await _api.ListTeamMembershipsAsync(teamId, max, cancellationToken).ConfigureAwait(false);

            return resultTeamMembershipList.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListTeamsAsync method.
        /// </summary>
        /// <param name="max">Limit the maximum number of teams in the response.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A list of the teams.</returns>
        public virtual async Task<TeamList> ListTeamsAsync(int? max = null, CancellationToken cancellationToken = default)
        {
            var resultTeamsList = await _api.ListTeamsAsync(max, cancellationToken).ConfigureAwait(false);

            return resultTeamsList.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's UpdateTeamAsync method.
        /// </summary>
        /// <param name="teamId">Team id to be updated.</param>
        /// <param name="name">A user-friendly name for the team.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The updated team.</returns>
        public virtual async Task<Team> UpdateTeamAsync(string teamId, string name, CancellationToken cancellationToken)
        {
            var resultTeam = await _api.UpdateTeamAsync(teamId, name, cancellationToken).ConfigureAwait(false);

            return resultTeam.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's UpdateTeamMembershipAsync method.
        /// </summary>
        /// <param name="membershipId">Membership id to be updated.</param>
        /// <param name="isModerator">Set to true to make the person a team moderator.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The updated team membership.</returns>
        public virtual async Task<TeamMembership> UpdateTeamMembershipAsync(string membershipId, bool isModerator, CancellationToken cancellationToken)
        {
            var resultTeamMembership = await _api.UpdateTeamMembershipAsync(membershipId, isModerator, cancellationToken).ConfigureAwait(false);

            return resultTeamMembership.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetFileDataAsync method.
        /// </summary>
        /// <param name="fileUri">Uri of the file.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The teams file data.</returns>
        public virtual async Task<TeamsFileData> GetFileDataAsync(Uri fileUri, CancellationToken cancellationToken)
        {
            var resultTeamsFileData = await _api.GetFileDataAsync(fileUri, cancellationToken).ConfigureAwait(false);

            return resultTeamsFileData.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's GetFileInfoAsync method.
        /// </summary>
        /// <param name="fileUri">Uri of the file.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>The teams file info.</returns>
        public virtual async Task<TeamsFileInfo> GetFileInfoAsync(Uri fileUri, CancellationToken cancellationToken)
        {
            var resultTeamsFileInfo = await _api.GetFileInfoAsync(fileUri, cancellationToken).ConfigureAwait(false);

            return resultTeamsFileInfo.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListMessagesAsync method.
        /// </summary>
        /// <param name="spaceId">List messages for a space, by ID.</param>
        /// <param name="mentionedPeople">List messages where the caller is mentioned by specifying "me" or the caller personId.</param>
        /// <param name="before">List messages sent before a date and time.</param>
        /// <param name="beforeMessage">List messages sent before a message, by ID.</param>
        /// <param name="max">Limit the maximum number of messages in the response.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A list of the messages.</returns>
        public virtual async Task<MessageList> ListMessagesAsync(string spaceId, string mentionedPeople = null, DateTime? before = null, string beforeMessage = null, int? max = null, CancellationToken cancellationToken = default)
        {
            var resultMessageList = await _api.ListMessagesAsync(spaceId, mentionedPeople, before, beforeMessage, max, cancellationToken).ConfigureAwait(false);

            return resultMessageList.GetData(false);
        }

        /// <summary>
        /// Wraps Webex API's ListPeopleAsync method.
        /// </summary>
        /// <param name="email">List people with this email address. For non-admin requests, either this or displayName are required.</param>
        /// <param name="displayName">List people whose name starts with this string. For non-admin requests, either this or email are required.</param>
        /// <param name="ids">List people by ID. Accepts up to 85 person IDs.</param>
        /// <param name="max">Limit the maximum number of people in the response.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A list with the requested people.</returns>
        public virtual async Task<PersonList> ListPeopleAsync(string email = null, string displayName = null, IEnumerable<string> ids = null, int? max = null, CancellationToken cancellationToken = default)
        {
            var resultPersonList = await _api.ListPeopleAsync(email, displayName, ids, max, cancellationToken).ConfigureAwait(false);

            return resultPersonList.GetData(false);
        }
    }
}

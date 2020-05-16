using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public abstract class UserSharedDataController : ChannelServiceController
    {
        private const string ConversationIdKeyName = "ConversationId";
        private const string PropertyNameKeyName = "PropertyName";
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly BotAdapter _botAdapter;
        private readonly UserState _userState;

        public UserSharedDataController(ChannelServiceHandler handler, BotAdapter botAdapter, UserState userState, SkillConversationIdFactoryBase conversationIdFactory)
        : base(handler)
        {
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _botAdapter = botAdapter ?? throw new ArgumentNullException(nameof(botAdapter));
            _conversationIdFactory = conversationIdFactory ?? throw new ArgumentNullException(nameof(conversationIdFactory));
        }

        /// <summary>
        /// StateAsync.
        /// </summary>
        /// <returns>Queries User State and return.</returns>
        [HttpPost]
        [HttpGet]
        public virtual async Task StateAsync()
        {
            Response.ContentType = "application/json";

            if (Request.Method == HttpMethods.Get)
            {
                var response = string.Empty;
                if (!Request.Query.ContainsKey(ConversationIdKeyName) || string.IsNullOrWhiteSpace(Request.Query[ConversationIdKeyName].ToString()))
                {
                    response = "{ 'error': 'Needs ConversationId' }";
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                if (!Request.Query.ContainsKey(PropertyNameKeyName) || string.IsNullOrWhiteSpace(Request.Query[PropertyNameKeyName].ToString()))
                {
                    response = "{ 'error': 'Needs PropertyName' }";
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                if (Response.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(false, false), 1024, true))
                        {
                            using (var jsonWriter = new JsonTextWriter(writer))
                            {
                                HttpHelper.BotMessageSerializer.Serialize(jsonWriter, response);
                            }
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(Response.Body).ConfigureAwait(false);
                    }

                    return;
                }

                var conversationId = Request.Query[ConversationIdKeyName].ToString();
                var propertyName = Request.Query[PropertyNameKeyName].ToString();

                var conversationReference = await _conversationIdFactory.GetSkillConversationReferenceAsync(conversationId, new System.Threading.CancellationToken()).ConfigureAwait(false);

                if (conversationReference != null)
                {
                    var activity = (Activity)Activity.CreateMessageActivity();
                    activity.ApplyConversationReference(conversationReference.ConversationReference);

                    using (var context = new TurnContext(_botAdapter, activity))
                    {
                        var stateProperty = _userState.CreateProperty<IDictionary<string, object>>(propertyName);
                        var statePropertyData = await stateProperty.GetAsync(context, () => new Dictionary<string, object>()).ConfigureAwait(false);

                        var statePropertyPayload = new StateRestPayload { Data = statePropertyData };

                        using (var memoryStream = new MemoryStream())
                        {
                            using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(false, false), 1024, true))
                            {
                                using (var jsonWriter = new JsonTextWriter(writer))
                                {
                                    HttpHelper.BotMessageSerializer.Serialize(jsonWriter, statePropertyPayload);
                                }
                            }

                            memoryStream.Seek(0, SeekOrigin.Begin);
                            await memoryStream.CopyToAsync(Response.Body).ConfigureAwait(false);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    var payload = await HttpHelper.ReadRequestAsync<StateRestPayload>(Request).ConfigureAwait(false);

                    if (payload != null)
                    {
                        var conversationReference = await _conversationIdFactory.GetSkillConversationReferenceAsync(payload.ConversationId, new System.Threading.CancellationToken()).ConfigureAwait(false);
                        var propertyName = payload.PropertyName;
                        var activity = (Activity)Activity.CreateMessageActivity();
                        activity.ApplyConversationReference(conversationReference.ConversationReference);

                        using (var context = new TurnContext(_botAdapter, activity))
                        {
                            var stateProperty = _userState.CreateProperty<IDictionary<string, object>>(propertyName);
                            await stateProperty.SetAsync(context, payload.Data).ConfigureAwait(false);
                            await _userState.SaveChangesAsync(context, true).ConfigureAwait(false);
                            Response.StatusCode = (int)HttpStatusCode.OK;
                        }
                    }
                }
                catch
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Alexa.Directives;
using Microsoft.Bot.Builder.Alexa.Integration;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Alexa
{
    public class AlexaAdapter : BotAdapter
    {
        private Dictionary<string, List<Activity>> Responses { get; set; }
        private AlexaOptions Options { get; set; }

        public async Task<AlexaResponseBody> ProcessActivity(AlexaRequestBody alexaRequest, AlexaOptions alexaOptions, Func<ITurnContext, Task> callback)
        {
            Options = alexaOptions;

            var activity = RequestToActivity(alexaRequest);
            BotAssert.ActivityNotNull(activity);

            var context = new TurnContext(this, activity);

            if (alexaRequest.Session.Attributes != null && alexaRequest.Session.Attributes.Any())
            {
                context.Services.Add("AlexaSessionAttributes", alexaRequest.Session.Attributes);
            }
            else
            {
                context.Services.Add("AlexaSessionAttributes", new Dictionary<string, string>());
            }

            context.Services.Add("AlexaResponseDirectives", new List<IAlexaDirective>());

            Responses = new Dictionary<string, List<Activity>>();

            await base.RunPipeline(context, callback).ConfigureAwait(false);

            var key = $"{activity.Conversation.Id}:{activity.Id}";

            try
            {
                var activities = Responses.ContainsKey(key) ? Responses[key] : new List<Activity>();
                var response = CreateResponseFromLastActivity(activities, context);
                response.SessionAttributes = context.AlexaSessionAttributes();
                return response;
            }
            finally
            {
                if (Responses.ContainsKey(key))
                {
                    Responses.Remove(key);
                }
            }
        }

        public override Task<ResourceResponse[]> SendActivities(ITurnContext context, Activity[] activities)
        {
            var resourceResponses = new List<ResourceResponse>();

            foreach (var activity in activities)
            {
                switch (activity.Type)
                {
                    case ActivityTypes.Message:
                    case ActivityTypes.EndOfConversation:
                        var conversation = activity.Conversation ?? new ConversationAccount();
                        var key = $"{conversation.Id}:{activity.ReplyToId}";

                        if (Responses.ContainsKey(key))
                        {
                            Responses[key].Add(activity);
                        }
                        else
                        {
                            Responses[key] = new List<Activity> { activity };
                        }

                        break;
                    default:
                        Trace.WriteLine(
                            $"AlexaAdapter.SendActivities(): Activities of type '{activity.Type}' aren't supported.");
                        break;
                }

                resourceResponses.Add(new ResourceResponse(activity.Id));
            }

            return Task.FromResult(resourceResponses.ToArray());
        }

        private static Activity RequestToActivity(AlexaRequestBody skillRequest)
        {
            var system = skillRequest.Context.System;

            var activity = new Activity
            {
                ChannelId = "alexa",
                ServiceUrl = $"{system.ApiEndpoint}?token ={system.ApiAccessToken}",
                Recipient = new ChannelAccount(system.Application.ApplicationId, "skill"),
                From = new ChannelAccount(system.User.UserId, "user"),

                Conversation = new ConversationAccount(false, "conversation",
                    $"{system.Application.ApplicationId}:{system.User.UserId}"),

                Type = skillRequest.Request.Type,
                Id = skillRequest.Request.RequestId,
                Timestamp = DateTime.ParseExact(skillRequest.Request.Timestamp, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                Locale = skillRequest.Request.Locale
            };

            switch (activity.Type)
            {
                case AlexaRequestTypes.IntentRequest:
                    activity.Value = (skillRequest.Request as AlexaIntentRequest)?.Intent;
                    activity.Code = (skillRequest.Request as AlexaIntentRequest)?.DialogState.ToString();
                    break;
                case AlexaRequestTypes.SessionEndedRequest:
                    activity.Code = (skillRequest.Request as AlexaSessionEndRequest)?.Reason;
                    activity.Value = (skillRequest.Request as AlexaSessionEndRequest)?.Error;
                    break;
            }

            activity.ChannelData = skillRequest;

            return activity;
        }

        private AlexaResponseBody CreateResponseFromLastActivity(IEnumerable<Activity> activities, ITurnContext context)
        {
            var response = new AlexaResponseBody()
            {
                Version = "1.0",
                Response = new AlexaResponse()
                {
                    ShouldEndSession = Options.ShouldEndSessionByDefault
                }
            };

            var activity = activities.First();

            if (activity.Type == ActivityTypes.EndOfConversation)
            {
                response.Response.ShouldEndSession = true;
            }

            if (!string.IsNullOrEmpty(activity.Speak))
            {
                response.Response.OutputSpeech = new AlexaOutputSpeech()
                {
                    Type = AlexaOutputSpeechType.SSML,
                    Ssml = activity.Speak.Contains("<speak>")
                        ? activity.Speak
                        : $"<speak>{activity.Speak}</speak>"
                };

                if (!string.IsNullOrEmpty(activity.Text))
                {
                    response.Response.OutputSpeech.Text = $"{activity.Text} ";
                }
            }
            else if (!string.IsNullOrEmpty(activity.Text))
            {
                if (response.Response.OutputSpeech == null)
                {
                    response.Response.OutputSpeech = new AlexaOutputSpeech()
                    {
                        Type = AlexaOutputSpeechType.PlainText,
                        Text = activity.Text
                    };
                }
            }

            AddDirectivesToResponse(context, response);

            if (Options.ConvertFirstActivityAttachmentToAlexaCard)
            {
                CreateAlexaCardFromAttachment(activity, response);
            }

            switch (activity.InputHint)
            {
                case InputHints.IgnoringInput:
                    response.Response.ShouldEndSession = true;
                    break;
                case InputHints.AcceptingInput:
                case InputHints.ExpectingInput:
                    response.Response.ShouldEndSession = false;
                    break;
            }

            return response;
        }

        private static void AddDirectivesToResponse(ITurnContext context, AlexaResponseBody response)
        {
            response.Response.Directives = context.AlexaResponseDirectives().Select(a => a).ToArray();
        }

        private static void CreateAlexaCardFromAttachment(Activity activity, AlexaResponseBody response)
        {
            var attachment = activity.Attachments != null && activity.Attachments.Any()
                ? activity.Attachments[0]
                : null;

            if (attachment != null)
            {
                switch (attachment.ContentType)
                {
                    case HeroCard.ContentType:
                    case ThumbnailCard.ContentType:
                        if (attachment.Content is HeroCard)
                        {
                            response.Response.Card = CreateAlexaCardFromHeroCard(attachment);
                        }

                        break;
                    case SigninCard.ContentType:
                        response.Response.Card = new AlexaCard()
                        {
                            Type = AlexaCardType.LinkAccount
                        };
                        break;
                }
            }
        }

        private static AlexaCard CreateAlexaCardFromHeroCard(Attachment attachment)
        {
            if (!(attachment.Content is HeroCard heroCardContent))
                return null;

            AlexaCard alexaCard = null;

            if (heroCardContent.Images != null && heroCardContent.Images.Any())
            {
                alexaCard = new AlexaCard()
                {
                    Type = AlexaCardType.Standard,
                    Image = new AlexaCardImage()
                    {
                        SmallImageUrl = heroCardContent.Images[0].Url,
                        LargeImageUrl = heroCardContent.Images.Count > 1 ? heroCardContent.Images[1].Url : null
                    }
                };

                if (heroCardContent.Title != null)
                {
                    alexaCard.Title = heroCardContent.Title;
                }

                if (heroCardContent.Text != null)
                {
                    alexaCard.Content = heroCardContent.Text;
                }
            }
            else
            {
                alexaCard = new AlexaCard()
                {
                    Type = AlexaCardType.Simple
                };
                if (heroCardContent.Title != null)
                {
                    alexaCard.Title = heroCardContent.Title;
                }

                if (heroCardContent.Text != null)
                {
                    alexaCard.Content = heroCardContent.Text;
                }
            }

            return alexaCard;
        }

        public override Task<ResourceResponse> UpdateActivity(ITurnContext context, Activity activity)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivity(ITurnContext context, ConversationReference reference)
        {
            throw new NotImplementedException();
        }
    }
}

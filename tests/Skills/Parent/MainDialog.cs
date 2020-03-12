using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples
{
    public class MainDialog : ComponentDialog
    {
        private readonly SkillsHelper _skillsHelper;

        public MainDialog(IConfiguration configuration, SkillsHelper skillsHelper)
            : base(nameof(MainDialog))
        {
            var connectionName = configuration.GetSection("ConnectionName")?.Value ?? throw new ArgumentNullException("Connection name is needed in configuration");
            _skillsHelper = skillsHelper;

            var steps = new WaterfallStep[]
                {
                    SignInStepAsync,
                    ShowTokenResponseAsync
                };
            AddDialog(new WaterfallDialog(nameof(MainDialog), steps));
            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings()
                {
                    ConnectionName = connectionName,
                    Text = "Sign In to AAD",
                    Title = "Sign In"
                }));
        }

        private async Task<DialogTurnResult> SignInStepAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            if (context.Context.Activity.Text == "login")
            {
                return await context.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // it's not meant for oauthprompt of parent, so proxy to skill
                var turnContext = context.Context;
                var cloneActivity = MessageFactory.Text(turnContext.Activity.Text);
                cloneActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
                cloneActivity.DeliveryMode = DeliveryModes.ExpectReplies;
                var response1 = await _skillsHelper.PostActivityAsync(cloneActivity, cancellationToken) as InvokeResponse<ExpectedReplies>;

                if (response1 != null && response1.Status == (int)HttpStatusCode.OK && response1.Body?.Activities != null)
                {
                    var activities = response1.Body.Activities.ToArray();
                    if (!(await _skillsHelper.InterceptOAuthCards(turnContext, activities, cancellationToken)))
                    {
                        await turnContext.SendActivitiesAsync(activities, cancellationToken);
                    }
                }
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        private async Task<DialogTurnResult> ShowTokenResponseAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            var result = context.Result as TokenResponse;
            if (result == null)
            {
                await context.Context.SendActivityAsync(MessageFactory.Text("No token response from OAuthPrompt"));
            }
            else
            {
                await context.Context.SendActivityAsync(MessageFactory.Text($"Your token is {result.Token}"));
            }

            return await context.EndDialogAsync(null, cancellationToken);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.LanguageGeneration.Resolver;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class MultiTurnPromptsBot : IBot
    {
        /// <summary>
        /// The <see cref="LanguageGenerationResolver"/> used to generate responses to the user chatting with the bot.
        /// </summary>
        private static LanguageGenerationResolver _languageGenerationResolver;
        private static Random _random = new Random();
        private readonly BotAccessors _accessors;

        /// <summary>
        /// The <see cref="DialogSet"/> that contains all the Dialogs that can be used at runtime.
        /// </summary>
        private DialogSet _dialogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTurnPromptsBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        public MultiTurnPromptsBot(BotAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            // The DialogSet needs a DialogState accessor, it will call it when it has a turn context.
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                WelcomeStepAsync,
                LocationStepAsync,
                DateStepAsync,
                PartySizeStepAsync,
                ConfirmBookingStepAsync,
                FinalStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("details", waterfallSteps));
            _dialogs.Add(new TextPrompt("welcome"));
            _dialogs.Add(new TextPrompt("location"));
            _dialogs.Add(new NumberPrompt<int>("partySize"));
            _dialogs.Add(new TextPrompt("date"));
            _dialogs.Add(new TextPrompt("confirmBooking"));
            _dialogs.Add(new ConfirmPrompt("confirm"));

            var applicationId = "cafebot";
            var endpointKey = Keys.LanguageGenerationSubscriptionKey;
            var endpointUri = "https://platform.bing.com/speechdx/lg-dev/v1/lg";
            var tokenGenerationApiEndpoint = "https://wuppe.api.cognitive.microsoft.com/sts/v1.0/issueToken";

            var languageGenerationApplication = new LanguageGenerationApplication(applicationId, endpointKey, endpointUri);
            var languageGenerationOptions = new LanguageGenerationOptions()
            {
                TokenGenerationApiEndpoint = tokenGenerationApiEndpoint,
            };

            _languageGenerationResolver = new LanguageGenerationResolver(languageGenerationApplication, languageGenerationOptions);
        }

        /// <summary>
        /// This controls what happens when an activity gets sent to the bot.
        /// </summary>
        /// <param name="turnContext">Provides the <see cref="ITurnContext"/> for the turn of the bot.</param>
        /// <param name="cancellationToken" >(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>>A <see cref="Task"/> representing the operation result of the Turn operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // We are only interested in Message Activities.
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return;
            }

            // Run the DialogSet - let the framework identify the current state of the dialog from
            // the dialog stack and figure out what (if any) is the active dialog.
            var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);

            // If the DialogTurnStatus is Empty we should start a new dialog.
            if (results.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync("details", null, cancellationToken);
            }

            // Save the dialog state into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            // Save the user profile updates into the user state.
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// One of the functions that make up the <see cref="WaterfallDialog"/>.
        /// </summary>
        /// <param name="stepContext">The <see cref="WaterfallStepContext"/> gives access to the executing dialog runtime.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private static async Task<DialogTurnResult> WelcomeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            var outgoingActivity = new Activity()
            {
                Text = TemplateResponses.WelcomeUserTemplate,
            };
            await _languageGenerationResolver.ResolveAsync(outgoingActivity, new Dictionary<string, object>()).ConfigureAwait(false);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(outgoingActivity.Text), cancellationToken);

            return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text(FallbackMessages.AskToBookTable) }, cancellationToken);
        }

        /// <summary>
        /// One of the functions that make up the <see cref="WaterfallDialog"/>.
        /// </summary>
        /// <param name="stepContext">The <see cref="WaterfallStepContext"/> gives access to the executing dialog runtime.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private async Task<DialogTurnResult> LocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                userProfile.Location = "Cairo";
                var metaData = new Dictionary<string, object>();
                if (!userProfile.Location.Equals("NOT-Cairo"))
                {
                    metaData.Add("knowCurUserLocation", true);
                    metaData.Add("curUserLocation", "Cairo");
                }

                // User said "yes" so we will be prompting for the age.
                var outgoingActivity = new Activity()
                {
                    Text = TemplateResponses.AskForLocationTemplate,
                };

                await _languageGenerationResolver.ResolveAsync(outgoingActivity, metaData).ConfigureAwait(false);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(outgoingActivity.Text), cancellationToken);

                return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text("Please confirm") }, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(FallbackMessages.UnableToHelp), cancellationToken).ConfigureAwait(false);
                return await stepContext.EndDialogAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// One of the functions that make up the <see cref="WaterfallDialog"/>.
        /// </summary>
        /// <param name="stepContext">The <see cref="WaterfallStepContext"/> gives access to the executing dialog runtime.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                userProfile.Location = "Cairo";

                var outgoingActivity = new Activity()
                {
                    Text = TemplateResponses.AskForDateTimeTemplate,
                };

                var metaData = new Dictionary<string, object>()
                {
                    { "haveDate", true },
                };

                await _languageGenerationResolver.ResolveAsync(outgoingActivity, metaData).ConfigureAwait(false);

                // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
                return await stepContext.PromptAsync("date", new PromptOptions { Prompt = MessageFactory.Text(outgoingActivity.Text) }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(FallbackMessages.NotAvailableInThisPlace), cancellationToken).ConfigureAwait(false);
                return await stepContext.EndDialogAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// One of the functions that make up the <see cref="WaterfallDialog"/>.
        /// </summary>
        /// <param name="stepContext">The <see cref="WaterfallStepContext"/> gives access to the executing dialog runtime.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private async Task<DialogTurnResult> PartySizeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Time = (string)stepContext.Result;

            var outgoingActivity = new Activity()
            {
                Text = TemplateResponses.AskForPartySizeTemplate,
            };

            var metaData = new Dictionary<string, object>();

            await _languageGenerationResolver.ResolveAsync(outgoingActivity, metaData).ConfigureAwait(false);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
            return await stepContext.PromptAsync("partySize", new PromptOptions { Prompt = MessageFactory.Text(outgoingActivity.Text) }, cancellationToken);
        }

        /// <summary>
        /// One of the functions that make up the <see cref="WaterfallDialog"/>.
        /// </summary>
        /// <param name="stepContext">The <see cref="WaterfallStepContext"/> gives access to the executing dialog runtime.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private async Task<DialogTurnResult> ConfirmBookingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.PartySize = (int)stepContext.Result;

            var outgoingActivity = new Activity()
            {
                Text = TemplateResponses.ConfirmBookingReadoutTemplate,
            };

            var metaData = new Dictionary<string, object>()
            {
                { "partySize", userProfile.PartySize },
                { "userLocation", userProfile.Location },
                { "dateTimeReadout", userProfile.Time },
            };

            await _languageGenerationResolver.ResolveAsync(outgoingActivity, metaData).ConfigureAwait(false);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(outgoingActivity.Text), cancellationToken);
            return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text("Please confirm") }, cancellationToken);
        }

        /// <summary>
        /// One of the functions that make up the <see cref="WaterfallDialog"/>.
        /// </summary>
        /// <param name="stepContext">The <see cref="WaterfallStepContext"/> gives access to the executing dialog runtime.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="DialogTurnResult"/> to communicate some flow control back to the containing WaterfallDialog.</returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                var outgoingActivity = new Activity()
                {
                    Text = TemplateResponses.BookingConfirmationReadoutTemplate,
                };

                var metaData = new Dictionary<string, object>()
                {
                    { "confNumber", GenerateRandomString(5) },
                };

                await _languageGenerationResolver.ResolveAsync(outgoingActivity, metaData).ConfigureAwait(false);

                // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(outgoingActivity.Text), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Alright then, let's start over !"), cancellationToken).ConfigureAwait(false);
            }

            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}

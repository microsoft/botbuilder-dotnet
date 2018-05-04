// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;

namespace AspNetCore_RichCards_Bot
{
    public class RichCardsBot : IBot
    {
        private readonly DialogSet dialogs;
        private readonly ChoicePromptOptions cardOptions;

        public RichCardsBot()
        {
            dialogs = new DialogSet();
            // Choice prompt with list style to show card types
            var cardPrompt = new ChoicePrompt(Culture.English)
            {
                Style = Microsoft.Bot.Builder.Prompts.ListStyle.List
            };
            cardOptions = GenerateOptions();

            // Register the card prompt
            dialogs.Add("cardPrompt", cardPrompt);

            // Create a dialog waterfall for prompting the user
            dialogs.Add("cardSelector", new WaterfallStep[] { ChoiceCardStep, ShowCardStep });
        }

        public async Task OnTurn(ITurnContext context)
        {
            var state = context.GetConversationState<Dictionary<string, object>>();
            var dialogContext = dialogs.CreateContext(context, state);
            if (context.Activity.Type == ActivityTypes.Message)
            {
                await dialogContext.Continue();
                if (!context.Responded)
                {
                    await dialogContext.Begin("cardSelector");
                }
            }
        }

        // Create our prompt's choices
        private ChoicePromptOptions GenerateOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "Adaptive card",
                        Synonyms = new List<string>() { "1", "adaptive card" }
                    },
                    new Choice()
                    {
                        Value = "Animation card",
                        Synonyms = new List<string>() { "2", "animation card" }
                    },
                    new Choice()
                    {
                        Value = "Audio card",
                        Synonyms = new List<string>() { "3", "audio card" }
                    },
                    new Choice()
                    {
                        Value = "Hero card",
                        Synonyms = new List<string>() { "4", "hero card" }
                    },
                    new Choice()
                    {
                        Value = "Receipt card",
                        Synonyms = new List<string>() { "5", "receipt card" }
                    },
                    new Choice()
                    {
                        Value = "Signin card",
                        Synonyms = new List<string>() { "6", "signin card" }
                    },
                    new Choice()
                    {
                        Value = "Thumbnail card",
                        Synonyms = new List<string>() { "7", "thumbnail card" }
                    },
                    new Choice()
                    {
                        Value = "Video card",
                        Synonyms = new List<string>() { "8", "video card" }
                    },
                    new Choice()
                    {
                        Value = "All card",
                        Synonyms = new List<string>() { "9", "all card" }
                    }
                }
            };
        }

        private Task ChoiceCardStep(DialogContext dialogContext, object result, SkipStepFunction next)
        {
            return dialogContext.Prompt("cardPrompt", "Which card would you like to choose?", cardOptions);
        }

        private async Task ShowCardStep(DialogContext dialogContext, object result, SkipStepFunction next)
        {
            var selectedCard = (result as Microsoft.Bot.Builder.Prompts.ChoiceResult).Value.Value;
            var activity = dialogContext.Context.Activity;
            switch (selectedCard)
            {
                case "Adaptive card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateAdaptiveCardAttachment()));
                    break;
                case "Animation card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateAnimationCardAttachment()));
                    break;
                case "Audio card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateAudioCardAttachment()));
                    break;
                case "Hero card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateHeroCardAttachment()));
                    break;
                case "Receipt card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateReceiptCardAttachment()));
                    break;
                case "Signin card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateSignInCardAttachment()));
                    break;
                case "Thumbnail card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateThumbnailCardAttachment()));
                    break;
                case "Video card":
                    await dialogContext.Context.SendActivity(CreateResponse(activity, CreateVideoCardAttacment()));
                    break;
                default: // all cards
                    await dialogContext.Context.SendActivities(new Activity[]
                    {
                        CreateResponse(activity, CreateAdaptiveCardAttachment()),
                        CreateResponse(activity, CreateAnimationCardAttachment()),
                        CreateResponse(activity, CreateAudioCardAttachment()),
                        CreateResponse(activity, CreateHeroCardAttachment()),
                        CreateResponse(activity, CreateReceiptCardAttachment()),
                        CreateResponse(activity, CreateSignInCardAttachment()),
                        CreateResponse(activity, CreateThumbnailCardAttachment()),
                        CreateResponse(activity, CreateVideoCardAttacment())
                    });
                    break;
            }
            await dialogContext.End();
        }

        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Methods to generate cards
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\adaptiveCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard)
            };
        }

        private Attachment CreateAnimationCardAttachment()
        {
            return new AnimationCard()
            {
                Title = "Microsoft Bot Framework",
                Media = new List<MediaUrl>()
                    {
                        new MediaUrl("http://i.giphy.com/Ki55RUbOV5njy.gif")
                    },
                Subtitle = "Animation Card"
            }.ToAttachment();
        }

        private Attachment CreateAudioCardAttachment()
        {
            return new AudioCard()
            {
                Title = "I am your father",
                Media = new List<MediaUrl>()
                {
                    new MediaUrl("http://www.wavlist.com/movies/004/father.wav")
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "Read more",
                        Value = "https://en.wikipedia.org/wiki/The_Empire_Strikes_Back"
                    }
                },
                Subtitle = "Star Wars: Episode V - The Empire Strikes Back",
                Text = "The Empire Strikes Back (also known as Star Wars: Episode V – The Empire Strikes Back) is a 1980 American epic space opera film directed by Irvin Kershner. Leigh Brackett and Lawrence Kasdan wrote the screenplay, with George Lucas writing the film\'s story and serving as executive producer. The second installment in the original Star Wars trilogy, it was produced by Gary Kurtz for Lucasfilm Ltd. and stars Mark Hamill, Harrison Ford, Carrie Fisher, Billy Dee Williams, Anthony Daniels, David Prowse, Kenny Baker, Peter Mayhew and Frank Oz.",
                Image = new ThumbnailUrl("https://upload.wikimedia.org/wikipedia/en/3/3c/SW_-_Empire_Strikes_Back.jpg")
            }.ToAttachment();
        }

        private Attachment CreateHeroCardAttachment()
        {
            return new HeroCard()
            {
                Title = "",
                Images = new List<CardImage>()
                {
                    new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg")
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "Get Started",
                        Value = "https://docs.microsoft.com/en-us/azure/bot-service/"
                    }
                }
            }.ToAttachment();
        }

        private Attachment CreateReceiptCardAttachment()
        {
            return new ReceiptCard()
            {
                Title = "John Doe",
                Facts = new List<Fact>()
                {
                    new Fact(key: "Order Number", value: "1234"),
                    new Fact(key: "Payment Method", value: "VISA 5555-****")
                },
                Items = new List<ReceiptItem>()
                {
                    new ReceiptItem()
                    {
                        Title = "Data Transfer",
                        Price = "$38.45",
                        Quantity = "368",
                        Image = new CardImage("https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")
                    },
                    new ReceiptItem()
                    {
                        Title = "App Service",
                        Price = "$45.00",
                        Quantity = "720",
                        Image = new CardImage("https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")
                    }
                },
                Tax = "$7.50",
                Total = "$90.95",
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "More Information",
                        Value = "https://azure.microsoft.com/en-us/pricing/details/bot-service/"
                    }
                }
            }.ToAttachment();
        }

        private Attachment CreateSignInCardAttachment()
        {
            return new SigninCard()
            {
                Text = "BotFramework Sign-in Card",
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Type = ActionTypes.Signin,
                        Title = "Sign-in",
                        Value = "https://login.microsoftonline.com"
                    }
                }
            }.ToAttachment();
        }

        private Attachment CreateThumbnailCardAttachment()
        {
            return new ThumbnailCard()
            {
                Title = "BotFramework Thumbnail Card",
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = "https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg"
                    }
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "Get Started",
                        Value = "https://docs.microsoft.com/en-us/azure/bot-service/"
                    }
                },
                Subtitle = "Your bots — wherever your users are talking",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are, from text/sms to Skype, Slack, Office 365 mail and other popular services."
            }.ToAttachment();
        }

        private Attachment CreateVideoCardAttacment()
        {
            return new VideoCard()
            {
                Title = "Big Buck Bunny",
                Media = new List<MediaUrl>()
                {
                    new MediaUrl("http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4")
                },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "Learn More",
                        Value = "https://peach.blender.org/"
                    }
                },
                Subtitle = "by the Blender Institute",
                Text = "Big Buck Bunny (code-named Peach) is a short computer-animated comedy film by the Blender Institute, part of the Blender Foundation. Like the foundation\'s previous film Elephants Dream, the film was made using Blender, a free software application for animation made by the same foundation. It was released as an open-source film under Creative Commons License Attribution 3.0."
            }.ToAttachment();
        }
    }
}

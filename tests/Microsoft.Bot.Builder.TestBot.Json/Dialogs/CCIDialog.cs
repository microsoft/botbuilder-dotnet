//using System;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Autofac.Features.Indexed;
//using Microsoft.Bot.Builder.Dialogs;
//using Microsoft.CCI.BotProvider;
//using Microsoft.CCI.Content;
//using Microsoft.CCI.OnlineEngine.Content;
//using Microsoft.CCI.OnlineEngine.Dialog.Engine.Apis;
//using Microsoft.CCI.OnlineEngine.Dialog.Engine.Options;
//using Microsoft.CCI.OnlineEngine.EngineBuilder;
//using Microsoft.CCI.OnlineEngine.Metrics;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;

//namespace Microsoft.Bot.Builder.TestBot.Json.Dialogs
//{
//    public class CCIDialog : IDialog
//    {
//        private Contents _contents;
//        private ConversationApi _conversationApi;

//        public CCIDialog(string contentPath)
//        {
//            var loggerFactory = new LoggerFactory();
//            var logger = loggerFactory.CreateLogger<CCIDialog>();
//            var botContent = JsonConvert.DeserializeObject<BotContent>(File.ReadAllText(contentPath));
//            _contents = new Contents(botContent, botContent.Version, null, logger);
//            _conversationApi = new ConversationApi(
//                null,
//                _contents,
//                new CCIConversationActionsMap(),
//                Enumerable.Empty<IRequestPreprocessor>(),
//                Enumerable.Empty<IRequestPostprocessor>(),
//                new CCISessionLoggerProvider(),
//                loggerFactory.CreateLogger<ConversationApi>(),
//                new DiagnosticOptions());
//        }

//        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public IBotTelemetryClient TelemetryClient { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//        public Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            throw new NotImplementedException();
//        }

//        public Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            throw new NotImplementedException();
//        }

//        public Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            throw new NotImplementedException();
//        }

//        public Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            throw new NotImplementedException();
//        }

//        public Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            throw new NotImplementedException();
//        }

//        private class CCIConversationActionsMap : IIndex<string, IConversationAction>
//        {
//            public CCIConversationActionsMap()
//            {
//            }

//            public IConversationAction this[string key] => throw new NotImplementedException();

//            public bool TryGetValue(string key, out IConversationAction value)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        private class CCISessionLoggerProvider: ISessionLoggerProvider
//        {
//            public CCISessionLoggerProvider()
//            {
//            }

//            Microsoft.CCI.BotProvider.ITranscriptLogger ISessionLoggerProvider.Logger => throw new NotImplementedException();


//            public void Initialize(Microsoft.CCI.BotProvider.ITranscriptLogger logger)
//            {
//                throw new NotImplementedException();
//            }
//        }
//    }
//}

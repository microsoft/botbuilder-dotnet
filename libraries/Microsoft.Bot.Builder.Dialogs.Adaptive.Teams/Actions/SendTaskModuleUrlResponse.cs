// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Send a url Task Module Continue response to the user.
    /// </summary>
    public class SendTaskModuleUrlResponse : BaseSendTaskModuleContinueResponse
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendTaskModuleUrlResponse";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendTaskModuleUrlResponse"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendTaskModuleUrlResponse([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression for the Url of the Task Module response.
        /// </summary>
        /// <value>
        /// An string expression. 
        /// </value>
        [JsonProperty("url")]
        public StringExpression Url { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the Fallback Url the Task Module Task Info response.
        /// </summary>
        /// <value>
        /// An string expression. 
        /// </value>
        [JsonProperty("fallbackUrl")]
        public StringExpression FallbackUrl { get; set; }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            string url = Url.GetValueOrNull(dc.State);
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException($"Missing {nameof(Url)} for {Kind}.");
            }

            var title = Title.GetValueOrNull(dc.State);
            var height = Height.GetValueOrNull(dc.State);
            var width = Width.GetValueOrNull(dc.State);
            
            var fallbackUrl = FallbackUrl.GetValueOrNull(dc.State);
            var completionBotId = CompletionBotId.GetValueOrNull(dc.State);

            var response = new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Title = title,
                        Url = url,
                        FallbackUrl = fallbackUrl,
                        Height = height,
                        Width = width,
                        CompletionBotId = completionBotId,
                    },
                },
                CacheInfo = GetCacheInfo(dc),
            };

            var responseActivity = CreateInvokeResponseActivity(response);
            ResourceResponse sendResponse = await dc.Context.SendActivityAsync(responseActivity, cancellationToken: cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(sendResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}('{Url?.ToString() ?? string.Empty},{Title?.ToString() ?? string.Empty}')";
        }
    }
}

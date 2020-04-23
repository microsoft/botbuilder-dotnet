// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.LanguageGeneration.Events;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator implementation which uses LGFile. 
    /// </summary>
    public class TemplateEngineLanguageGenerator : LanguageGenerator
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TemplateEngineLanguageGenerator";

        private const string DEFAULTLABEL = "Unknown";

        private readonly LanguageGeneration.Templates lg;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        public TemplateEngineLanguageGenerator()
        {
            this.lg = new LanguageGeneration.Templates();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="engine">template engine.</param>
        public TemplateEngineLanguageGenerator(LanguageGeneration.Templates engine = null)
        {
            this.lg = engine ?? new LanguageGeneration.Templates();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="lgText">lg template text.</param>
        /// <param name="id">optional label for the source of the templates (used for labeling source of template errors).</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(string lgText, string id, Dictionary<string, IList<IResource>> resourceMapping)
        {
            this.Id = id ?? DEFAULTLABEL;
            var (_, locale) = LGResourceLoader.ParseLGFileName(id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            this.lg = LanguageGeneration.Templates.ParseText(lgText ?? string.Empty, Id, importResolver, registeSourceMap: SourceMapRegister);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="filePath">lg template file absolute path.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(string filePath, Dictionary<string, IList<IResource>> resourceMapping)
        {
            filePath = PathUtils.NormalizePath(filePath);
            this.Id = Path.GetFileName(filePath);

            var (_, locale) = LGResourceLoader.ParseLGFileName(Id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            this.lg = LanguageGeneration.Templates.ParseFile(filePath, importResolver, registeSourceMap: SourceMapRegister);
        }

        /// <summary>
        /// Gets or sets id of the source of this template (used for labeling errors).
        /// </summary>
        /// <value>
        /// Id of the source of this template (used for labeling errors).
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Method to generate text from given template and data.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>generated text.</returns>
        public override async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            EventHandler onEvent = async (s, e) => await HandlerTemplateEvaluationEvent(turnContext, s, e);
            onEvent += async (s, e) => await HandlerExpressionEvaluationEvent(turnContext, s, e);
            onEvent += async (s, e) => await HandlerMessageEvent(turnContext, s, e);

            try
            {
                return await Task.FromResult(lg.EvaluateText(template, data, new EvaluationOptions { OnEvent = onEvent }).ToString());
            }
            catch (Exception err)
            {
                if (!string.IsNullOrEmpty(this.Id))
                {
                    throw new Exception($"{Id}:{err.Message}");
                }

                throw;
            }
        }

        private async Task HandlerTemplateEvaluationEvent(ITurnContext turnContext, object sender, EventArgs e)
        {
            if (e is BeginTemplateEvaluationArgs be && Path.IsPathRooted(be.Source))
            {
                await turnContext.GetDebugger().StepAsync(new DialogContext(new DialogSet(), turnContext, new DialogState()), sender, be.Type, new System.Threading.CancellationToken());
            }
        }

        private async Task HandlerExpressionEvaluationEvent(ITurnContext turnContext, object sender, EventArgs e)
        {
            if (e is BeginExpressionEvaluationArgs expr && Path.IsPathRooted(expr.Source))
            {
                await turnContext.GetDebugger().StepAsync(new DialogContext(new DialogSet(), turnContext, new DialogState()), sender, expr.Type, new System.Threading.CancellationToken());
            }
        }

        private async Task HandlerMessageEvent(ITurnContext turnContext, object sender, EventArgs e)
        {
            if (e is MessageArgs message && Path.IsPathRooted(message.Source))
            {
                if (turnContext.GetDebugger() is IDebugger dda)
                {
                    await dda.OutputAsync(message.Text, sender, message.Text, new System.Threading.CancellationToken());
                }
            }
        }

        private void SourceMapRegister(object sender, EventArgs e)
        {
            if (e is RegisteSourceMapArgs rs && sender != null)
            {
                var sourceRange = rs.SourceRange;
                var debugSM = new Debugging.SourceRange(
                    sourceRange.Source,
                    sourceRange.Range.Start.Line,
                    sourceRange.Range.Start.Character,
                    sourceRange.Range.End.Line,
                    sourceRange.Range.End.Character);
                DebugSupport.SourceMap.Add(sender, debugSM);
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator implementation which uses LGFile. 
    /// </summary>
    public class TemplateEngineLanguageGenerator : LanguageGenerator
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TemplateEngineLanguageGenerator";

        private const string DEFAULTLABEL = "Unknown";

        private static readonly TaskFactory TaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

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
        [Obsolete("This method will soon be deprecated. Use LGResource as the first parameter instead.")]
        public TemplateEngineLanguageGenerator(string lgText, string id, Dictionary<string, IList<Resource>> resourceMapping)
        {
            this.Id = id ?? DEFAULTLABEL;
            var (_, locale) = LGResourceLoader.ParseLGFileName(id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var lgResource = new LGResource(Id, Id, lgText ?? string.Empty);
            this.lg = LanguageGeneration.Templates.ParseResource(lgResource, importResolver);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="filePath">lg template file absolute path.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        [Obsolete("This method will soon be deprecated. Use LGResource as the first parameter instead.")]
        public TemplateEngineLanguageGenerator(string filePath, Dictionary<string, IList<Resource>> resourceMapping)
        {
            filePath = PathUtils.NormalizePath(filePath);
            this.Id = Path.GetFileName(filePath);

            var (_, locale) = LGResourceLoader.ParseLGFileName(Id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var resource = new LGResource(Id, filePath, File.ReadAllText(filePath));
            this.lg = LanguageGeneration.Templates.ParseResource(resource, importResolver);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(Resource resource, Dictionary<string, IList<Resource>> resourceMapping)
        {
            this.Id = resource.Id;

            var (_, locale) = LGResourceLoader.ParseLGFileName(Id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var content = resource.ReadTextAsync().GetAwaiter().GetResult();
            var lgResource = new LGResource(Id, resource.FullName, content);
            this.lg = LanguageGeneration.Templates.ParseResource(lgResource, importResolver);
            RegisterSourcemap(lg, resource);
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
        /// <param name="dialogContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>generated text.</returns>
        public override Task<object> GenerateAsync(DialogContext dialogContext, string template, object data, CancellationToken cancellationToken = default)
        {
            EventHandler onEvent = (s, e) => RunSync(() => HandlerLGEventAsync(dialogContext, s, e, cancellationToken));

            var lgOpt = new EvaluationOptions() { Locale = dialogContext.GetLocale(), OnEvent = onEvent };

            try
            {
                return Task.FromResult(lg.EvaluateText(template, data, lgOpt));
            }
            catch (Exception err)
            {
                if (!string.IsNullOrEmpty(this.Id))
                {
                    throw new InvalidOperationException($"{Id}:{err.Message}");
                }

                throw;
            }
        }

        private static void RunSync(Func<Task> func)
        {
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
            TaskFactory.StartNew(() =>
            {
                return func();
            }).Unwrap().GetAwaiter().GetResult();
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
        }

        private static void RegisterSourcemap(LanguageGeneration.Templates templates, Resource resource)
        {
            foreach (var template in templates.AllTemplates)
            {
                RegisterSourcemap(template, template.SourceRange, template.SourceRange.Source);
                foreach (var expressionRef in template.Expressions)
                {
                    RegisterSourcemap(expressionRef, expressionRef.SourceRange, resource.FullName);
                }
            }
        }

        private static void RegisterSourcemap(object item, LanguageGeneration.SourceRange sr, string path)
        {
            if (Path.IsPathRooted(path))
            {
                var debugSM = new Debugging.SourceRange(
                    path,
                    sr.Range.Start.Line,
                    sr.Range.Start.Character + 1,
                    sr.Range.End.Line,
                    sr.Range.End.Character + 1);

                if (!DebugSupport.SourceMap.TryGetValue(item, out var _))
                {
                    DebugSupport.SourceMap.Add(item, debugSM);
                }
            }
        }

        private async Task HandlerLGEventAsync(DialogContext dialogContext, object sender, EventArgs eventArgs, CancellationToken cancellationToken = default)
        {
            // skip the events that is not LG event or the event path is invalid.
            if (!(eventArgs is LGEventArgs))
            {
                await Task.CompletedTask.ConfigureAwait(false);
            }

            if (eventArgs is BeginTemplateEvaluationArgs || eventArgs is BeginExpressionEvaluationArgs)
            {
                // Send debugger event
                await dialogContext.GetDebugger().StepAsync(dialogContext, sender, DialogEvents.Custom, cancellationToken).ConfigureAwait(false);
            }
            else if (eventArgs is MessageArgs message && dialogContext.GetDebugger() is IDebugger dda)
            {
                // send debugger message
                await dda.OutputAsync(message.Text, sender, message.Text, cancellationToken).ConfigureAwait(false);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}

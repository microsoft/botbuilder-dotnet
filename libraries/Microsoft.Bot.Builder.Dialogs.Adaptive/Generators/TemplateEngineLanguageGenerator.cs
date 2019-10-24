// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator implementation which uses TemplateEngine. 
    /// </summary>
    public class TemplateEngineLanguageGenerator : ILanguageGenerator
    {
        private const string DEFAULTLABEL = "Unknown";

        // lazy loading
        private TemplateEngine engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        public TemplateEngineLanguageGenerator()
        {
            this.engine = new TemplateEngine();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="lgText">lg template text.</param>
        /// <param name="id">optional label for the source of the templates (used for labeling source of template errors).</param>
        /// <param name="multiLanguageResolver">template resource loader delegate (local) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(string lgText, string id = null, Func<string, ImportResolverDelegate> multiLanguageResolver = null)
        {
            this.LGText = lgText ?? string.Empty;
            this.Id = id ?? DEFAULTLABEL;
            this.MultiLanguageResolver = multiLanguageResolver;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="engine">template engine.</param>
        public TemplateEngineLanguageGenerator(TemplateEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        /// Gets or sets id of the source of this template (used for labeling errors).
        /// </summary>
        /// <value>
        /// Id of the source of this template (used for labeling errors).
        /// </value>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets text content of the LG file.
        /// </summary>
        /// <value>
        /// Text content of the LG file.
        /// </value>
        public string LGText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets get <see cref="ImportResolverDelegate"/> from local.
        /// </summary>
        /// <value>
        /// get <see cref="ImportResolverDelegate"/> from local.
        /// </value>
        public Func<string, ImportResolverDelegate> MultiLanguageResolver { get; set; }

        /// <summary>
        /// Method to generate text from given template and data.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>generated text.</returns>
        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            engine = InitTemplateEngine(turnContext);

            try
            {
                return await Task.FromResult(engine.Evaluate(template, data).ToString());
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

        private TemplateEngine InitTemplateEngine(ITurnContext turnContext)
        {
            if (MultiLanguageResolver != null)
            {
                var local = turnContext.Activity.Locale?.ToLower() ?? string.Empty;
                engine = new TemplateEngine().AddText(LGText, Id, MultiLanguageResolver(local));
            }
            else if (!string.IsNullOrWhiteSpace(LGText) || !string.IsNullOrWhiteSpace(Id))
            {
                engine = new TemplateEngine().AddText(LGText, Id);
            }
            else
            {
                // Do not rewrite to ??= (C# 8.0 new feature). It will break in linux/mac
                engine = engine ?? new TemplateEngine();
            }

            return engine;
        }
    }
}

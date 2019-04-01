using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class LGLanguageGenerator : ILanguageGenerator
    {
        private Dictionary<string, TemplateEngine> engines;
        private ResourceExplorer resourceManager;

        public LGLanguageGenerator(ResourceExplorer resourceExplorer, ILanguagePolicy languagePolicy = null)
        {
            this.resourceManager = resourceExplorer;
            this.LanguagePolicy = languagePolicy ?? new LanguagePolicy();
        }

        /// <summary>
        /// This allows you to specify per language the fallback policies you want
        /// </summary>
        public ILanguagePolicy LanguagePolicy { get; set; }

        public async Task LoadResources(bool force = false)
        {
            if (force || this.engines == null)
            {
                var engs = new Dictionary<string, TemplateEngine>(StringComparer.CurrentCultureIgnoreCase);

                var lgs = this.resourceManager.GetResources("lg").ToArray();
                var contents = lgs.Select(resource => File.ReadAllText(resource.FullName));

                Dictionary<string, StringBuilder> languageResources = new Dictionary<string, StringBuilder>();
                foreach (var result in contents)
                {
                    var lgText = result;
                    // get lang (HACK)
                    var iEnd = lgText.IndexOf("\r\n");
                    var firstLine = (iEnd > 0) ? lgText.Substring(0, iEnd) : lgText;

                    string lang = String.Empty;
                    iEnd = firstLine.IndexOf("]");
                    if (iEnd > 0)
                    {
                        lang = firstLine.Substring(0, iEnd).Trim('[', ']').ToLower();
                        if (!LanguagePolicy.ContainsKey(lang))
                        {
                            lang = String.Empty;
                        }
                        lgText = lgText.Substring(iEnd);
                    }

                    StringBuilder sb;
                    if (!languageResources.TryGetValue(lang, out sb))
                    {
                        sb = new StringBuilder();
                        languageResources.Add(lang, sb);
                    }

                    // add in the lg file text
                    sb.AppendLine(lgText);
                }

                foreach (var lang in languageResources.Keys)
                {
                    engs.Add(lang, TemplateEngine.FromText(languageResources[lang].ToString()));
                }

                if (!engs.ContainsKey(""))
                {
                    engs.Add(string.Empty, TemplateEngine.FromText(" "));
                }

                this.engines = engs;
            }
        }

        public async Task<string> Generate(string targetLocale, string inlineTemplate = null, string id = null, object data = null, string[] types = null, string[] tags = null, Func<string, object, object> valueBinder = null)
        {
            await LoadResources().ConfigureAwait(false);

            // see if we have any locales that match
            targetLocale = targetLocale?.ToLower() ?? string.Empty;
            var languagePolicy = new string[] { String.Empty };
            if (!this.LanguagePolicy.TryGetValue(targetLocale, out languagePolicy))
            {
                if (!this.LanguagePolicy.TryGetValue(String.Empty, out languagePolicy))
                {
                    return null;
                }
            }

            foreach (var locale in languagePolicy)
            {
                if (this.engines.TryGetValue(locale, out TemplateEngine engine))
                {
                    var lgResult = BindToTemplate(engine, inlineTemplate, id, data, types, tags, valueBinder);
                    if (lgResult != null)
                    {
                        return lgResult;
                    }
                }
            }
            return null;
        }


        private string BindToTemplate(TemplateEngine engine, string inline, string id, object data, string[] types, string[] tags, Func<string, object, object> valueBinder)
        {
            string result;
            if (!String.IsNullOrEmpty(inline))
            {
                // do inline evaluation first
                return this.TryEvaluate(engine, inline, data, valueBinder);
            }
            else if (!String.IsNullOrEmpty(id))
            {
                // try each combination of tags+type+id
                if (tags != null && types != null)
                {
                    foreach (var tag in tags)
                    {
                        foreach (var type in types)
                        {
                            result = this.TryEvaluate(engine, $"[{tag}-{type}-{id}]", data, valueBinder);
                            if (result != null)
                                return result;
                        }
                    }
                }

                // try each combination of types+id
                if (types != null)
                {
                    foreach (var type in types)
                    {
                        result = this.TryEvaluate(engine, $"[{type}-{id}]", data, valueBinder);
                        if (result != null)
                            return result;
                    }
                }

                // try each combination of tags+id
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        result = this.TryEvaluate(engine, $"[{tag}-{id}]", data, valueBinder);
                        if (result != null)
                            return result;
                    }
                }

                // try exact match on id 
                result = this.TryEvaluate(engine, $"[{id}]", data, valueBinder);
                if (result != null)
                    return result;

                return null;
            }
            throw new ArgumentException("One of Inline or Id needs to be set");
        }

        private string TryEvaluate(TemplateEngine engine, string text, object data, Func<string, object, object> valueBinder)
        {
            try
            {
                // do inline evaluation first
                return engine.Evaluate(text, data, null);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

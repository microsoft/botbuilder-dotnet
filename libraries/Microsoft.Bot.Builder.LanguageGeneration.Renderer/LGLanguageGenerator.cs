using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.LanguageGeneration.Renderer
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
                var contents = lgs.Select(async resource => (FileLocale(resource.Id), await resource.ReadTextAsync().ConfigureAwait(false)));

                Dictionary<string, string> languageResources = new Dictionary<string, string>();
                foreach (var result in contents)
                {
                    var (locale, text) = await result;
                    if (!languageResources.ContainsKey(locale))
                    {
                        languageResources[locale] = text;
                    }
                    else
                    {
                        languageResources[locale] += $"\n\n{text}";
                    }
                }

                foreach (var lang in languageResources.Keys)
                {
                    engs.Add(lang, TemplateEngine.FromText(languageResources[lang]));
                }

                if (!engs.ContainsKey(""))
                {
                    engs.Add(string.Empty, TemplateEngine.FromText(" "));
                }

                this.engines = engs;
            }
        }

        public async Task<string> Generate(string targetLocale, string inlineTemplate = null, string id = null, object data = null, string[] types = null, string[] tags = null)
        {
            await LoadResources().ConfigureAwait(false);

            // see if we have any locales that match
            targetLocale = targetLocale?.ToLower() ?? string.Empty;
            var languagePolicy = new string[] { String.Empty };
            if (!this.LanguagePolicy.TryGetValue(targetLocale, out languagePolicy))
            {
                if (!this.LanguagePolicy.TryGetValue(String.Empty, out languagePolicy))
                {
                    throw new Exception($"No supported language found for {targetLocale}");
                }
            }

            List<string> errors = new List<string>();
            foreach (var locale in languagePolicy)
            {
                if (this.engines.TryGetValue(locale, out TemplateEngine engine))
                {
                    var (result, errs) = BindToTemplate(engine, inlineTemplate, id, data, types, tags);
                    if (result != null)
                    {
                        return result;
                    }
                    errors.AddRange(errs);
                }
            }
            throw new Exception(String.Join(",\n", errors.Distinct()));
        }

        private string FileLocale(string filename)
        {
            var locale = "";
            filename = Path.GetFileNameWithoutExtension(filename);
            var start = filename.LastIndexOf('.');
            if (start == -1)
                // default
                return "";
            ++start;
            locale = filename.Substring(start, filename.Length - start).Trim().ToLower();
            if (CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => String.Compare(c.IetfLanguageTag, locale, ignoreCase: true) == 0).Any())
            {
                return locale;
            }

            return "";
        }

        private (string, string[]) BindToTemplate(TemplateEngine engine, string inline, string id, object data, string[] types, string[] tags)
        {
            List<string> errors = new List<string>();
            string result;
            string error;
            if (!String.IsNullOrEmpty(inline))
            {
                // do inline evaluation first
                if (this.TryEvaluate(engine, inline, data, out result, out error))
                {
                    return (result, null);
                }
                errors.Add(error);
                return (null, errors.ToArray());
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
                            if (this.TryEvaluate(engine, $"[{tag}-{type}-{id}]", data, out result, out error))
                            {
                                return (result, null);
                            }
                            errors.Add(error);
                        }
                    }
                }

                // try each combination of types+id
                if (types != null)
                {
                    foreach (var type in types)
                    {
                        if (this.TryEvaluate(engine, $"[{type}-{id}]", data, out result, out error))
                        {
                            return (result, null);
                        }
                        errors.Add(error);
                    }
                }

                // try each combination of tags+id
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        if (this.TryEvaluate(engine, $"[{tag}-{id}]", data, out result, out error))
                        {
                            return (result, null);
                        }
                        errors.Add(error);
                    }
                }

                // try exact match on id 
                if (this.TryEvaluate(engine, $"[{id}]", data, out result, out error))
                {
                    return (result, null);
                }
                errors.Add(error);

                return (null, errors.ToArray());
            }
            throw new ArgumentException("One of Inline or Id needs to be set");
        }

        private bool TryEvaluate(TemplateEngine engine, string text, object data, out string result, out string error)
        {
            result = null;
            error = null;
            try
            {
                // do inline evaluation first
                text = !text.Trim().StartsWith("```") && text.IndexOf('\n') >= 0
                    ? "```" + text + "```" : text;
                result = engine.Evaluate(text, data, null);
                return true;
            }
            catch (Exception err)
            {
                error = err.Message;
            }
            return false;
        }
    }
}

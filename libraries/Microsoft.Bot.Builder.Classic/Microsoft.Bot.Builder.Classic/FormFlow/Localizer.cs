// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using static Microsoft.Bot.Builder.Classic.Resource.Extensions;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    #region Documentation
    /// <summary>   A resource localizer. </summary>
    #endregion
    public class Localizer : ILocalizer
    {
        public CultureInfo Culture { get; set; }

        public void Add(string key, string translation)
        {
            if (translation != null)
            {
                _translations.Add(key, translation);
            }
        }

        public void Add(string key, IEnumerable<string> list)
        {
            if (list.Any())
            {
                _arrayTranslations.Add(key, list.ToArray());
            }
        }

        public void Add(string prefix, IReadOnlyDictionary<object, DescribeAttribute> dictionary)
        {
            foreach (var entry in dictionary)
            {
                if (entry.Value.IsLocalizable)
                {
                    if (entry.Key.GetType().IsEnum)
                    {
                        var key = entry.Key.GetType().Name + "." + entry.Key;
                        if (!_translations.ContainsKey(key))
                        {
                            Add(key, entry.Value.Description);
                            Add(key, entry.Value.Image);
                            Add(key + nameof(entry.Value.Title), entry.Value.Title);
                            Add(key + nameof(entry.Value.SubTitle), entry.Value.SubTitle);
                            Add(key + nameof(entry.Value.Message), entry.Value.Message);
                        }
                    }
                    else
                    {
                        Add(prefix + SEPARATOR + entry.Key, entry.Value.Description);
                    }
                }
            }
        }

        public void Add(string prefix, IReadOnlyDictionary<object, TermsAttribute> dictionary)
        {
            foreach (var entry in dictionary)
            {
                if (entry.Value.IsLocalizable)
                {
                    if (entry.Key.GetType().IsEnum)
                    {
                        var key = entry.Key.GetType().Name + "." + entry.Key;
                        if (!_arrayTranslations.ContainsKey(key))
                        {
                            _arrayTranslations.Add(key, entry.Value.Alternatives);
                        }
                    }
                    else
                    {
                        _arrayTranslations.Add(prefix + SEPARATOR + entry.Key, entry.Value.Alternatives);
                    }
                }
            }
        }

        public void Add(string prefix, IReadOnlyDictionary<TemplateUsage, TemplateAttribute> templates)
        {
            foreach (var template in templates.Values)
            {
                Add(prefix, template);
            }
        }

        public void Add(string prefix, TemplateAttribute template)
        {
            if (template.IsLocalizable)
            {
                _templateTranslations.Add(MakeList(prefix, template.Usage.ToString()), template.Patterns);
            }
        }

        public bool Lookup(string key, out string value)
        {
            return _translations.TryGetValue(key, out value);
        }

        public bool LookupValues(string key, out string[] values)
        {
            return _arrayTranslations.TryGetValue(key, out values);
        }

        public void LookupDictionary(string prefix, IDictionary<object, DescribeAttribute> dictionary)
        {
            foreach (var entry in dictionary)
            {
                var key = entry.Key;
                var desc = entry.Value;
                string skey;
                if (key.GetType().IsEnum)
                {
                    skey = key.GetType().Name + "." + key;
                }
                else
                {
                    skey = prefix + SEPARATOR + key;
                }
                string value;
                if (_translations.TryGetValue(skey, out value))
                {
                    desc.Description = value;
                }
                if (_translations.TryGetValue(skey + nameof(desc.Image), out value))
                {
                    desc.Image = value;
                }
                if (_translations.TryGetValue(skey + nameof(desc.Title), out value))
                {
                    desc.Title = value;
                }
                if (_translations.TryGetValue(skey + nameof(desc.SubTitle), out value))
                {
                    desc.SubTitle = value;
                }
                if (_translations.TryGetValue(skey + nameof(desc.Message), out value))
                {
                    desc.Message = value;
                }
            }
        }

        public void LookupDictionary(string prefix, IDictionary<object, TermsAttribute> dictionary)
        {
            foreach (var key in dictionary.Keys.ToArray())
            {
                string skey;
                if (key.GetType().IsEnum)
                {
                    skey = key.GetType().Name + "." + key;
                }
                else
                {
                    skey = prefix + SEPARATOR + key;
                }
                string[] values;
                if (_arrayTranslations.TryGetValue(skey, out values))
                {
                    dictionary[key] = new TermsAttribute(values);
                }
            }
        }

        public void LookupTemplates(string prefix, IDictionary<TemplateUsage, TemplateAttribute> templates)
        {
            foreach (var template in templates.Values)
            {
                string[] patterns;
                if (_templateTranslations.TryGetValue(prefix + SEPARATOR + template.Usage, out patterns))
                {
                    template.Patterns = patterns;
                }
            }
        }

        public void Remove(string key)
        {
            _translations.Remove(key);
            _arrayTranslations.Remove(key);
            _templateTranslations.Remove(key);
        }

        public ILocalizer Load(IDictionaryEnumerator reader, out IEnumerable<string> missing, out IEnumerable<string> extra)
        {
            var lmissing = new List<string>();
            var lextra = new List<string>();
            var newLocalizer = new Localizer();
            while (reader.MoveNext())
            {
                var entry = (DictionaryEntry)reader.Current;
                var fullKey = (string)entry.Key;
                var semi = fullKey.LastIndexOf(SEPARATOR[0]);
                var key = fullKey.Substring(0, semi);
                var type = fullKey.Substring(semi + 1);
                var val = (string)entry.Value;
                if (type == "VALUE")
                {
                    newLocalizer.Add(key, val);
                }
                else if (type == "LIST")
                {
                    newLocalizer.Add(key, val.SplitList().ToArray());
                }
                else if (type == "TEMPLATE")
                {
                    var elements = key.SplitList();
                    var usage = elements.First();
                    var fields = elements.Skip(1);
                    var patterns = val.SplitList();
                    var template = new TemplateAttribute((TemplateUsage)Enum.Parse(typeof(TemplateUsage), usage), patterns.ToArray());
                    foreach (var field in fields)
                    {
                        newLocalizer.Add(field, template);
                    }
                }
            }

            // Find missing and extra keys
            lmissing.AddRange(_translations.Keys.Except(newLocalizer._translations.Keys));
            lmissing.AddRange(_arrayTranslations.Keys.Except(newLocalizer._arrayTranslations.Keys));
            lmissing.AddRange(_templateTranslations.Keys.Except(newLocalizer._templateTranslations.Keys));
            lextra.AddRange(newLocalizer._translations.Keys.Except(_translations.Keys));
            lextra.AddRange(newLocalizer._arrayTranslations.Keys.Except(_arrayTranslations.Keys));
            lextra.AddRange(newLocalizer._templateTranslations.Keys.Except(_templateTranslations.Keys));
            missing = lmissing;
            extra = lextra;
            return newLocalizer;
        }

        public void Save(IResourceWriter writer)
        {
            foreach (var entry in _translations)
            {
                writer.AddResource(entry.Key + SEPARATOR + "VALUE", entry.Value);
            }

            foreach (var entry in _arrayTranslations)
            {
                writer.AddResource(entry.Key + SEPARATOR + "LIST", MakeList(entry.Value));
            }

            // Switch from field;usage -> patterns
            // to usage;pattern* -> [fields]
            var byPattern = new Dictionary<string, List<string>>();
            foreach (var entry in _templateTranslations)
            {
                var names = entry.Key.SplitList().ToArray();
                var field = names[0];
                var usage = names[1];
                var key = MakeList(AddPrefix(usage, entry.Value));
                List<string> fields;
                if (byPattern.TryGetValue(key, out fields))
                {
                    fields.Add(field);
                }
                else
                {
                    byPattern.Add(key, new List<string> { field });
                }
            }

            // WriteAsync out TEMPLATE;usage;field* -> pattern*
            foreach (var entry in byPattern)
            {
                var elements = entry.Key.SplitList().ToArray();
                var usage = elements[0];
                var patterns = elements.Skip(1);
                var key = usage + SEPARATOR + MakeList(entry.Value) + SEPARATOR + "TEMPLATE";
                writer.AddResource(key, MakeList(patterns));
            }
        }

        protected IEnumerable<string> AddPrefix(string prefix, IEnumerable<string> suffix)
        {
            return new string[] { prefix }.Union(suffix);
        }

        protected Dictionary<string, string> _translations = new Dictionary<string, string>();
        protected Dictionary<string, string[]> _arrayTranslations = new Dictionary<string, string[]>();
        protected Dictionary<string, string[]> _templateTranslations = new Dictionary<string, string[]>();
    }
}

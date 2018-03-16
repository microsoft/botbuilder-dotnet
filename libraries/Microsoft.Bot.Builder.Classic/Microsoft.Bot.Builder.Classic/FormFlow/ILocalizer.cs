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

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace Microsoft.Bot.Builder.Classic.FormFlow.Advanced
{
    #region Documentation
    /// <summary>   Interface for localizing string resources. </summary>
    #endregion
    public interface ILocalizer
    {
        /// <summary>
        /// Return the localizer culture.
        /// </summary>
        /// <returns>Current culture.</returns>
        CultureInfo Culture { get; set; }

        /// <summary>
        /// Add a key and its translation.
        /// </summary>
        /// <param name="key">Key for indexing translation.</param>
        /// <param name="translation">Translation for key.</param>
        void Add(string key, string translation);

        /// <summary>
        /// Add a key and a list of translations separated by semi-colon.
        /// </summary>
        /// <param name="key">Key for indexing translation list.</param>
        /// <param name="list">List of translated terms.</param>
        void Add(string key, IEnumerable<string> list);

        #region Documentation
        /// <summary>   Adds value from dictionary under object if enumeration and prefix;object otherwise. </summary>
        /// <param name="prefix">       The resource prefix. </param>
        /// <param name="dictionary">   The dictionary to add. </param>
        #endregion
        void Add(string prefix, IReadOnlyDictionary<object, DescribeAttribute> dictionary);

        #region Documentation
        /// <summary>   Adds values from dictionary separated by semi-colons under object if enumeration and prefix;object otherwise.</summary>
        /// <param name="prefix">       The resource prefix. </param>
        /// <param name="dictionary">   The dictionary to add. </param>
        #endregion
        void Add(string prefix, IReadOnlyDictionary<object, TermsAttribute> dictionary);

        #region Documentation
        /// <summary>   Adds patterns from template separated by semi-colons under prefix;usage. </summary>
        /// <param name="prefix">       The resource prefix. </param>
        /// <param name="templates">    The template dictionary to add. </param>
        #endregion
        void Add(string prefix, IReadOnlyDictionary<TemplateUsage, TemplateAttribute> templates);

        #region Documentation
        /// <summary>   Adds patterns from template separated by semi-colons under prefix;usage.</summary>
        /// <param name="prefix">       The resource prefix. </param>
        /// <param name="template">     The template to add. </param>
        #endregion
        void Add(string prefix, TemplateAttribute template);

        /// <summary>
        /// Translate a key to a translation.
        /// </summary>
        /// <param name="key">Key to lookup.</param>
        /// <param name="value">Value to set if present.</param>
        /// <returns>True if value is found. </returns>
        bool Lookup(string key, out string value);

        /// <summary>
        /// Translate a key to an array of values.
        /// </summary>
        /// <param name="key">Key to lookup.</param>
        /// <param name="values">Array value to set if present.</param>
        /// <returns>True if value is found. </returns>
        bool LookupValues(string key, out string[] values);

        #region Documentation
        /// <summary>   Look up prefix;object from dictionary and replace value from localizer. </summary>
        /// <param name="prefix">       The prefix. </param>
        /// <param name="dictionary">   Dictionary with existing values. </param>
        #endregion
        void LookupDictionary(string prefix, IDictionary<object, DescribeAttribute> dictionary);

        #region Documentation
        /// <summary>   Look up prefix;object from dictionary and replace values from localizer. </summary>
        /// <param name="prefix">       The prefix. </param>
        /// <param name="dictionary">   Dictionary with existing values. </param>
        #endregion
        void LookupDictionary(string prefix, IDictionary<object, TermsAttribute> dictionary);

        #region Documentation
        /// <summary>   Looks up prefix;usage and replace patterns in template from localizer. </summary>
        /// <param name="prefix">       The prefix. </param>
        /// <param name="templates">    Template dictionary with existing values. </param>
        #endregion
        void LookupTemplates(string prefix, IDictionary<TemplateUsage, TemplateAttribute> templates);

        /// <summary>
        /// Remove a key from the localizer.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        void Remove(string key);

        /// <summary>
        /// Save localizer resources to stream.
        /// </summary>
        /// <param name="writer">Where to write resources.</param>
        /// <remarks>
        /// Resource values are all strings.  The key and value can have different parts separated by semi-colons.
        /// Key | Value | Description
        /// ----|-------|------------
        /// key;VALUE | string | Simple value.
        /// key;LIST | string[;string]* | List of values.
        /// usage;field[;field]*;TEMPLATE | pattern[;pattern]* | List of template patterns.  Key includes fields that use template.
        /// </remarks>
        void Save(IResourceWriter writer);

        /// <summary>
        /// Load the localizer from a stream.
        /// </summary>
        /// <param name="reader">Dictionary with resources.</param>
        /// <param name="missing">Keys found in current localizer that are not in loaded localizer.</param>
        /// <param name="extra">Keys found in loaded localizer that were not in current localizer.</param>
        /// <returns>New localizer from reader.</returns>
        /// <remarks>
        /// <see cref="Save(IResourceWriter)"/> to see resource format.
        /// </remarks>
        ILocalizer Load(IDictionaryEnumerator reader, out IEnumerable<string> missing, out IEnumerable<string> extra);
    }
}

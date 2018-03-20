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

using Microsoft.Bot.Builder.Classic.Dialogs;
using Microsoft.Bot.Builder.Classic.FormFlow.Advanced;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.FormFlow
{
    #region Documentation
    /// <summary>   Form definition interface. </summary>
    /// <typeparam name="T">    Form state. </typeparam>
    #endregion
    public abstract class IForm<T>
        where T : class
    {
        /// <summary>
        /// Fields that make up form.
        /// </summary>
        public abstract IFields<T> Fields { get; }

        #region Documentation
        /// <summary>   Save all string resources to binary stream for future localization. </summary>
        /// <param name="writer">   Where to write resources. </param>
        #endregion
        public abstract void SaveResources(IResourceWriter writer);

        #region Documentation
        /// <summary>   Localize all string resources from binary stream. </summary>
        /// <param name="reader">   Where to read resources. </param>
        /// <param name="missing">  [out] Any values in the form, but missing from the stream. </param>
        /// <param name="extra">    [out] Any values in the stream, but missing from the form. </param>
        /// <remarks>When you localize all form string resources will be overridden if present in the stream.
        ///          Otherwise the value will remain unchanged.
        /// </remarks>
        #endregion
        public abstract void Localize(IDictionaryEnumerator reader, out IEnumerable<string> missing, out IEnumerable<string> extra);

        // Internals
        internal abstract ILocalizer Resources { get; }
        internal abstract FormConfiguration Configuration { get; }
        internal abstract IReadOnlyList<IStep<T>> Steps { get; }
        internal abstract OnCompletionAsyncDelegate<T> Completion { get; }
        internal abstract Task<FormPrompt> Prompt(IDialogContext context, FormPrompt prompt, T state, IField<T> field);
    }
}

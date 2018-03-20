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


namespace Microsoft.Bot.Builder.Classic.Luis
{
    /// <summary>
    /// Interface containing optional parameters for a LUIS request.
    /// </summary>
    public interface ILuisOptions
    {
        /// <summary>
        /// Indicates if logging of queries to LUIS is allowed.
        /// </summary>
        bool? Log { get; set; }

        /// <summary>
        /// Turn on spell checking.
        /// </summary>
        bool? SpellCheck { get; set; }

        /// <summary>
        /// Use the staging endpoint.
        /// </summary>
        bool? Staging { get; set; }

        /// <summary>
        /// The time zone offset.
        /// </summary>
        double? TimezoneOffset { get; set; }

        /// <summary>
        /// The verbose flag.
        /// </summary>
        bool? Verbose { get; set; }

        /// <summary>
        /// The Bing Spell Check subscription key.
        /// </summary>
        string BingSpellCheckSubscriptionKey { get; set; }
    }

    public static partial class Extensions
    {
        public static void Apply(this ILuisOptions source, ILuisOptions target)
        {
            if (source.Log.HasValue) target.Log = source.Log.Value;
            if (source.SpellCheck.HasValue) target.SpellCheck = source.SpellCheck.Value;
            if (source.Staging.HasValue) target.Staging = source.Staging.Value;
            if (source.TimezoneOffset.HasValue) target.TimezoneOffset = source.TimezoneOffset.Value;
            if (source.Verbose.HasValue) target.Verbose = source.Verbose.Value;
            if (!string.IsNullOrWhiteSpace(source.BingSpellCheckSubscriptionKey)) target.BingSpellCheckSubscriptionKey = source.BingSpellCheckSubscriptionKey;
        }
    }
}


﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Recognizes hashtag input.
    /// </summary>
    public class HashtagEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.HashtagEntityRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="HashtagEntityRecognizer"/> class.
        /// </summary>
        [JsonConstructor]
        public HashtagEntityRecognizer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashtagEntityRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public HashtagEntityRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Hashtag recognizing implementation.
        /// </summary>
        /// <param name="text">Text to recognize.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The recognized <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            return SequenceRecognizer.RecognizeHashtag(text, culture);
        }
    }
}

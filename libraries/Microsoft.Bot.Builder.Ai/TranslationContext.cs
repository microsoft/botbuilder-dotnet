// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Core.Extensions;

namespace Microsoft.Bot.Builder.Ai
{
    public class TranslationContext : FlexObject
    {
        /// <summary>
        /// Original pre-translation text 
        /// </summary>
        public string SourceText { get; set; }

        /// <summary>
        /// source language 
        /// </summary>
        public string SourceLanguage { get; set; }

        /// <summary>
        /// The targeted translation language
        /// </summary>
        public string TargetLanguage { get; set; }
    }
}

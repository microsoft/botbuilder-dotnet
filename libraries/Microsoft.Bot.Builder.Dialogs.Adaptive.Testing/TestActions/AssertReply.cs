// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Test Script action to assert that the bots' reply matches expectations.
    /// </summary>
    [DebuggerDisplay("AssertReply{Exact ? \"[Exact]\" : string.Empty}:{GetConditionDescription()}")]
    public class AssertReply : AssertReplyActivity
    {
        /// <summary>
        /// Kind for the json object.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.Test.AssertReply";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertReply"/> class.
        /// </summary>
        /// <param name="path">path.</param>
        /// <param name="line">line number.</param>
        [JsonConstructor]
        public AssertReply([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
            : base(path, line)
        {
        }

        /// <summary>
        /// Gets or sets the Text to assert.
        /// </summary>
        /// <value>The text value to look for in the reply.</value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether text should be an exact match.
        /// </summary>
        /// <value>if true, then exact match, if false, then it will be a Contains match.</value>
        [DefaultValue(true)]
        [JsonProperty("exact")]
        public bool Exact { get; set; } = true;

        /// <inheritdoc/>
        public override void ValidateReply(Activity activity)
        {
            // if we have a reply
            if (!string.IsNullOrEmpty(Text))
            {
                var description = Description != null ? Description + "\n" : string.Empty;
                var text = activity.AsMessageActivity()?.Text;
                var error = $"{description}Text '{text}' didn't match expected text: '{Text}'";
                if (text == null)
                {
                    throw new Exception(error);
                }
                else if (Exact)
                {
                    // Normalize line endings to work on windows and mac
                    if (text.Replace("\r", string.Empty) != Text.Replace("\r", string.Empty))
                    {
                        throw new Exception(error);
                    }
                }
                else
                {
                    if (text.ToLowerInvariant().Trim().Contains(Text.ToLowerInvariant().Trim()) == false)
                    {
                        throw new Exception(error);
                    }
                }
            }

            base.ValidateReply(activity);
        }

        /// <inheritdoc/>
        public override string GetConditionDescription()
        {
            return $"{Text}";
        }
    }
}

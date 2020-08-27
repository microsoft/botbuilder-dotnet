using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    /// <summary>
    /// Assertion that reply from the bot matches one of options.
    /// </summary>
    [DebuggerDisplay("AssertReplyOneOf:{GetConditionDescription()}")]
    public class AssertReplyOneOf : AssertReplyActivity
    {
        /// <summary>
        /// kind for serialization.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.Test.AssertReplyOneOf";

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertReplyOneOf"/> class.
        /// </summary>
        /// <param name="path">path to source.</param>
        /// <param name="line">line number in source.</param>
        [JsonConstructor]
        public AssertReplyOneOf([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
            : base(path, line)
        {
        }

        /// <summary>
        /// Gets the text variations.
        /// </summary>
        /// <value>
        /// The text variations.
        /// </value>
        [JsonProperty("text")]
        public List<string> Text { get; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether exact match policy should be used.
        /// </summary>
        /// <value>
        /// A bool value for match policy.
        /// </value>
        [DefaultValue(true)]
        [JsonProperty("exact")]
        public bool Exact { get; set; } = true;

        /// <inheritdoc/>
        public override void ValidateReply(Activity activity)
        {
            bool found = false;

            foreach (var reply in Text)
            {
                // if we have a reply
                if (this.Exact)
                {
                    if (activity.AsMessageActivity()?.Text == reply)
                    {
                        found = true;
                        break;
                    }
                }
                else
                {
                    if (activity.AsMessageActivity()?.Text.ToLowerInvariant().Trim().Contains(reply.ToLowerInvariant().Trim()) == true)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                throw new Exception(this.Description ?? $"Text {activity.Text} didn't match one of expected text: {string.Join("\n", this.Text)}");
            }

            base.ValidateReply(activity);
        }

        /// <inheritdoc/>
        public override string GetConditionDescription()
        {
            return string.Join("\n", this.Text);
        }
    }
}

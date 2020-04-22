using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    [DebuggerDisplay("AssertReplyOneOf:{GetConditionDescription()}")]
    public class AssertReplyOneOf : AssertReplyActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.Test.AssertReplyOneOf";

        [JsonConstructor]
        public AssertReplyOneOf([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
            : base(path, line)
        {
        }

        /// <summary>
        /// Gets or sets the text variations.
        /// </summary>
        /// <value>
        /// The text variations.
        /// </value>
        [JsonProperty("text")]
        public List<string> Text { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether exact match policy should be used.
        /// </summary>
        /// <value>
        /// A bool value for match policy.
        /// </value>
        [DefaultValue(true)]
        [JsonProperty("exact")]
        public bool Exact { get; set; } = true;

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
                    if (activity.AsMessageActivity()?.Text.ToLower().Trim().Contains(reply.ToLower().Trim()) == true)
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

        public override string GetConditionDescription()
        {
            return string.Join("\n", this.Text);
        }
    }
}

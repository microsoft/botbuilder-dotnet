using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions
{
    [DebuggerDisplay("AssertReply{Exact ? \"[Exact]\" : string.Empty}:{GetConditionDescription()}")]
    public class AssertReply : AssertReplyActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.Test.AssertReply";

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

        public override void ValidateReply(Activity activity)
        {
            // if we have a reply
            if (!string.IsNullOrEmpty(this.Text))
            {
                if (this.Exact)
                {
                    if (activity.AsMessageActivity()?.Text != this.Text)
                    {
                        throw new Exception(this.Description ?? $"Text '{activity.Text}' didn't match expected text: {this.Text}'");
                    }
                }
                else
                {
                    if (activity.AsMessageActivity()?.Text.ToLower().Trim().Contains(this.Text.ToLower().Trim()) == false)
                    {
                        throw new Exception(this.Description ?? $"Text '{activity.Text}' didn't match expected text: '{this.Text}'");
                    }
                }
            }

            base.ValidateReply(activity);
        }

        public override string GetConditionDescription()
        {
            return $"{Text}";
        }
    }
}

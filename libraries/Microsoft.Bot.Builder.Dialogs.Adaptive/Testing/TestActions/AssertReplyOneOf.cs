using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions
{
    public class AssertReplyOneOf : AssertReplyActivity
    {
        public List<string> Text { get; set; } = new List<string>();

        public bool Exact { get; set; } = false;

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

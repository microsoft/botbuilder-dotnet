using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Mocks
{
    /// <summary>
    /// MockBotFrameworkClient mock.
    /// </summary>
    public class MockBotFrameworkClient : BotFrameworkClient
    {
        /// <inheritdoc/>
        public override Task<InvokeResponse<T>> PostActivityAsync<T>(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            var responseActivity = activity;

            if (activity.Text == "skill")
            {
                responseActivity = new Activity()
                {
                    Type = "message",
                    Text = "This is the skill talking: hello"
                };
            }

            var response = new InvokeResponse<ExpectedReplies>()
            {
                Status = 200,
                Body = new ExpectedReplies
                {
                    Activities = new List<Activity>()
                    {
                        responseActivity
                    }
                }
            };

            var casted = (InvokeResponse<T>)Convert.ChangeType(response, typeof(InvokeResponse<T>), new System.Globalization.CultureInfo("en-US"));
            var result = Task.FromResult(casted);
            return result;
        }

        /// <inheritdoc/>
        /// <remarks>Not implemented.</remarks>
        public override Task<InvokeResponse> PostActivityAsync(string fromBotId, string toBotId, Uri toUrl, Uri serviceUrl, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Slack.TestBot
{
    public class SimpleSlackAdapterOptions : SlackAdapterOptions
    {
        public SimpleSlackAdapterOptions()
        {
        }

        public SimpleSlackAdapterOptions(string verificationToken, string botToken, string signingSecret)
        {
            this.VerificationToken = verificationToken;
            this.BotToken = botToken;
            this.ClientSigningSecret = signingSecret;
        }
    }
}

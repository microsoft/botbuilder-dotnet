using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Adapters.Slack.TestBot
{
    public class SimpleSlackAdapterOptions : ISlackAdapterOptions
    {
        public SimpleSlackAdapterOptions()
        {
        }

        public SimpleSlackAdapterOptions(string verificationToken, string botToken)
        {
            this.VerificationToken = verificationToken;
            this.BotToken = botToken;
        }

        public string VerificationToken { get; set; }

        public string ClientSigningSecret { get; set; }

        public string BotToken { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string[] Scopes { get; set; }

        public string RedirectUri { get; set; }

        public Task<string> GetBotUserByTeam(string teamId)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetTokenForTeam(string teamId)
        {
            throw new System.NotImplementedException();
        }
    }
}

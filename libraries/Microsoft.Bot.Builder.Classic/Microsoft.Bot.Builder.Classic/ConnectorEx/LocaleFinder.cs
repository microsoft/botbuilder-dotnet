using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Base;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.ConnectorEx
{

    /// <summary>
    /// The interface for finding and setting locale for <see cref="LocalizedScope"/> in <see cref="SetAmbientThreadCulture"/>.
    /// </summary>
    public interface ILocaleFinder
    {
        /// <summary>
        /// Given an activity it finds the locale.
        /// </summary>
        /// <param name="activity"> The activity.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        Task<string> FindLocale(IActivity activity, CancellationToken token);
    }

    /// <summary>
    /// The locale finder implementation based on <see cref="ResumptionContext"/>.
    /// </summary>
    public sealed class LocaleFinder : ILocaleFinder
    {
        private readonly ResumptionContext resumptionContext;
        private readonly ConversationReference conversationReference;
        private string locale;

        public LocaleFinder(ConversationReference conversationReference, ResumptionContext resumptionContext)
        {
            SetField.NotNull(out this.conversationReference, nameof(conversationReference), conversationReference);
            SetField.NotNull(out this.resumptionContext, nameof(resumptionContext), resumptionContext);
        }

        public async Task<string> FindLocale(IActivity activity, CancellationToken token)
        {
            if (string.IsNullOrEmpty(this.locale))
            {
                var resumptionData = await this.resumptionContext.LoadDataAsync(token);

                if (resumptionData != null && resumptionData.IsTrustedServiceUrl)
                {
                    MicrosoftAppCredentials.TrustServiceUrl(this.conversationReference.ServiceUrl);
                }

                this.locale = (activity as IMessageActivity)?.Locale;

                // if locale is null or whitespace in the incoming request,
                // try to set it from the ResumptionContext
                if (string.IsNullOrWhiteSpace(this.locale))
                {
                    this.locale = resumptionData?.Locale;
                }

                // persist resumptionData with updated information
                var data = new ResumptionData
                {
                    Locale = this.locale,
                    IsTrustedServiceUrl = MicrosoftAppCredentials.IsTrustedServiceUrl(this.conversationReference.ServiceUrl)
                };
                await this.resumptionContext.SaveDataAsync(data, token);
            }
            return this.locale;
        }
    }
}

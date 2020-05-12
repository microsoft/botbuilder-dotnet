using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.IdentityModel.Logging;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    public class EntityRecognizer
    {
        public EntityRecognizer()
        {
        }

        public virtual Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            return this.RecognizeEntitiesAsync(dialogContext, dialogContext.Context.Activity, entities, cancellationToken);
        }

        public virtual async Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, Activity activity, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                return await this.RecognizeEntitiesAsync(dialogContext, activity.Text, activity.Locale, entities, cancellationToken).ConfigureAwait(false);
            }

            return new List<Entity>();
        }

        public virtual Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Entity>>(Array.Empty<Entity>());
        }
    }
}

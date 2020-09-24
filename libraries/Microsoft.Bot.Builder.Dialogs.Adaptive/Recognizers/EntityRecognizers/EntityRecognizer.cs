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
    /// <summary>
    /// Entity recognizers base class.
    /// </summary>
    public class EntityRecognizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRecognizer"/> class.
        /// </summary>
        public EntityRecognizer()
        {
        }

        /// <summary>
        /// Recognizes entities from an <see cref="Entity"/> list.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="entities">The enumerated <see cref="Entity"/> to be recognized.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> from the task.</param>
        /// <returns>Recognized <see cref="Entity"/> list.</returns>
        public virtual Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            return this.RecognizeEntitiesAsync(dialogContext, dialogContext.Context.Activity, entities, cancellationToken);
        }

        /// <summary>
        /// Recognizes entities from an <see cref="Entity"/> list.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="activity">The dialog's <see cref="Activity"/>.</param>
        /// <param name="entities">The enumerated <see cref="Entity"/> to be recognized.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> from the task.</param>
        /// <returns>Recognized <see cref="Entity"/> list.</returns>
        public virtual async Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, Activity activity, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                return await this.RecognizeEntitiesAsync(dialogContext, activity.Text, activity.Locale, entities, cancellationToken).ConfigureAwait(false);
            }

            return new List<Entity>();
        }

        /// <summary>
        /// Recognizes entities from an <see cref="Entity"/> list.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="text">Text to recognize.</param>
        /// <param name="locale">Locale to use.</param>
        /// <param name="entities">The enumerated <see cref="Entity"/> to be recognized.</param>
        /// <param name="cancellationToken">Optional, the <see cref="CancellationToken"/> from the task.</param>
        /// <returns>Recognized <see cref="Entity"/> list.</returns>
        public virtual Task<IEnumerable<Entity>> RecognizeEntitiesAsync(DialogContext dialogContext, string text, string locale, IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Entity>>(Array.Empty<Entity>());
        }
    }
}

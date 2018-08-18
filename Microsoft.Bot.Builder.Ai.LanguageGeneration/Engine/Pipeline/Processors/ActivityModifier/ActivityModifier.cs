using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ActivityModifier : IActivityModifier
    {
        private IList<IActivityComponentModifier> _componentModifiers;

        public ActivityModifier()
        {
            InitializeComponentModifiers();
        }

        public async Task ModifyActivityAsync(Activity activity, ICompositeResponse response)
        {
            foreach (var componentModifier in _componentModifiers)
            {
                await componentModifier.ModifyAsync(activity, response).ConfigureAwait(false);
            }
        }

        private void InitializeComponentModifiers()
        {
            _componentModifiers = new List<IActivityComponentModifier>
            {
                new ActivityTextModifier(),
                new ActivitySpeechModifier(),
                new ActivitySuggestedActionsModifier(),
                new ActivityAttachmentsModifier()
            };
        }
    }
}

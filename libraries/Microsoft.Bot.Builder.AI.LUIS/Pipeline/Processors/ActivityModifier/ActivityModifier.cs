using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    internal class ActivityModifier : IActivityModifier
    {
        private IList<IActivityComponentModifier> _componentModifiers;

        public ActivityModifier()
        {
            InitializeComponentModifiers();
        }

        public void ModifyActivity(Activity activity, ICompositeResponse response)
        {
            foreach (var componentModifier in _componentModifiers)
            {
                componentModifier.Modify(activity, response);
            }
        }

        private void InitializeComponentModifiers()
        {
            _componentModifiers = new List<IActivityComponentModifier>
            {
                new ActivityTextModifier(),
                new ActivitySpeechModifier(),
                new ActivitySuggestedActionsModifier(),
                //new ActivityAttachmentsModifier()
            };
        }
    }
}

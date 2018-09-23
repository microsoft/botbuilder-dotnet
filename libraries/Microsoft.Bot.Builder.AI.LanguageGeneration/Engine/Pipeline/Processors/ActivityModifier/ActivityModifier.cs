using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The main <see cref="IActivityModifier"/> that will call all configured <see cref="IActivityComponentModifier"/> objects to modify the <see cref="Activity"/> object from all prespectives.
    /// </summary>
    internal class ActivityModifier : IActivityModifier
    {
        private IList<IActivityComponentModifier> _componentModifiers;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActivityModifier()
        {
            InitializeComponentModifiers();
        }

        /// <summary>
        /// Modifies/substitutes a <see cref="Activity"/> for template references by looping for each configured <see cref="IActivityComponentModifier"/> and calling it's modify function.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> object that will be modified ie : it's template refereces will be substituted with their resolved values.</param>
        /// <param name="response">The <see cref="ICompositeResponse"/> object that carries the tempolate resolution values, which will be used to modify the activity.</param>
        public void ModifyActivity(Activity activity, ICompositeResponse response)
        {
            foreach (var componentModifier in _componentModifiers)
            {
                componentModifier.Modify(activity, response);
            }
        }

        private void InitializeComponentModifiers() => _componentModifiers = new List<IActivityComponentModifier>
            {
                new ActivityTextModifier(),
                new ActivitySpeechModifier(),
                new ActivitySuggestedActionsModifier(),
                new ActivityAttachmentsModifier()
            };
    }
}

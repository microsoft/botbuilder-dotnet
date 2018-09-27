using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Engine
{
    /// <summary>
    /// The main <see cref="IActivityInspector"/> that will call all configured <see cref="IActivityComponentInspector"/> objects to search the <see cref="Activity"/> object from all prespectives.
    /// </summary>
    internal class ActivityInspector : IActivityInspector
    {
        private IList<IActivityComponentInspector> _componentInspectors;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActivityInspector()
        {
            InitializeComponentBuilders();
        }

        /// <summary>
        /// Inspects/Searches a <see cref="Activity"/> for template references by looping for each configured <see cref="IActivityComponentInspector"/> and calling it's inspect function.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>a <see cref="IList{string}"/> containing recognized template references.</returns>
        public IList<string> Inspect(Activity activity)
        {
            var foundPatterns = new HashSet<string>();
            foreach (var componentInspector in _componentInspectors)
            {
                var inspectionResult = componentInspector.Inspect(activity);
                foundPatterns.UnionWith(inspectionResult);
            }
            return new List<string>(foundPatterns);
        }

        private void InitializeComponentBuilders()
        {
            _componentInspectors = new List<IActivityComponentInspector>
            {
                new ActivityTextInspector(),
                new ActivitySpeechInspector(),
                new ActivitySuggestedActionsInspector(),
                new ActivityAttachmentInspector()
            };
        }
    }
}

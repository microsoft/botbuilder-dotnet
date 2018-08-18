using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ActivityInspector : IActivityInspector
    {
        private IList<IActivityComponentInspector> _componentInspectors;

        public ActivityInspector()
        {
            InitializeComponentBuilders();
        }

        public async Task<IList<string>> InspectAsync(Activity activity)
        {
            var foundPatterns = new HashSet<string>();
            foreach (var componentInspector in _componentInspectors)
            {
                var inspectionResult = await componentInspector.InspectAsync(activity).ConfigureAwait(false);
                foundPatterns.UnionWith(inspectionResult);
            }
            return new List<string>(foundPatterns);
        }
        
        private void InitializeComponentBuilders()
        {
            _componentInspectors = new List<IActivityComponentInspector>();
            _componentInspectors.Add(new ActivityTextInspector());
            _componentInspectors.Add(new ActivitySpeechInspector());
            _componentInspectors.Add(new ActivitySuggestedActionsInspector());
            _componentInspectors.Add(new ActivityAttachmentInspector());
        }
    }
}

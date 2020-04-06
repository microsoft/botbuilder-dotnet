using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public class DeclarativeComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield break;
        }

        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<SourceRange> context)
        {
            yield return new InterfaceConverter<IStorage>(resourceExplorer, context);
            yield return new InterfaceConverter<IRecognizer>(resourceExplorer, context);
            yield return new InterfaceConverter<Dialog>(resourceExplorer, context);
            yield return new InterfaceConverter<Recognizer>(resourceExplorer, context);
            yield return new ActivityConverter();
        }
    }
}

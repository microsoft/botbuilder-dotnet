using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    public class DeclarativeComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes()
        {
            yield break;
        }

        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths)
        {
            yield return new InterfaceConverter<IStorage>(resourceExplorer, paths);
            yield return new InterfaceConverter<IRecognizer>(resourceExplorer, paths);
            yield return new InterfaceConverter<Dialog>(resourceExplorer, paths);
            yield return new InterfaceConverter<Recognizer>(resourceExplorer, paths);
            yield return new ActivityConverter();
        }
    }
}

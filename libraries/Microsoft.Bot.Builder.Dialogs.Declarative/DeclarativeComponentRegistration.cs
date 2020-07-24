using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Registers declarative kinds.
    /// </summary>
    public class DeclarativeComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Returns an enumeration of DeclarativeType objects.
        /// </summary>
        /// <param name="resourceExplorer">The resource explorer.</param>
        /// <returns>An empty enumerator.</returns>
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield break;
        }

        /// <summary>
        /// Returns an enumeration of JsonConverters for supporting loading declarative types.
        /// </summary>
        /// <param name="resourceExplorer">The ResourceExplorer.</param>
        /// <param name="sourceContext">The SourceContext to build a debugger source map.</param>
        /// <returns>A list of JSON converters.</returns>
        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new InterfaceConverter<IStorage>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<IRecognizer>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<Dialog>(resourceExplorer, sourceContext);
            yield return new InterfaceConverter<Recognizer>(resourceExplorer, sourceContext);
            yield return new ActivityConverter();
        }
    }
}

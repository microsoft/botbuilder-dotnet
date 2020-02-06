using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Interface for definition component types.
    /// </summary>
    public interface IComponentDeclarativeTypes 
    {
        IEnumerable<TypeRegistration> GetTypes();

        IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths);
    }
}

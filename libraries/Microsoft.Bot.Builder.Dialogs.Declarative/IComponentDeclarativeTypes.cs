// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Interface for registering declarative kinds and jsonconverters to support them.
    /// </summary>
    public interface IComponentDeclarativeTypes 
    {
        /// <summary>
        /// Return an enumeration of KindRegistrations $kind => Type.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer.</param>
        /// <returns>declarative type registration.</returns>
        IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer);

        /// <summary>
        /// Return an enumeration of JsonConverters for supporting loading declarative types.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        /// <returns>jsonsconverters.</returns>
        IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext);
    }
}

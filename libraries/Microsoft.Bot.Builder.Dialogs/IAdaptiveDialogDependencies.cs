// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Defines Dialog Dependencies interface for enumerating child dialogs should exposed for parent.
    /// </summary>
    public interface IAdaptiveDialogDependencies
    {
        /// <summary>
        /// Enumerate child dialog dependencies required to be exposed for parent containers dialogset.
        /// </summary>
        /// <returns>dialog enumeration.</returns>
        IEnumerable<Dialog> GetExternalDependencies();
    }
}

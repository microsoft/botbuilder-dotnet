// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Localization
{
    /// <summary>
    /// Base abstraction for locale resolution in the dialog system.
    /// </summary>
    internal abstract class LocaleResolver
    {
        /// <summary>
        /// Given the current <see cref="DialogContext"/>, resolves the effective locale that the 
        /// dialog should use.
        /// </summary>
        /// <param name="dc"><see cref="DialogContext"/> for which the locale is being resolved.</param>
        /// <returns>The resolved <see cref="CultureInfo"/> that the dialog should use.</returns>
        public abstract CultureInfo Resolve(DialogContext dc);
    }
}

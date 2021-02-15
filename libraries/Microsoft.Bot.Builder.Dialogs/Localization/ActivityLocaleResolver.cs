// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Localization
{
    /// <summary>
    /// Simple <see cref="LocaleResolver"/> that uses the current <see cref="Activity"/> locale, 
    /// and if no locale is found in the activity, falls back to the default locale.
    /// </summary>
    internal class ActivityLocaleResolver : LocaleResolver
    {
        /// <inheritdoc/>
        public override CultureInfo Resolve(DialogContext dc)
        {
            try
            {
                if (!string.IsNullOrEmpty(dc.Context.Activity?.Locale))
                {
                    return new CultureInfo(dc.Context.Activity?.Locale);
                }
                else
                {
                    return Thread.CurrentThread.CurrentCulture;
                }
            }
            catch (CultureNotFoundException)
            {
                // If the activity didn't have locale information, stay the course to whatever was set as default locale
                return Thread.CurrentThread.CurrentCulture;
            }
        }
    }
}

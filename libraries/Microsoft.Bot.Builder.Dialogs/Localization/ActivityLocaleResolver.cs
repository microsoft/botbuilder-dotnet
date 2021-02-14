using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Localization
{
    /// <summary>
    /// fds.
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

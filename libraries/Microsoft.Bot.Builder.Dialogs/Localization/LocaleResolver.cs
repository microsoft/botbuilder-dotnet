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
    internal abstract class LocaleResolver
    {
        /// <summary>
        /// sfd.
        /// </summary>
        /// <param name="dc">fs.</param>
        /// <returns>fds.</returns>
        public abstract CultureInfo Resolve(DialogContext dc);
    }
}

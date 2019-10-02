using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Constants used in TurnState.
    /// </summary>
    public static class TurnStateConstants
    {
        /// <summary>
        /// TurnState key for the OAuth login timeout.
        /// </summary>
        public const string OAuthLoginTimeoutKey = "LoginTimeout";

        /// <summary>
        /// Default amount of time an OAuthCard will remain active.
        /// </summary>
        public const int OAuthLoginTimeoutMsValue = 900000;

        /// <summary>
        /// Name of the token polling settings key.
        /// </summary>
        public const string TokenPollingSettingsKey = "tokenPollingSettings";
    }
}

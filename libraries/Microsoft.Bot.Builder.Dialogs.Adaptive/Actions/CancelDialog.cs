// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Command to cancel all of the current dialogs by emitting an event which must be caught to prevent cancelation from propagating.
    /// </summary>
    public class CancelDialog : CancelAllDialogsBase
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.CancelDialog";

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelDialog"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public CancelDialog([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(cancelAll: false)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }
    }
}

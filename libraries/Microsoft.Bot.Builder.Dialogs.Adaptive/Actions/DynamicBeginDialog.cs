// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Internal BeginDialog action which dynamically binds x.schema/x.dialog to invoke the x.dialog resource with properties as the options.
    /// </summary>
    public class DynamicBeginDialog : BeginDialog
    {
        [JsonConstructor]
        public DynamicBeginDialog([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the extra properties.
        /// </summary>
        /// <value>
        /// options if there is no explicit options set.
        /// </value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        protected Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
#pragma warning restore CA2227 // Collection properties should be read only

        protected override object BindOptions(DialogContext dc, object options)
        {
            // use overflow properties of deserialized object instead of the passed in option.
            return base.BindOptions(dc, JObject.FromObject(this.Properties));
        }
    }
}

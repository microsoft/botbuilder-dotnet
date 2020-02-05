﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions.Properties;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Format specifier for outputs.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
    public enum AttachmentOutputFormat
    {
        /// <summary>
        /// Pass iputs in a List.
        /// </summary>
        All,

        /// <summary>
        /// Pass input as a single element.
        /// </summary>
        First
    }

    public class AttachmentInput : InputDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.AttachmentInput";

        public AttachmentInput([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("outputFormat")]
        public EnumExpression<AttachmentOutputFormat> OutputFormat { get; set; } = AttachmentOutputFormat.First;

        protected override Task<InputState> OnRecognizeInput(DialogContext dc)
        {
            var dcState = dc.GetState();
            var input = dcState.GetValue<List<Attachment>>(VALUE_PROPERTY);
            var first = input.Count > 0 ? input[0] : null;

            if (first == null || (string.IsNullOrEmpty(first.ContentUrl) && first.Content == null))
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            switch (this.OutputFormat.GetValue(dcState))
            {
                case AttachmentOutputFormat.All:
                    dcState.SetValue(VALUE_PROPERTY, input);
                    break;
                case AttachmentOutputFormat.First:
                    dcState.SetValue(VALUE_PROPERTY, first);
                    break;
            }

            return Task.FromResult(InputState.Valid);
        }
    }
}

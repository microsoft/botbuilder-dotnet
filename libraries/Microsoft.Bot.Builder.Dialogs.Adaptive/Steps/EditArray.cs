// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Lets you modify an array in memory.
    /// </summary>
    public class EditArray : DialogCommand
    {
        public enum ArrayChangeType
        {
            /// <summary>
            /// Push item onto the end of the array
            /// </summary>
            Push,

            /// <summary>
            /// Pop the item off the end of the array
            /// </summary>
            Pop,

            /// <summary>
            /// Take an item from the front of the array
            /// </summary>
            Take,

            /// <summary>
            /// Remove the item from the array, regardless of it's location
            /// </summary>
            Remove,

            /// <summary>
            /// Clear the contents of the array
            /// </summary>
            Clear
        }

        private Expression value;
        private Expression arrayProperty;
        private Expression resultProperty;

        [JsonConstructor]
        public EditArray([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        protected override string OnComputeId()
        {
            return $"array[{ChangeType + ": " + ArrayProperty}]";
        }

        /// <summary>
        /// Gets or sets type of change being applied
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("changeType")]
        public ArrayChangeType ChangeType { get; set; }

        /// <summary>
        /// Gets or sets memory expression of the array to manipulate.
        /// </summary>Edit
        [JsonProperty("arrayProperty")]
        public string ArrayProperty
        {
            get { return arrayProperty?.ToString(); }
            set { this.arrayProperty = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets the result of the action
        /// </summary>
        [JsonProperty("resultProperty")]
        public string ResultProperty 
        {
            get { return resultProperty?.ToString(); }
            set { this.resultProperty = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets the expression of the item to put onto the array.
        /// </summary>
        [JsonProperty("value")]
        public string Value
        {
            get { return value?.ToString(); }
            set { this.value = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditArray"/> class.
        /// </summary>
        /// <param name="changeType">change type.</param>
        /// <param name="arrayProperty">array property (optional)</param>
        /// <param name="value">value to insert</param>
        /// <param name="resultProperty">output property to put Pop/Take into</param>
        public EditArray(ArrayChangeType changeType, string arrayProperty = null, string value = null, string resultProperty = null)
            : base()
        {
            this.ChangeType = changeType;

            if (!string.IsNullOrEmpty(arrayProperty))
            {
                this.ArrayProperty = arrayProperty;
            }

            switch (changeType)
            {
                case ArrayChangeType.Clear:
                case ArrayChangeType.Pop:
                case ArrayChangeType.Take:
                    this.ResultProperty = resultProperty;
                    break;
                case ArrayChangeType.Push:
                case ArrayChangeType.Remove:
                    this.Value = value;
                    break;
            }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (string.IsNullOrEmpty(ArrayProperty))
            {
                throw new Exception($"EditArray: \"{ ChangeType }\" operation couldn't be performed because the arrayProperty wasn't specified.");
            }

            var array = dc.State.GetValue<JArray>(this.arrayProperty, new JArray());

            object item = null;
            object result = null;

            switch (ChangeType)
            {
                case ArrayChangeType.Pop:
                    item = array[array.Count - 1];
                    array.RemoveAt(array.Count - 1);
                    result = item;
                    break;
                case ArrayChangeType.Push:
                    EnsureValue();
                    var (itemResult, error) = this.value.TryEvaluate(dc.State);
                    if (error == null && itemResult != null)
                    {
                        array.Add(itemResult);
                    }

                    break;
                case ArrayChangeType.Take:
                    if (array.Count == 0)
                    {
                        break;
                    }

                    item = array[0];
                    array.RemoveAt(0);
                    result = item;
                    break;
                case ArrayChangeType.Remove:
                    EnsureValue();
                    (itemResult, error) = this.value.TryEvaluate(dc.State);
                    if (error == null && itemResult != null)
                    {
                        result = false;
                        for (var i = 0; i < array.Count(); ++i)
                        {
                            if (array[i].ToString() == itemResult.ToString() || JToken.DeepEquals(array[i], JToken.FromObject(itemResult)))
                            {
                                result = true;
                                array.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    break;
                case ArrayChangeType.Clear:
                    result = array.Count > 0;
                    array.Clear();
                    break;
            }

            dc.State.SetValue(this.arrayProperty, array);

            if (ResultProperty != null)
            {
                dc.State.SetValue(resultProperty, result);
            }

            return await dc.EndDialogAsync(result);
        }

        private void EnsureValue()
        {
            if (Value == null)
            {
                throw new Exception($"EditArray: \"{ChangeType}\" operation couldn't be performed for array \"{ArrayProperty}\" because a value wasn't specified.");
            }
        }

    }
}

// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Executes a set of actions once for each item in an in-memory list or collection.
    /// </summary>
    public class Foreach : Dialog, IDialogDependencies
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Foreach";

        private const string INDEX = "dialog.foreach.index";
        private const string VALUE = "dialog.foreach.value";

        [JsonConstructor]
        public Foreach([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets property path expression to the collection of items.
        /// </summary>
        /// <value>
        /// Property path expression to the collection of items.
        /// </value>
        [JsonProperty("itemsProperty")]
        public string ItemsProperty { get; set; }

        /// <summary>
        /// Gets or sets the actions to be run for each of items.
        /// </summary>
        /// <value>
        /// The actions to be run for each of items.
        /// </value>
        [JsonProperty("actions")]
        public List<Dialog> Actions { get; set; } = new List<Dialog>();

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            return this.Actions;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // Ensure planning context
            if (dc is SequenceContext sc)
            {
                Expression itemsProperty = null;
                int offset = 0;
                if (options != null && options is ForeachOptions)
                {
                    var opt = options as ForeachOptions;
                    if (!string.IsNullOrEmpty(opt.List))
                    {
                        itemsProperty = new ExpressionEngine().Parse(opt.List);
                    }

                    offset = opt.Offset;
                }

                itemsProperty = new ExpressionEngine().Parse(this.ItemsProperty);
                var (itemList, error) = itemsProperty.TryEvaluate(dc.GetState());

                if (error == null)
                {
                    var item = this.GetItem(itemList, offset);
                    if (item != null)
                    {
                        dc.GetState().SetValue(VALUE, item);
                        dc.GetState().SetValue(INDEX, offset);
                        var changes = new ActionChangeList()
                        {
                            ChangeType = ActionChangeType.InsertActions,
                            Actions = new List<ActionState>()
                        };
                        this.Actions.ForEach(step => changes.Actions.Add(new ActionState(step.Id)));

                        changes.Actions.Add(new ActionState()
                        {
                            DialogStack = new List<DialogInstance>(),
                            DialogId = this.Id,
                            Options = new ForeachOptions()
                            {
                                List = ItemsProperty,
                                Offset = offset + 1
                            }
                        });
                        sc.QueueChanges(changes);
                    }
                }

                return await sc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`Foreach` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.ItemsProperty})";
        }

        private object GetItem(object list, int index)
        {
            JToken result = null;
            if (list != null && list.GetType() == typeof(JArray))
            {
                if (index < JArray.FromObject(list).Count)
                {
                    result = JArray.FromObject(list)[index];
                }
            }
            else if (list != null && list is JObject)
            {
                result = ((JObject)list).SelectToken(index.ToString());
            }

            return result;
        }

        public class ForeachOptions
        {
            public string List { get; set; }

            public int Offset { get; set; }
        }
    }
}

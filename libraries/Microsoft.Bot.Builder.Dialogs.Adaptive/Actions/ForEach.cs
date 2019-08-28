// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Executes a set of actions once for each item in an in-memory list or collection.
    /// </summary>
    public class Foreach : DialogAction, IDialogDependencies
    {
        private Expression listProperty;

        [JsonConstructor]
        public Foreach([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        // Expression used to compute the list that should be enumerated.
        [JsonProperty("listProperty")]
        public string ListProperty
        {
            get { return listProperty?.ToString(); }
            set { this.listProperty = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        // In-memory property that will contain the current items index. Defaults to `dialog.index`.
        [JsonProperty("indexProperty")]
        public string IndexProperty { get; set; } = "dialog.index";

        // In-memory property that will contain the current items value. Defaults to `dialog.value`.
        [JsonProperty("valueProperty")]
        public string ValueProperty { get; set; } = DialogContextState.DIALOG_VALUE;

        // Actions to be run for each of items.
        [JsonProperty("actions")]
        public List<IDialog> Actions { get; set; } = new List<IDialog>();

        public override List<IDialog> ListDependencies()
        {
            return this.Actions;
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            // Ensure planning context
            if (dc is SequenceContext sc)
            {
                Expression listProperty = null;
                int offset = 0;
                if (options != null && options is ForeachOptions)
                {
                    var opt = options as ForeachOptions;
                    listProperty = opt.List;
                    offset = opt.Offset;
                }

                if (listProperty == null)
                {
                    listProperty = new ExpressionEngine().Parse(this.ListProperty);
                }

                var (itemList, error) = listProperty.TryEvaluate(dc.State);

                if (error == null)
                {
                    var item = this.GetItem(itemList, offset);
                    if (item != null)
                    {
                        dc.State.SetValue(this.ValueProperty, item);
                        dc.State.SetValue(this.IndexProperty, offset);
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
                                List = listProperty,
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
            return $"{nameof(Foreach)}({this.ListProperty})";
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
            public Expression List { get; set; }

            public int Offset { get; set; }
        }
    }
}

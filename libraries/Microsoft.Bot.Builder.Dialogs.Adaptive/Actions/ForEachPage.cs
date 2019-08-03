// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Executes a set of actions once for each item in an in-memory list or collection.
    /// </summary>
    public class ForeachPage : DialogAction, IDialogDependencies
    {
        private Expression listProperty;

        // Expression used to compute the list that should be enumerated.
        [JsonProperty("listProperty")]
        public string ListProperty
        {
            get { return listProperty?.ToString(); }
            set { this.listProperty = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 10;

        // In-memory property that will contain the current items value. Defaults to `dialog.value`.
        [JsonProperty("valueProperty")]
        public string ValueProperty { get; set; } = DialogContextState.DIALOG_VALUE;

        // Actions to be run for each of items.
        [JsonProperty("actions")]
        public List<IDialog> Actions { get; set; } = new List<IDialog>();

        [JsonConstructor]
        public ForeachPage([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
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
                int pageSize = 0;
                if (options != null && options is ForeachPageOptions)
                {
                    var opt = options as ForeachPageOptions;
                    listProperty = opt.list;
                    offset = opt.offset;
                    pageSize = opt.pageSize;
                }

                if (pageSize == 0)
                {
                    pageSize = this.PageSize;
                }

                if (listProperty == null)
                {
                    listProperty = new ExpressionEngine().Parse(this.ListProperty);
                }

                var (itemList, error) = listProperty.TryEvaluate(dc.State);
                if (error == null)
                {
                    var page = this.GetPage(itemList, offset, pageSize);

                    if (page.Count() > 0)
                    {
                        dc.State.SetValue(this.ValueProperty, page);
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
                            Options = new ForeachPageOptions()
                            {
                                list = listProperty,
                                offset = offset + pageSize,
                                pageSize = pageSize
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

        private List<object> GetPage(object list, int index, int pageSize)
        {
            List<object> page = new List<object>();
            int end = index + pageSize;
            if (list != null && list.GetType() == typeof(JArray))
            {
                for (int i = index;  i < end && i < JArray.FromObject(list).Count; i++)
                {
                    page.Add(JArray.FromObject(list)[i]);
                }
            }
            else if (list != null && list is JObject)
            {
                for (int i = index; i < end; i++)
                {
                    if (((JObject)list).SelectToken(i.ToString()).HasValues)
                    {
                        page.Add(((JObject)list).SelectToken(i.ToString()));
                    }
                }
            }
            return page;
        }
        protected override string OnComputeId()
        {
            return $"{nameof(Foreach)}({this.ListProperty})";
        }

        public override List<IDialog> ListDependencies()
        {
            return this.Actions;
        }

        public class ForeachPageOptions
        {
            public Expression list { get; set; }
            public int offset { get; set; }
            public int pageSize { get; set; }
        }
    }
}

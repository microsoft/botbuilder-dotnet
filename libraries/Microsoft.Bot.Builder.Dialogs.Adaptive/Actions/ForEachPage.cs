// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ForeachPage : Dialog, IDialogDependencies
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.ForeachPage";

        private const string ForEachPage = "dialog.foreach.page";

        [JsonConstructor]
        public ForeachPage([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        // Expression used to compute the list that should be enumerated.
        [JsonProperty("itemsProperty")]
        public string ItemsProperty { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 10;

        // Actions to be run for each of items.
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
                Expression itemsProperty = new ExpressionEngine().Parse(this.ItemsProperty);
                int offset = 0;
                int pageSize = 0;
                if (options != null && options is ForeachPageOptions)
                {
                    var opt = options as ForeachPageOptions;
                    itemsProperty = opt.Items;
                    offset = opt.Offset;
                    pageSize = opt.PageSize;
                }

                if (pageSize == 0)
                {
                    pageSize = this.PageSize;
                }

                var (items, error) = itemsProperty.TryEvaluate(dc.GetState());
                if (error == null)
                {
                    var page = this.GetPage(items, offset, pageSize);

                    if (page.Count() > 0)
                    {
                        dc.GetState().SetValue(ForEachPage, page);
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
                                Items = itemsProperty,
                                Offset = offset + pageSize,
                                PageSize = pageSize
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

        private List<object> GetPage(object list, int index, int pageSize)
        {
            List<object> page = new List<object>();
            int end = index + pageSize;
            if (list != null && list.GetType() == typeof(JArray))
            {
                for (int i = index; i < end && i < JArray.FromObject(list).Count; i++)
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

        public class ForeachPageOptions
        {
            public Expression Items { get; set; }

            public int Offset { get; set; }

            public int PageSize { get; set; }
        }
    }
}

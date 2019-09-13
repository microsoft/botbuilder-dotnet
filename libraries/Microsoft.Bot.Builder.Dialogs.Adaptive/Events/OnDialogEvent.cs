// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{
    /// <summary>
    /// Event triggered when a dialog event matching a list of event names is emitted.
    /// </summary>
    public class OnDialogEvent : OnEvent
    {
        [JsonConstructor]
        public OnDialogEvent(List<string> events = null, List<IDialog> actions = null, string constraint = null, int priority = 0, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
     : base(constraint: constraint, actions: actions, callerPath: callerPath, callerLine: callerLine)
        {
            this.Events = events ?? new List<string>();
            this.Actions = actions ?? new List<IDialog>();
            this.Priority = priority;
        }
        /// <summary>
        /// Gets or sets list of events to filter.
        /// </summary>
        /// <value>
        /// List of events to filter. (Example: [BeginDialog,Error]
        /// </value>
        public List<string> Events { get; set; }

        /// <summary>
        /// Gets or sets the relative priority of this event handler.
        /// </summary>
        /// <value>
        /// The relative priority of this event handler.
        /// </value>
        public int Priority { get; set; }
        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({string.Join(",", Events)})";
        }

        protected override ActionChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            var changeList = new ActionChangeList()
            {
                ChangeType = ActionChangeType.InsertActions,
                Actions = new List<ActionState>()
            };

            this.Actions.ForEach(s =>
            {
                var stepState = new ActionState()
                {
                    DialogId = s.Id,
                    DialogStack = new List<DialogInstance>()
                };

                if (dialogOptions != null)
                {
                    stepState.Options = dialogOptions;
                }

                changeList.Actions.Add(stepState);
            });

            return changeList;
        }

        protected override Expression BuildExpression(IExpressionParser factory)
        {
            List<Expression> expressions = new List<Expression>();

            foreach (var evt in Events)
            {
                expressions.Add(factory.Parse($"turn.dialogEvent.name == '{evt}'"));
            }

            return expressions.Any()
                ? Expression.AndExpression(
                    Expression.OrExpression(expressions.ToArray()),
                    base.BuildExpression(factory))
                : base.BuildExpression(factory);
        }
    }
}

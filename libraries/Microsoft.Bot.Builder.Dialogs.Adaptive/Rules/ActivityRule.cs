// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{

    /// <summary>
    /// Rule triggered when a Activity of a given type is received 
    /// </summary>
    public class ActivityRule : EventRule
    {
        [JsonConstructor]
        public ActivityRule(string type=null, List<IDialog> steps = null, string constraint = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(events: new List<string>()
            {
                AdaptiveEvents.ActivityReceived
            },
            steps: steps,
            constraint: constraint,
            callerPath: callerPath, callerLine: callerLine)
        {
            Type = type;
        }

        /// <summary>
        /// ActivityType
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        protected override Expression BuildExpression(IExpressionParser factory)
        {

            // add constraints for activity type
            return Expression.AndExpression(factory.Parse($"turn.dialogEvent.value.type == '{this.Type}'"), 
                base.BuildExpression(factory));
        }

        protected override StepChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            return new StepChangeList()
            {
                Steps = Steps.Select(s => new StepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = dialogOptions
                }).ToList()
            };
        }

        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({this.Type})[{this.Constraint}]";
        }
    }

    /// <summary>
    /// Rule for ConversationUpdate Activity
    /// </summary>
    public class ConversationUpdateActivityRule : ActivityRule
    {
        [JsonConstructor]
        public ConversationUpdateActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.ConversationUpdate, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

    /// <summary>
    /// Rule for EndOfConversation Activity
    /// </summary>
    public class EndOfConversationActivityRule : ActivityRule
    {
        [JsonConstructor]
        public EndOfConversationActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.EndOfConversation, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }


    /// <summary>
    /// Rule for Event Activity
    /// </summary>
    public class EventActivityRule : ActivityRule
    {
        [JsonConstructor]
        public EventActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.Event, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

    /// <summary>
    /// Rule for Handoff Activity
    /// </summary>
    public class HandoffActivityRule: ActivityRule
    {
        [JsonConstructor]
        public HandoffActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.Handoff, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

    /// <summary>
    /// Rule for Invoke Activity
    /// </summary>
    public class InvokeActivityRule : ActivityRule
    {
        [JsonConstructor]
        public InvokeActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.Invoke, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

    /// <summary>
    /// Rule for Message Activity
    /// </summary>
    public class MessageActivityRule : ActivityRule
    {
        [JsonConstructor]
        public MessageActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.Message, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

    /// <summary>
    /// Rule for MessageReaction Activity
    /// </summary>
    public class MessageReactionActivityRule : ActivityRule
    {
        [JsonConstructor]
        public MessageReactionActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.MessageReaction, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }


    /// <summary>
    /// Rule for MessageUpdate Activity
    /// </summary>
    public class MessageUpdateActivityRule : ActivityRule
    {
        [JsonConstructor]
        public MessageUpdateActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.MessageUpdate, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

    /// <summary>
    /// Rule for MessageDelete Activity
    /// </summary>
    public class MessageDeleteActivityRule : ActivityRule
    {
        [JsonConstructor]
        public MessageDeleteActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.MessageDelete, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

    /// <summary>
    /// Rule for TypingActivity Activity
    /// </summary>
    public class TypingActivityRule : ActivityRule
    {
        [JsonConstructor]
        public TypingActivityRule(List<IDialog> steps = null, string constraint= null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.Typing, steps: steps, constraint: constraint, callerPath: callerPath, callerLine: callerLine) { }
    }

}

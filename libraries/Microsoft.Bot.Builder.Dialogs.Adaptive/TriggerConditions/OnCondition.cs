// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Expressions.TriggerTrees;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when condition is true.
    /// </summary>
    [DebuggerDisplay("{GetIdentity()}")]
    public class OnCondition : IItemIdentity, IDialogDependencies
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.OnCondition";

        // constraints from Rule.AddConstraint()
        private List<Expression> extraConstraints = new List<Expression>();

        // cached expression representing all constraints (constraint AND extraConstraints AND childrenConstraints)
        private Expression fullConstraint = null;

        [JsonConstructor]
        public OnCondition(string condition = null, List<Dialog> actions = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Condition = condition;
            this.Actions = actions;
        }

        /// <summary>
        /// Gets or sets the condition which needs to be met for the actions to be executed (OPTIONAL).
        /// </summary>
        /// <value>
        /// The condition which needs to be met for the actions to be executed.
        /// </value>
        [JsonProperty("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Gets or sets the actions to add to the plan when the rule constraints are met.
        /// </summary>
        /// <value>
        /// The actions to add to the plan when the rule constraints are met.
        /// </value>
        [JsonProperty("actions")]
        public List<Dialog> Actions { get; set; } = new List<Dialog>();

        [JsonIgnore]
        public virtual SourceRange Source => DebugSupport.SourceMap.TryGetValue(this, out var range) ? range : null;

        /// <summary>
        /// Gets or sets the rule priority where 0 is the highest.
        /// </summary>
        /// <value>Priority of condition.</value>
        [JsonProperty("priority")]
        public int? Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether rule should only run once per unique set of memory paths.
        /// </summary>
        /// <value>Boolean if should run once per unique values.</value>
        [JsonProperty("runOnce")]
        public bool RunOnce { get; set; }

        /// <summary>
        /// Gets or sets the value of the Unique id for this condition.
        /// </summary>
        /// <value>Id for condition.</value>
        [JsonIgnore]
        public uint Id { get; set; }

        /// <summary>
        /// Get the expression for this rule by calling GatherConstraints().
        /// </summary>
        /// <param name="parser">Expression parser.</param>
        /// <returns>Expression which will be cached and used to evaluate this rule.</returns>
        public virtual Expression GetExpression(IExpressionParser parser)
        {
            lock (this.extraConstraints)
            {
                if (this.fullConstraint == null)
                {
                    var allExpressions = new List<Expression>();
                    if (!string.IsNullOrWhiteSpace(this.Condition))
                    {
                        try
                        {
                            allExpressions.Add(parser.Parse(this.Condition));
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Invalid constraint expression: {this.Condition}, {e.Message}");
                        }
                    }

                    if (this.extraConstraints.Any())
                    {
                        allExpressions.AddRange(this.extraConstraints);
                    }

                    if (allExpressions.Any())
                    {
                        this.fullConstraint = Expression.AndExpression(allExpressions.ToArray());
                    }
                    else
                    {
                        this.fullConstraint = Expression.ConstantExpression(true);
                    }

                    if (RunOnce)
                    {
                        this.fullConstraint = Expression.AndExpression(
                            this.fullConstraint,
                            new Expression(
                                TriggerTree.LookupFunction("ignore"),
                                new Expression(new ExpressionEvaluator(
                                    $"runOnce{Id}",
                                    (expression, os) =>
                                    {
                                        var state = os as DialogStateManager;
                                        var basePath = AdaptiveDialog.ConditionTracker + "." + Id.ToString() + ".";
                                        var lastRun = state.GetValue<uint>(basePath + "lastRun");
                                        var paths = state.GetValue<string[]>(basePath + "paths");
                                        var changed = state.AnyPathChanged(lastRun, paths);
                                        return (changed, null);
                                    },
                                    ReturnType.Boolean,
                                    BuiltInFunctions.ValidateUnary))));
                    }
                }
            }

            return this.fullConstraint;
        }

        /// <summary>
        /// Add external condition to the OnCondition (mostly used by external OnConditionSet to apply external constraints to OnCondition).
        /// </summary>
        /// <param name="condition">External constraint to add, it will be AND'ed to all other constraints.</param>
        public void AddExternalCondition(string condition)
        {
            if (!string.IsNullOrWhiteSpace(condition))
            {
                try
                {
                    lock (this.extraConstraints)
                    {
                        this.extraConstraints.Add(new ExpressionEngine().Parse(condition));
                        this.fullConstraint = null; // reset to force it to be recalcaulated
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Invalid constraint expression: {this.Condition}, {e.Message}");
                }
            }
        }

        /// <summary>
        /// Method called to execute the rule's actions.
        /// </summary>
        /// <param name="planningContext">Context.</param>
        /// <returns>A <see cref="Task"/> with plan change list.</returns>
        public virtual async Task<List<ActionChangeList>> ExecuteAsync(SequenceContext planningContext)
        {
            if (RunOnce)
            {
                var count = planningContext.GetState().GetValue<uint>(DialogPath.EventCounter);
                planningContext.GetState().SetValue(AdaptiveDialog.ConditionTracker + "." + Id.ToString() + ".lastRun", count);
            }

            return await Task.FromResult(new List<ActionChangeList>()
            {
                this.OnCreateChangeList(planningContext)
            });
        }

        /// <summary>
        /// Method called to execute the rule's actions.
        /// </summary>
        /// <returns>A <see cref="Task"/> with plan change list.</returns>
        public virtual string GetIdentity()
        {
            return $"{this.GetType().Name}()";
        }

        public virtual IEnumerable<Dialog> GetDependencies()
        {
            foreach (var action in this.Actions)
            {
                yield return action;
            }

            yield break;
        }

        protected virtual ActionChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            var changeList = new ActionChangeList()
            {
                Actions = new List<ActionState>()
            };

            Actions.ForEach(s =>
            {
                var stepState = new ActionState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id
                };

                if (dialogOptions != null)
                {
                    stepState.Options = dialogOptions;
                }

                changeList.Actions.Add(stepState);
            });

            return changeList;
        }

        protected void RegisterSourceLocation(string path, int lineNumber)
        {
            if (path != null)
            {
                DebugSupport.SourceMap.Add(this, new SourceRange()
                {
                    Path = path,
                    StartPoint = new SourcePoint() { LineIndex = lineNumber, CharIndex = 0 },
                    EndPoint = new SourcePoint() { LineIndex = lineNumber + 1, CharIndex = 0 },
                });
            }
        }
    }
}

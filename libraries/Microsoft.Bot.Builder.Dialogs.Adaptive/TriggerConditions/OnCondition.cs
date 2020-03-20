// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Memory;
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

        private ActionScope actionScope;

        // constraints from Rule.AddConstraint()
        private List<Expression> extraConstraints = new List<Expression>();

        // cached expression representing all constraints (constraint AND extraConstraints AND childrenConstraints)
        private Expression fullConstraint = null;

        [JsonConstructor]
        public OnCondition(string condition = null, List<Dialog> actions = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            if (condition != null)
            {
                this.Condition = condition;
            }

            this.Actions = actions;
        }

        /// <summary>
        /// Gets or sets the condition which needs to be met for the actions to be executed (OPTIONAL).
        /// </summary>
        /// <value>
        /// The condition which needs to be met for the actions to be executed.
        /// </value>
        [JsonProperty("condition")]
        public BoolExpression Condition { get; set; }

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
        /// Gets or sets the rule priority expression where 0 is the highest and less than 0 is ignored.
        /// </summary>
        /// <value>Priority of condition expression.</value>
        [JsonProperty("priority")]
        public IntExpression Priority { get; set; } = new IntExpression();

        /// <summary>
        /// Gets or sets a value indicating whether rule should only run once per unique set of memory paths.
        /// </summary>
        /// <value>Boolean if should run once per unique values.</value>
        [JsonProperty("runOnce")]
        public bool RunOnce { get; set; }

        /// <summary>
        /// Gets or sets the value of the unique id for this condition.
        /// </summary>
        /// <value>Id for condition.</value>
        [JsonIgnore]
        public string Id { get; set; }

        protected ActionScope ActionScope
        {
            get
            {
                if (actionScope == null)
                {
                    actionScope = new ActionScope() { Actions = this.Actions };
                }

                return actionScope;
            }
        }

        /// <summary>
        /// Get the expression for this rule by calling GatherConstraints().
        /// </summary>
        /// <returns>Expression which will be cached and used to evaluate this rule.</returns>
        public virtual Expression GetExpression()
        {
            lock (this.extraConstraints)
            {
                if (this.fullConstraint == null)
                {
                    var allExpressions = new List<Expression>();

                    if (this.Condition != null)
                    {
                        allExpressions.Add(this.Condition.ToExpression());
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
                                Expression.Lookup(ExpressionType.Ignore),
                                new Expression(new ExpressionEvaluator(
                                    $"runOnce{Id}",
                                    (expression, os) =>
                                    {
                                        var basePath = $"{AdaptiveDialog.ConditionTracker}.{Id}.";
                                        var changed = false;

                                        if (os.TryGetValue(basePath + "lastRun", out object val))
                                        {
                                            uint lastRun = ObjectPath.MapValueTo<uint>(val);

                                            if (os.TryGetValue(basePath + "paths", out val))
                                            {
                                                string[] paths = ObjectPath.MapValueTo<string[]>(val);
                                                if (paths != null)
                                                {
                                                    foreach (var path in paths)
                                                    {
                                                        if (os.TryGetValue($"dialog._tracker.paths.{path}", out val))
                                                        {
                                                            uint current = ObjectPath.MapValueTo<uint>(val);
                                                            if (current > lastRun)
                                                            {
                                                                changed = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return (changed, null);
                                    },
                                    ReturnType.Boolean,
                                    ExpressionFunctions.ValidateUnary))));
                    }
                }
            }

            return this.fullConstraint;
        }

        /// <summary>
        /// Compute the current value of the priority expression and return it.
        /// </summary>
        /// <param name="actionContext">Context to use for evaluation.</param>
        /// <returns>Computed priority.</returns>
        public int CurrentPriority(ActionContext actionContext)
        {
            var (priority, error) = this.Priority.TryGetValue(actionContext.State);
            if (error != null)
            {
                priority = -1;
            }

            return priority;
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
                        this.extraConstraints.Add(Expression.Parse(condition.TrimStart('=')));
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
        /// <param name="actionContext">Context.</param>
        /// <returns>A <see cref="Task"/> with plan change list.</returns>
        public virtual async Task<List<ActionChangeList>> ExecuteAsync(ActionContext actionContext)
        {
            if (RunOnce)
            {
                var count = actionContext.State.GetValue<uint>(DialogPath.EventCounter);
                actionContext.State.SetValue($"{AdaptiveDialog.ConditionTracker}.{Id}.lastRun", count);
            }

            return await Task.FromResult(new List<ActionChangeList>()
            {
                this.OnCreateChangeList(actionContext)
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
            yield return this.ActionScope;
        }

        protected virtual ActionChangeList OnCreateChangeList(ActionContext actionContext, object dialogOptions = null)
        {
            var changeList = new ActionChangeList()
            {
                Actions = new List<ActionState>()
                {
                    new ActionState()
                    {
                        DialogId = this.ActionScope.Id,
                        Options = dialogOptions
                    }
                },
            };
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

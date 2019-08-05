// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{
    /// <summary>
    /// Defines basic OnEvent handler
    /// </summary>
    [DebuggerDisplay("{GetIdentity()}")]
    public abstract class OnEvent : IOnEvent, IItemIdentity
    {
        // constraints from Rule.AddConstraint()
        private List<Expression> extraConstraints = new List<Expression>();

        // cached expression representing all constraints (constraint AND extraConstraints AND childrenConstraints)
        private Expression fullConstraint = null;

        [JsonConstructor]
        public OnEvent(string constraint = null, List<IDialog> actions = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);

            this.Constraint = constraint;
            this.Actions = actions;
        }

        /// <summary>
        /// Gets or sets the constraint to apply to the rule (OPTIONAL) 
        /// </summary>
        [JsonProperty("constraint")]
        public string Constraint { get; set; }

        /// <summary>
        /// Gets or sets the actions to add to the plan when the rule constraints are met
        /// </summary>
        [JsonProperty("actions")]
        public List<IDialog> Actions { get; set; } = new List<IDialog>();

        /// <summary>
        /// Get the expression for this rule by calling GatherConstraints()
        /// </summary>
        public Expression GetExpression(IExpressionParser parser)
        {
            lock (this.extraConstraints)
            {
                if (this.fullConstraint == null)
                {
                    this.fullConstraint = BuildExpression(parser);
                }
            }

            return this.fullConstraint;
        }

        /// <summary>
        /// Override this method to define the expression which is evaluated to determine if this rule should fire
        /// </summary>
        /// <returns>Expression which will be cached and used to evaluate this rule</returns>
        protected virtual Expression BuildExpression(IExpressionParser factory)
        {
            List<Expression> allExpressions = new List<Expression>();
            if (!string.IsNullOrWhiteSpace(this.Constraint))
            {
                try
                {
                    allExpressions.Add(factory.Parse(this.Constraint));
                }
                catch(Exception e)
                {
                    throw new Exception($"Invalid constraint expression: {this.Constraint}, {e.Message}");
                }
            }

            if (this.extraConstraints.Any())
            {
                allExpressions.AddRange(this.extraConstraints);
            }

            if (allExpressions.Any())
            {
                return Expression.AndExpression(allExpressions.ToArray());
            }
            else
            {
                return Expression.ConstantExpression(true);
            }
        }

        /// <summary>
        /// Add external constraint to the rule (mostly used by RuleSet to apply external constraints to rule)
        /// </summary>
        /// <param name="constraint"></param>
        public void AddConstraint(string constraint)
        {
            if (!string.IsNullOrWhiteSpace(constraint))
            {
                try
                {
                    lock (this.extraConstraints)
                    {
                        this.extraConstraints.Add(new ExpressionEngine().Parse(constraint));
                        this.fullConstraint = null; // reset to force it to be recalcaulated
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Invalid constraint expression: {this.Constraint}, {e.Message}");
                }
            }
                
        }

        /// <summary>
        /// Method called to execute the rule's actions
        /// </summary>
        /// <param name="planningContext"></param>
        /// <param name="dialogEvent"></param>
        /// <returns></returns>
        public async Task<List<ActionChangeList>> ExecuteAsync(SequenceContext planningContext)
        {
            return await OnExecuteAsync(planningContext).ConfigureAwait(false);
        }


        /// <summary>
        /// Method called to process the request to execute the actions
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dialogEvent"></param>
        /// <returns></returns>
        public async virtual Task<List<ActionChangeList>> OnExecuteAsync(SequenceContext planning)
        {
            return new List<ActionChangeList>()
            {
                this.OnCreateChangeList(planning)
            };
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
                DebugSupport.SourceRegistry.Add(this, new Source.Range()
                {
                    Path = path,
                    Start = new Source.Point() { LineIndex = lineNumber, CharIndex = 0 },
                    After = new Source.Point() { LineIndex = lineNumber + 1, CharIndex = 0 },
                });
            }
        }

        public virtual string GetIdentity()
        {
            return $"{this.GetType().Name}()";
        }
    }
}

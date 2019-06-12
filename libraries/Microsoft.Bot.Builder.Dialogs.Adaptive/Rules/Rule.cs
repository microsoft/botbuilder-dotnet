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

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{
    /// <summary>
    /// Defines basic Rule contract
    /// </summary>
    [DebuggerDisplay("{GetIdentity()}")]
    public abstract class Rule : IRule, IItemIdentity
    {
        private Expression constraint;

        private List<Expression> extraConstraints = new List<Expression>();

        [JsonConstructor]
        public Rule(string constraint = null, List<IDialog> steps = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);

            this.Constraint = constraint;
            this.Steps = steps;
        }

        /// <summary>
        /// Gets or sets the constraint to apply to the rule (OPTIONAL) 
        /// </summary>
        public string Constraint { get; set; }

        /// <summary>
        /// Gets or sets the steps to add to the plan when the rule constraints are met
        /// </summary>
        public List<IDialog> Steps { get; set; } = new List<IDialog>();

        /// <summary>
        /// Get the expression for this rule by calling GatherConstraints()
        /// </summary>
        public Expression GetExpression(IExpressionParser parser)
        {
            if (this.constraint == null)
            {
                this.constraint = BuildExpression(parser);
            }

            return this.constraint;
        }

        /// <summary>
        /// Override this method to define the expression which is evaluated to determine if this rule should fire
        /// </summary>
        /// <returns>Expression which will be cached and used to evaluate this rule</returns>
        protected virtual Expression BuildExpression(IExpressionParser factory)
        {
            List<Expression> allExpressions = new List<Expression>();
            if (!String.IsNullOrEmpty(this.Constraint))
            {
                allExpressions.Add(factory.Parse(this.Constraint));
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
        public void AddConstraint(Expression constraint)
        {
            this.extraConstraints.Add(constraint);
        }

        /// <summary>
        /// Method called to execute the rule's steps
        /// </summary>
        /// <param name="planningContext"></param>
        /// <param name="dialogEvent"></param>
        /// <returns></returns>
        public async Task<List<StepChangeList>> ExecuteAsync(SequenceContext planningContext)
        {
            return await OnExecuteAsync(planningContext).ConfigureAwait(false);
        }


        /// <summary>
        /// Method called to process the request to execute the steps
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dialogEvent"></param>
        /// <returns></returns>
        public async virtual Task<List<StepChangeList>> OnExecuteAsync(SequenceContext planning)
        {
            return new List<StepChangeList>()
            {
                this.OnCreateChangeList(planning)
            };
        }

        protected virtual StepChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            var changeList = new StepChangeList()
            {
                Steps = new List<StepState>()
            };

            Steps.ForEach(s =>
            {
                var stepState = new StepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id
                };

                if (dialogOptions != null)
                {
                    stepState.Options = dialogOptions;
                }

                changeList.Steps.Add(stepState);
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

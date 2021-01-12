using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    /// <summary>
    /// Policy for a specific Action, or Trigger.
    /// </summary>
    internal class ActionPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionPolicy"/> class.
        /// </summary>
        public ActionPolicy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionPolicy"/> class.
        /// </summary>
        /// <param name="kind">Action or Trigger $kind to which the aciton policy applies.</param>
        /// <param name="actionPolicyType">The <see cref="ActionPolicyType"/> of this action policy.</param>
        /// <param name="actions">(Optional) action(s) for this policy.</param>
        public ActionPolicy(string kind, ActionPolicyType actionPolicyType, IEnumerable<string> actions = null)
        {
            ActionPolicyType = actionPolicyType;
            Actions = actions;
            Kind = kind;
        }

        /// <summary>
        /// Gets the $Kind of the Action or Trigger to which this Action Policy applies.
        /// </summary>
        /// <value>
        /// The $Kind of the Action or Trigger for this Action Policy.
        /// </value>
        public string Kind { get; }

        /// <summary>
        /// Gets the Action Policy Type.
        /// </summary>
        /// <value>
        /// ActionPolicyType value.
        /// </value>
        public ActionPolicyType ActionPolicyType { get; }

        /// <summary>
        /// Gets actions for this action policy. See <see cref="ActionPolicyType"/> for
        /// a description of what the actions represent.
        /// </summary>
        /// <value>
        /// Collection of actions for this policy.
        /// </value>
        public IEnumerable<string> Actions { get; }
    }
}

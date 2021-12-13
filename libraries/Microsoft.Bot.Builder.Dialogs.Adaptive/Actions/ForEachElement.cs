// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Executes a set of actions once for each item in an in-memory list or collection.
    /// </summary>
    public class ForEachElement : Dialog, IDialogDependencies
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Foreach";

        private const string IterationKey = "index";
        private const string IterationValue = "value";
        private const string ActionScopeState = "this.actionScopeState";
        private const string ForeachIndex = "this.foreachIndex";
        private const string CachedItemsProperty = "this.cachedItems";

        private readonly ActionScope _scope;

        private List<Dialog> _actions = new List<Dialog>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ForEachElement"/> class.
        /// </summary>
        /// <param name="actions">The actions to execute.</param>
        public ForEachElement(IEnumerable<Dialog> actions = null)
        {
            if (actions != null)
            {
                _actions = new List<Dialog>(actions);
            }

            _scope = new ActionScope(actions);
        }

        /// <summary>
        /// Gets or sets the actions to execute.
        /// </summary>
        /// <value>The actions to execute.</value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<Dialog> Actions
        {
            get
            {
                return this._actions;
            }

            set
            {
                this._actions = value ?? new List<Dialog>();
            }
        }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets property path expression to the collection of items.
        /// </summary>
        /// <value>
        /// Property path expression to the collection of items.
        /// </value>
        [JsonProperty("itemsProperty")]
        public StringExpression ItemsProperty { get; set; }

        /// <summary>
        /// Gets or sets property path expression to item index.
        /// </summary>
        /// <value>
        /// Property path expression to the item index.
        /// </value>
        [JsonProperty("index")]
        public StringExpression Index { get; set; } = "dialog.foreach.index";

        /// <summary>
        /// Gets or sets property path expression to item value.
        /// </summary>
        /// <value>
        /// Property path expression to the item value.
        /// </value>
        [JsonProperty("value")]
        public StringExpression Value { get; set; } = "dialog.foreach.value";

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            dc.State.SetValue(ForeachIndex, 0);
            return await RunItemsAsync(dc, beginDialog: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            return await RunItemsAsync(dc, beginDialog: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            var res = await base.ResumeDialogAsync(dc, reason, result, cancellationToken).ConfigureAwait(false);
            return res;
        }

        /// <summary>
        /// Enumerates child dialog dependencies so they can be added to the containers dialog set.
        /// </summary>
        /// <returns>Dialog enumeration.</returns>
        public virtual IEnumerable<Dialog> GetDependencies()
        {
            foreach (var action in Actions)
            {
                yield return action;
            }
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({this.ItemsProperty?.ToString()})";
        }

        private async Task<DialogTurnResult> RunItemsAsync(DialogContext dc, bool beginDialog = true, CancellationToken cancellationToken = default)
        {
            // Get list information
            var list = GetItemsProperty(dc.State, beginDialog);

            var index = dc.State.GetIntValue(ForeachIndex, 0);

            // Next item
            while (list != null && index < list.Count)
            {
                var childDialogState = GetActionScopeState(dc);
                var childDc = new DialogContext(new DialogSet().Add(_scope), dc.Parent ?? dc, childDialogState);
                childDc.Parent = dc.Parent;
                if (dc.Services != null)
                {
                    foreach (var service in dc.Services)
                    {
                        childDc.Services[service.Key] = service.Value;
                    }
                }

                dc.State.SetValue(Value.GetValue(dc.State), list[index][IterationValue]);
                dc.State.SetValue(Index.GetValue(dc.State), list[index][IterationKey]);

                var options = new Dictionary<string, object>()
                {
                    { Value.GetValue(dc.State), list[index][IterationValue] },
                    { Index.GetValue(dc.State), list[index][IterationKey] },
                };

                DialogTurnResult turnResult;

                if (beginDialog)
                {
                    turnResult = await childDc.BeginDialogAsync(_scope.Id, options: options, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    turnResult = await childDc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                }

                if (turnResult.Status == DialogTurnStatus.Waiting)
                {
                    UpdateActionScopeState(dc, childDialogState);
                    return turnResult;
                }

                index++;
                dc.State.SetValue(ForeachIndex, index);

                if (turnResult.Status == DialogTurnStatus.CompleteAndWait)
                {
                    // Child dialog completed, but wants us to wait for a new activity
                    turnResult.Status = DialogTurnStatus.Waiting;
                    UpdateActionScopeState(dc, childDialogState);
                    return turnResult;
                }

                beginDialog = true;
                UpdateActionScopeState(dc, new DialogState());
            }

            // End of list has been reached, or the list is null
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private void UpdateActionScopeState(DialogContext dc, DialogState state)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;

            if (activeDialogState != null)
            {
                activeDialogState[ActionScopeState] = state;
            }
        }

        private DialogState GetActionScopeState(DialogContext dc)
        {
            DialogState state = null;
            var activeDialogState = dc.ActiveDialog?.State as Dictionary<string, object>;

            if (activeDialogState != null && activeDialogState.TryGetValue(ActionScopeState, out var currentState))
            {
                state = currentState as DialogState;
            }

            if (state == null)
            {
                state = new DialogState();
            }

            activeDialogState[ActionScopeState] = state;

            return state;
        }

        private JArray GetItemsProperty(Memory.DialogStateManager state, bool beginDialog)
        {
            var instance = state.GetValue<object>(this.ItemsProperty.GetValue(state));
            var result = new JArray();
            if (FunctionUtils.TryParseList(instance, out var list))
            {
                for (var i = 0; i < list.Count; i++)
                {
                    result.Add(new JObject
                    {
                        [IterationKey] = ConvertToJToken(i),
                        [IterationValue] = ConvertToJToken(list[i])
                    });
                }
            }
            else if (instance is JObject jobj)
            {
                result = Object2List(jobj);
            }
            else if (ConvertToJToken(instance) is JObject jobject)
            {
                result = Object2List(jobject);
            }

            if (beginDialog)
            {
                state.SetValue(CachedItemsProperty, result);
            }
            else if (result == null || result.Count == 0)
            {
                if (state.TryGetValue<JArray>(CachedItemsProperty, out JArray cached))
                {
                    result = cached;
                }
            }

            return result;
        }

        private JArray Object2List(JObject jobj)
        {
            var result = new JArray();
            foreach (var item in jobj)
            {
                result.Add(new JObject
                {
                    [IterationKey] = ConvertToJToken(item.Key),
                    [IterationValue] = ConvertToJToken(item.Value)
                });
            }

            return result;
        }

        private JToken ConvertToJToken(object value)
        {
            return value == null ? JValue.CreateNull() : JToken.FromObject(value);
        }
    }
}

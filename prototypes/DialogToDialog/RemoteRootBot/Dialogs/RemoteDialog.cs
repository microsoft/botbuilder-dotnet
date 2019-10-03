// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace RemoteRootBot.Dialogs
{
    // TODO: work with Steve and see if we can make this a Dialog instead of a component dialog.
    public class RemoteDialog : ComponentDialog
    {
        private readonly SkillConnector _skillConnector;

        public RemoteDialog(SkillConnector skillConnector)
            : base(nameof(RemoteDialog))
        {
            _skillConnector = skillConnector;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var turnContext = dc.Context;
            AddActionToActivity(turnContext.Activity, options);

            // Send message with semantic action to the remote skill.
            return await SendToSkill(dc, turnContext.Activity, cancellationToken);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // Just forward to the remote skill
            return await SendToSkill(dc, dc.Context.Activity, cancellationToken);
        }

        private static void AddActionToActivity(Activity activity, object options)
        {
            // Set the action and the entities on the activity before sending it to the remote skill.
            activity.SemanticAction = new SemanticAction("BookFlight")
            {
                Entities = new Dictionary<string, Entity>
                {
                    { "bookingInfo", new Entity() },
                },
            };

            var bookingDetails = (BookingDetails)options;
            activity.SemanticAction.Entities["bookingInfo"].SetAs(bookingDetails);
        }

        private async Task<DialogTurnResult> SendToSkill(DialogContext dc, Activity activity, CancellationToken cancellationToken)
        {
            var ret = await _skillConnector.ProcessActivityAsync(dc.Context, activity, cancellationToken);

            var turnResult = new DialogTurnResult(DialogTurnStatus.Waiting);

            // Check if the remote skill ended.
            if (ret.Status == SkillTurnStatus.Complete)
            {
                // Pull booking details from the response if they are there and return the as part of the end dialog.
                // TODO: figure out an elegant way of casting the return value.
                var bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(JsonConvert.SerializeObject(ret.Result));
                return await EndComponentAsync(dc, bookingDetails, cancellationToken);
            }

            return turnResult;
        }
    }
}

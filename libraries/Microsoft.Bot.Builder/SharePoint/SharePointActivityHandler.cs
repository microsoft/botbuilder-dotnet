// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.SharePoint;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.SharePoint
{
    /// <summary>
    /// The SharePointActivityHandler is derived from ActivityHandler. It adds support for 
    /// the SharePoint specific events and interactions.
    /// </summary>
    public class SharePointActivityHandler : ActivityHandler
    {
        /// <summary>
        /// Invoked when an invoke activity is received from the connector.
        /// Invoke activities can be used to communicate many different things.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Invoke activities communicate programmatic commands from a client or channel to a bot.
        /// The meaning of an invoke activity is defined by the <see cref="IInvokeActivity.Name"/> property,
        /// which is meaningful within the scope of a channel.
        /// </remarks>
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                if (turnContext.Activity.Name == null && turnContext.Activity.ChannelId == Channels.SharePoint)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    switch (turnContext.Activity.Name)
                    {
                        case "cardExtension/getCardView":
                            return CreateInvokeResponse(await OnSharePointTaskGetCardViewAsync(turnContext, SafeCast<TaskModuleRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "cardExtension/getQuickView":
                            return CreateInvokeResponse(await OnSharePointTaskGetQuickViewAsync(turnContext, SafeCast<TaskModuleRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "cardExtension/getPropertyPaneConfiguration":
                            return CreateInvokeResponse(await OnSharePointTaskGetPropertyPaneConfigurationAsync(turnContext, SafeCast<TaskModuleRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "cardExtension/setPropertyPaneConfiguration":
                            await OnSharePointTaskSetPropertyPaneConfigurationAsync(turnContext, SafeCast<TaskModuleRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false);
                            return CreateInvokeResponse();
                    }
                }
            }
            catch (InvokeResponseException e)
            {
                return e.CreateInvokeResponse();
            }

            return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a card view is fetched.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="taskModuleRequest">The task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected virtual Task<GetCardViewResponse> OnSharePointTaskGetCardViewAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a quick view is fetched.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="taskModuleRequest">The task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected virtual Task<GetQuickViewResponse> OnSharePointTaskGetQuickViewAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for getting configuration pane properties.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="taskModuleRequest">The task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected virtual Task<GetPropertyPaneConfigurationResponse> OnSharePointTaskGetPropertyPaneConfigurationAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for setting configuration pane properties.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="taskModuleRequest">The task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected virtual Task OnSharePointTaskSetPropertyPaneConfigurationAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Safely casts an object to an object of type <typeparamref name="T"/> .
        /// </summary>
        /// <param name="value">The object to be casted.</param>
        /// <returns>The object casted in the new type.</returns>
        private static T SafeCast<T>(object value)
        {
            var obj = value as JObject;
            if (obj == null)
            {
                throw new InvokeResponseException(HttpStatusCode.BadRequest, $"expected type '{value.GetType().Name}'");
            }

            return obj.ToObject<T>();
        }
    }
}

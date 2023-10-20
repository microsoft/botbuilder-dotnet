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
                if (turnContext.Activity.Name == null)
                {
                    throw new NotSupportedException();
                }
                else
                {
                    switch (turnContext.Activity.Name)
                    {
                        case "cardExtension/getCardView":
                            return CreateInvokeResponse(await OnSharePointTaskGetCardViewAsync(turnContext, SafeCast<AceRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "cardExtension/getQuickView":
                            return CreateInvokeResponse(await OnSharePointTaskGetQuickViewAsync(turnContext, SafeCast<AceRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "cardExtension/getPropertyPaneConfiguration":
                            return CreateInvokeResponse(await OnSharePointTaskGetPropertyPaneConfigurationAsync(turnContext, SafeCast<AceRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "cardExtension/setPropertyPaneConfiguration":
                            BaseHandleActionResponse setPropPaneConfigResponse = await OnSharePointTaskSetPropertyPaneConfigurationAsync(turnContext, SafeCast<AceRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false);
                            ValidateSetPropertyPaneConfigurationResponse(setPropPaneConfigResponse);
                            return CreateInvokeResponse(setPropPaneConfigResponse);

                        case "cardExtension/handleAction":
                            return CreateInvokeResponse(await OnSharePointTaskHandleActionAsync(turnContext, SafeCast<AceRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));
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
        /// <param name="aceRequest">The ACE invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Card View Response for the request.</returns>
        protected virtual Task<CardViewResponse> OnSharePointTaskGetCardViewAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a quick view is fetched.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="aceRequest">The ACE invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Quick View Response for the request.</returns>
        protected virtual Task<QuickViewResponse> OnSharePointTaskGetQuickViewAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for getting configuration pane properties.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="aceRequest">The ACE invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Property Pane Configuration Response for the request.</returns>
        protected virtual Task<GetPropertyPaneConfigurationResponse> OnSharePointTaskGetPropertyPaneConfigurationAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for setting configuration pane properties.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="aceRequest">The ACE invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>Card view or no-op action response.</returns>
        /// <remarks>The handler will fail with 500 status code if the response is of type <see cref="QuickViewHandleActionResponse" />.</remarks>
        protected virtual Task<BaseHandleActionResponse> OnSharePointTaskSetPropertyPaneConfigurationAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for handling ACE actions.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="aceRequest">The ACE invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A view response.</returns>
        protected virtual Task<BaseHandleActionResponse> OnSharePointTaskHandleActionAsync(ITurnContext<IInvokeActivity> turnContext, AceRequest aceRequest, CancellationToken cancellationToken)
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

        private void ValidateSetPropertyPaneConfigurationResponse(BaseHandleActionResponse response)
        {
            if (response is QuickViewHandleActionResponse)
            {
                throw new InvokeResponseException(HttpStatusCode.InternalServerError, "Response for SetPropertyPaneConfiguration action can't be of QuickView type.");
            }
        }
    }
}

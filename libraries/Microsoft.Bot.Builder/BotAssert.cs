// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Provides methods for debugging Bot Builder code.
    /// </summary>
    public class BotAssert
    {
        /// <summary>
        /// Checks that an activity object is not <c>null</c>.
        /// </summary>
        /// <param name="activity">The activity object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="activity"/> is <c>null</c>.</exception>
        public static void ActivityNotNull(IActivity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }
        }

        /// <summary>
        /// Checks that a context object is not <c>null</c>.
        /// </summary>
        /// <param name="context">The context object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context"/> is <c>null</c>.</exception>
        public static void ContextNotNull(ITurnContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        /// <summary>
        /// Checks that a conversation reference object is not <c>null</c>.
        /// </summary>
        /// <param name="reference">The conversation reference object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="reference"/> is <c>null</c>.</exception>
        public static void ConversationReferenceNotNull(ConversationReference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }
        }

        /// <summary>
        /// Checks that an activity collection is not <c>null</c>.
        /// </summary>
        /// <param name="activities">The activities.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="activities"/> is <c>null</c>.</exception>
        public static void ActivityListNotNull(IEnumerable<Activity> activities)
        {
            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }
        }

        /// <summary>
        /// Checks that a middleware object is not <c>null</c>.
        /// </summary>
        /// <param name="middleware">The middleware object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="middleware"/> is <c>null</c>.</exception>
        public static void MiddlewareNotNull(IMiddleware middleware)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }
        }

        /// <summary>
        /// Checks that a middleware collection is not <c>null</c>.
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="middleware"/> is <c>null</c>.</exception>
        public static void MiddlewareNotNull(IEnumerable<IMiddleware> middleware)
        {
            if (middleware == null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }
        }
    }
}

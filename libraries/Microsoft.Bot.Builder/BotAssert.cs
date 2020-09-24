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
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable (we can't change this without breaking binary compat)
    public class BotAssert
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
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
        /// <param name="turnContext">The context object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="turnContext"/> is <c>null</c>.</exception>
        public static void ContextNotNull(ITurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
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

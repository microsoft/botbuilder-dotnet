// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Observers
{
    /// <summary>
    /// Marks a <see cref="JsonConverter"/> that allows registrations of <see cref="IConverterObserver"/>.
    /// </summary>
    public interface IObservableConverter
    {
        /// <summary>
        /// Registers a <see cref="IConverterObserver"/> to receive notifications on converter events.
        /// </summary>
        /// <param name="observer">The observer to be registered.</param>
        void RegisterObserver(IConverterObserver observer);
    }
}

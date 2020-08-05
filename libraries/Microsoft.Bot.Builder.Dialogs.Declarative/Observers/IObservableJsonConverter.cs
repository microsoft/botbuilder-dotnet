// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Observers
{
    /// <summary>
    /// Marks a <see cref="JsonConverter"/> that allows registrations of <see cref="IJsonLoadObserver"/>.
    /// </summary>
    public interface IObservableJsonConverter
    {
        /// <summary>
        /// Registers an observerto receive notifications on converter events.
        /// </summary>
        /// <param name="observer">The <see cref="IJsonLoadObserver"/> to be registered.</param>
        void RegisterObserver(IJsonLoadObserver observer);
    }
}

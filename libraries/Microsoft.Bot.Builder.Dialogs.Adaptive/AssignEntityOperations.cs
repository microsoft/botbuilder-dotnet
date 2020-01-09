// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Built-in ways to assign entities to properties.
    /// </summary>
    public static class AssignEntityOperations
    {
        /// <summary>
        /// Add an entity to a property.
        /// </summary>
        public const string Add = "add";

        /// <summary>
        /// Remove an entity from a property.
        /// </summary>
        public const string Remove = "remove";

        /// <summary>
        /// Clear a properties value.
        /// </summary>
        public const string Clear = "clear";
    }
}

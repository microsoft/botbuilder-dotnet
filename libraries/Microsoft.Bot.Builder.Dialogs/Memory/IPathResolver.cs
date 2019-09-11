// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    public interface IPathResolver
    {
        /// <summary>
        /// Override this method to have your resolver say that it can handle the path.
        /// </summary>
        /// <param name="path">path to inspect.</param>
        /// <returns>true if it will resolve the path.</returns>
        bool Matches(string path);

        /// <summary>
        /// Return the value from the obj using the transformed path.
        /// </summary>
        /// <typeparam name="T">type of value to get</typeparam>
        /// <param name="dc">dc</param>
        /// <param name="path">path to use.</param>
        /// <param name="value">value</param>
        /// <returns>true if value is found</returns>
        bool TryGetValue<T>(DialogContext dc, string path, out T value);

        /// <summary>
        /// Remove the value from the obj using the transformed path.
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="path">path to use.</param>
        void RemoveValue(DialogContext dc, string path);

        /// <summary>
        /// SEt the value from the obj using the transformed path.
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="path">path to use.</param>
        /// <param name="value">value to set</param>
        void SetValue(DialogContext dc, string path, object value);
    }
}

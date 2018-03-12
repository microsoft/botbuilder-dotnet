// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Ai
{
    public interface ILocaleConverter
    {
        bool IsLocaleAvailable(string locale);
        Task<string> Convert(string message, string fromLocale, string toLocale);
        string[] GetAvailableLocales();
    }
}

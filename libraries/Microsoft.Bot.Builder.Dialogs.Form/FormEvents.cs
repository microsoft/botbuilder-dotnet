// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using Microsoft.Bot.Builder.Dialogs.Adaptive;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormEvents : AdaptiveEvents
    {
        public const string Ask = "ask";
        public const string ChooseEntity = "chooseEntity";
        public const string ChooseMapping = "chooseMapping";
        public const string ChooseProperty = "chooseProperty";
        public const string ClarifyEntity = "clarifyEntity";
        public const string ClearProperty = "clearProperty";
        public const string SetPropertyToEntity = "setPropertyToEntity";
        public const string UnknownEntity = "unknownEntity";
    }
}

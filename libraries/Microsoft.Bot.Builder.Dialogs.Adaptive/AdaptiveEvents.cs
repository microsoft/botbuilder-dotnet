// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class AdaptiveEvents : DialogEvents
    {
        public const string RecognizeUtterance = "recognizeUtterance";
        public const string RecognizedIntent = "recognizedIntent";
        public const string UnknownIntent = "unknownIntent";
        public const string EndOfPlan = "endOfPlan";
        public const string EndOfActions = "endOfActions";
        public const string ChooseProperty = "chooseProperty";
        public const string ChooseEntity = "chooseEntity";
        public const string ClearProperty = "clearProperty";
        public const string AssignEntity = "assignEntity";
    }
}

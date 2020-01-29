// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    public class AdaptiveEvents : DialogEvents
    {
        /// <summary>
        /// Raised when utterance is recieved.
        /// </summary>
        public const string RecognizeUtterance = "recognizeUtterance";

        /// <summary>
        /// Raised when intent is recognized from utterance.
        /// </summary>
        public const string RecognizedIntent = "recognizedIntent";

        /// <summary>
        /// Raised when no intent can be identified from utterance.
        /// </summary>
        public const string UnknownIntent = "unknownIntent";

        /// <summary>
        /// Raised when all actions and ambiguity events have been finished.
        /// </summary>
        public const string EndOfActions = "endOfActions";

        /// <summary>
        /// Raised when there are multiple possible entity to property mappings.
        /// </summary>
        public const string ChooseProperty = "chooseProperty";

        /// <summary>
        /// Raised when there are multiple possible resolutions of an entity.
        /// </summary>
        public const string ChooseEntity = "chooseEntity";

        /// <summary>
        /// Raised when a property should be cleared.
        /// </summary>
        public const string ClearProperty = "clearProperty";

        /// <summary>
        /// Raised when an entity should be assigned to a property.
        /// </summary>
        public const string AssignEntity = "assignEntity";
    }
}

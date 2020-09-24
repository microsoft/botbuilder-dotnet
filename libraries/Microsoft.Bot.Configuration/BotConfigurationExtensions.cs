// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Extension methods for <see cref="BotConfiguration"/>.
    /// </summary>
    [Obsolete("This class is deprecated.  See https://aka.ms/bot-file-basics for more information.", false)]
    public static class BotConfigurationExtensions
    {
        /// <summary>
        /// Check if the Project's Name contains white spaces.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <param name="folder">Folder's path to validate.</param>
        /// <exception cref="System.Exception"> Thrown when no '.scproj' file is found or when there are two or more 'csproj' files.</exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when param 'folder' is null.</exception>
        /// <returns>Boolean that indicates if a Project's Name is spaceless.</returns>
        public static bool IsProjectNameSpaceless(this BotConfiguration botConfiguration, string folder)
        {
            // If the parameter 'folder' is null or empty, throws an Exception
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            // Gets the path to the first (and only) file with extension 'csproj'.
            var file = Directory.EnumerateFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();

            // Throws a FileNotFoundException if there isn't any `.csproj` in the specified folder
            if (file == null)
            {
                throw new FileNotFoundException("There isn't any \'.csproj\' in the specified folder.");
            }

            // Checks if the file name contains any white space
            return !file.Contains(" ");
        }

        /// <summary>
        /// Checks if the <see cref="LuisService"/> has its key defined.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <returns>Tuple with a boolean that indicates if a bot configuration file's Luis keys are present and its Error message.</returns>
        public static (bool IsValid, string ErrMsg) ValidateLuisKeyExistence(this BotConfiguration botConfiguration)
        {
            // Get all the luis types in the service list
            var luisServices = botConfiguration.Services.OfType<LuisService>();

            // If there isn't any Luis service, returns false and the corresponding error message
            if (!luisServices.Any())
            {
                return (false, "There is no LUIS service in the bot configuration file.");
            }

            // Check if exists any LuisService without its key (SubscriptionKey)
            var existEmptyLuis = luisServices.Any(x => string.IsNullOrWhiteSpace(x.SubscriptionKey));

            // If there is at least one LuisService without its key, the process will return false
            if (existEmptyLuis)
            {
                return (false, "There is at least one LuisService without a key.");
            }

            // Get all the Dispatch (Luis) types in the service list
            var dispatchServices = botConfiguration.Services.OfType<DispatchService>();

            // The dispatch service is not always included in the bot configuration files.
            // Therefore, it will only be validated if the bot configuration file contains at least one service of this type.
            if (dispatchServices.Any())
            {
                // Check if exists any dispatchServices without its key (SubscriptionKey)
                var existEmptyDispatch = dispatchServices.Any(x => string.IsNullOrWhiteSpace(x.SubscriptionKey));

                var errMsg = existEmptyDispatch ? "There is at least one ServiceDispatch without a key." : string.Empty;

                // If there is at least one dispatch service without its key, the process will return false and the corresponding error message
                return (!existEmptyDispatch, errMsg);
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Checks if the <see cref="QnAMakerService"/> has its key defined.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <returns>Boolean that indicates if a bot configuration file's QnAMaker keys are present.</returns>
        public static (bool IsValid, string ErrMsg) ValidateQnAKeyExistence(this BotConfiguration botConfiguration)
        {
            // Get all the QnA types in the service list
            var qnaServices = botConfiguration.Services.OfType<QnAMakerService>();

            // If there isn't any qna service, returns false and the corresponding error message
            if (!qnaServices.Any())
            {
                return (false, "There is no QnAMaker service in the bot configuration file.");
            }

            // Check if exists any QnAMakerService without its key (SubscriptionKey)
            var existEmptyQnA = qnaServices.Any(x => string.IsNullOrWhiteSpace(x.SubscriptionKey));

            var errorMsg = existEmptyQnA ? "There is at least one QnA service without its key." : string.Empty;

            // If there is at least one QnAMakerService without its key, the process will return false
            return (!existEmptyQnA, errorMsg);
        }
    }
}

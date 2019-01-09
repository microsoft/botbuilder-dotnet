// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class BotConfigurationExtensions
    {
        /// <summary>
        /// Returns a IEnumerable of a specific <see cref="ConnectedService"/> type contained in the bot configuration file.
        /// </summary>
        /// <param name="botServices">BotConfiguration's services.</param>
        /// <param name="serviceType">The type of <see cref="ConnectedService"/> to search.</param>
        /// <returns>List of bot's <see cref="ConnectedService"/>.</returns>
        public static IEnumerable<ConnectedService> OfType(this List<ConnectedService> botServices, string serviceType)
        {
            // If botServices is null, throws an exception
            if (botServices == null)
            {
                throw new ArgumentNullException();
            }

            // If there aren't any service, returns an empty IEnumerable
            if (botServices.Count == 0)
            {
                return Enumerable.Empty<ConnectedService>();
            }

            // Obtains all the services of the specified type
            var botFilteredServices = botServices.Where(service => service.Type.Equals(serviceType, StringComparison.CurrentCultureIgnoreCase));

            // If there isn't any service of the specified type, returns an empty IEnumerable
            if (botFilteredServices == null || botFilteredServices.Count() == 0)
            {
                return Enumerable.Empty<ConnectedService>();
            }

            return botFilteredServices;
        }

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
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            var files = Directory.GetFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly);

            // Throws an error if there are two or more 'csproj' files
            if (files.Length > 1)
            {
                throw new Exception("There could not be more than one '.csproj' file.");
            }

            try
            {
                // Gets the path to the first (and only) file with extension 'csproj'.
                var file = files.FirstOrDefault();

                // Checks if the file name contains any white space
                return !file.Contains(" ");
            }
            catch (FileNotFoundException ex)
            {
                // If there's no file, throws an Exception
                throw new FileNotFoundException("There isn't any \'.csproj\' in the specified folder", ex);
            }
        }

        /// <summary>
        /// Checks if the <see cref="LuisService"/> has its key defined.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <returns>Tuple with a boolean that indicates if a bot configuration file's Luis keys are present and its Error message.</returns>
        public static Tuple<bool, string> ValidateLuisKeyExistence(this BotConfiguration botConfiguration)
        {
            // Get all the luis types in the service list
            var luisServices = botConfiguration.Services.OfType(ServiceTypes.Luis);

            // If there isn't any Luis service, returns false and the corresponding error message
            if (luisServices == null)
            {
                return Tuple.Create(false, "There is no LUIS service in the bot configuration file.");
            }

            // Check if exists any LuisService without its key (SubscriptionKey)
            var existEmptyLuis = luisServices.Any(x => string.IsNullOrWhiteSpace(((LuisService)x).SubscriptionKey));

            // If there is at least one LuisService without its key, the process will return false
            if (existEmptyLuis)
            {
                return Tuple.Create(false, "There is at least one LuisService without a key.");
            }

            // Get all the Dispatch (Luis) types in the service list
            var dispatchServices = botConfiguration.Services.OfType(ServiceTypes.Dispatch);

            // The dispatch service is not always included in the the bot configuration files.
            // Therefore, it will only be validated if the bot configuration file contains at least one service of this type.
            if (dispatchServices.Count() != 0)
            {
                // Check if exists any dispatchServices without its key (SubscriptionKey)
                var existEmptyDispatch = dispatchServices.Any(x => string.IsNullOrWhiteSpace(((DispatchService)x).SubscriptionKey));

                string errMsg = existEmptyDispatch ? "There is at least one ServiceDispatch without a key." : string.Empty;

                // If there is at least one dispatch service without its key, the process will return false and the corresponding error message
                return Tuple.Create(!existEmptyDispatch, errMsg);
            }

            return Tuple.Create(true, string.Empty);
        }

        /// <summary>
        /// Checks if the <see cref="QnAMakerService"/> has its key defined.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <returns>Boolean that indicates if a bot configuration file's QnAMaker keys are present.</returns>
        public static Tuple<bool, string> ValidateQnAKeyExistence(this BotConfiguration botConfiguration)
        {
            // Get all the QnA types in the service list
            var qnaServices = botConfiguration.Services.OfType(ServiceTypes.QnA);

            // If there isn't any qna service, returns false and the corresponding error message
            if (qnaServices.Count() == 0)
            {
                return Tuple.Create(false, "There is no QnAMaker service in the bot configuration file.");
            }

            // Check if exists any QnAMakerService without its key (SubscriptionKey)
            var existEmptyQnA = qnaServices.Any(x => string.IsNullOrWhiteSpace(((QnAMakerService)x).SubscriptionKey));

            var errorMsg = existEmptyQnA ? "There is at least one QnA service without its key." : string.Empty;

            // If there is at least one QnAMakerService without its key, the process will return false
            return Tuple.Create(!existEmptyQnA, errorMsg);
        }
    }
}

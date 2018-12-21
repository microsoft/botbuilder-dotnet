using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Bot.Configuration
{
    public static class ValidationUtilities
    {
        /// <summary>
        /// Returns a IEnumerable of a specific <see cref="ConnectedService"/> type contained in the bot configuration file.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <param name="serviceType">The type of <see cref="ConnectedService"/> to search.</param>
        /// <returns>List of bot's <see cref="ConnectedService"/>.</returns>
        public static IEnumerable<ConnectedService> FindServicesByType(this BotConfiguration botConfiguration, string serviceType)
        {
            // List that contains the bot's services.
            var botServices = botConfiguration.Services;

            // If there isn't any service, throws an Exception
            if (botServices == null || botServices.Count() == 0)
            {
                throw new Exception("There are no services");
            }

            // Obtains all the services of the specified type
            var botFilteredServices = botServices.Where(service => service.Type.ToLower() == serviceType.ToLower());

            // If there isn't any service of the specified type, throws an Exception
            if (botFilteredServices == null || botFilteredServices.Count() == 0)
            {
                throw new Exception("There are no services of the specified type: " + serviceType);
            }

            return botFilteredServices;
        }

        /// <summary>
        /// Check if the Project's Name contains white spaces.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <param name="folder">Folder's path to validate.</param>
        /// <exception cref="System.Exception"> Thrown when no '.scproj' file is found.</exception>
        /// <exception cref="System.ArgumentNullException"> Thrown when param 'folder' is null.</exception>
        /// <returns>Boolean that indicates if a Project's Name is spaceless.</returns>
        public static bool IsProjectNameSpaceless(this BotConfiguration botConfiguration, string folder)
        {
            try
            {
                // If the parameter 'folder' is null or empty, throws an Exception
                if (string.IsNullOrEmpty(folder))
                {
                    throw new ArgumentNullException(nameof(folder));
                }

                // Gets the path to the first file with extension 'csproj'
                var file = Directory.GetFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();

                // Checks if the file name contains any white space
                var containsEmptySpaces = file.ToCharArray().Any(x => x == ' ');

                return !containsEmptySpaces;
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
        /// <returns>Boolean that indicates if a bot configuration file's Luis keys are present.</returns>
        public static bool ValidateLuisKey(this BotConfiguration botConfiguration)
        {
            try
            {
                // Get all the luis types in the service list
                var luisServices = botConfiguration.FindServicesByType(ServiceTypes.Luis);

                // If there isn't any Luis service, throws exception
                if (luisServices == null)
                {
                    throw new Exception("There is no LUIS service in the bot configuration file.");
                }

                // Check if exists any LuisService without its key (SubscriptionKey)
                var existEmptyLuis = luisServices.Any(x => string.IsNullOrWhiteSpace(((LuisService)x).SubscriptionKey));

                // If there is at least one LuisService without its key, the process will return false
                if (existEmptyLuis)
                {
                    return false;
                }

                // Get all the Dispatch (Luis) types in the service list
                var dispatchServices = botConfiguration.FindServicesByType(ServiceTypes.Dispatch);

                // The dispatch service is not always included in the the bot configuration files.
                // Therefore, it will only be validated if the bot configuration file contains at least one service of this type.
                if (dispatchServices != null && dispatchServices.Count() != 0)
                {
                    // Check if exists any dispatchServices without its key (SubscriptionKey)
                    var existEmptyDispatch = dispatchServices.Any(x => string.IsNullOrWhiteSpace(((DispatchService)x).SubscriptionKey));

                    // If there is at least one dispatch service without its key, the process will return false
                    return !existEmptyDispatch;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks if the <see cref="QnAMakerService"/> has its key defined.
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration object to extend.</param>
        /// <returns>Boolean that indicates if a bot configuration file's QnAMaker keys are present.</returns>
        public static bool ValidateQnAKey(this BotConfiguration botConfiguration)
        {
            try
            {
                // Get all the QnA types in the service list
                var qnaServices = botConfiguration.FindServicesByType(ServiceTypes.QnA);

                // If there isn't any qna service, throws exception
                if (qnaServices == null)
                {
                    throw new Exception("There is no QnAMaker service in the bot configuration file.");
                }

                // Check if exists any QnAMakerService without its key (SubscriptionKey)
                var existEmptyQnA = qnaServices.Any(x => string.IsNullOrWhiteSpace(((QnAMakerService)x).SubscriptionKey));

                // If there is at least one QnAMakerService without its key, the process will return false
                return !existEmptyQnA;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

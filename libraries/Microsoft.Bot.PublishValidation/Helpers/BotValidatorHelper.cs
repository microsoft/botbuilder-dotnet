// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.PublishValidation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Bot.Configuration;

    public class BotValidatorHelper
    {
        private const string FORBIDDENENDPOINTS = "Forbidden Endpoints";
        private const string REQUIREDENDPOINTS = "Required Endpoints";

        /// <summary>
        /// Checks if a bot file is valid or not according to some configuration options
        /// </summary>
        /// <param name="configurationOptions">Configuration Options</param>
        /// <param name="messages">Error Messages</param>
        /// <returns>bool that indicates if a .bot file is valid</returns>
        public static bool BotFileIsValid(ConfigurationOptions configurationOptions, List<NotificationMessage> messages)
        {
            try
            {
                // Load the first .bot file from the provided folder.
                // Also validates its existence (throws an exception is there isn't any file)
                var botConfiguration = BotValidatorHelper.LoadFromFolder(configurationOptions);

                return BotValidatorHelper.ValidateBotFile(botConfiguration, configurationOptions, messages);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Performs the validation of the specified .bot file, according the specified validation options
        /// </summary>
        /// <param name="botConfiguration">Bot Configuration</param>
        /// <param name="options">Options</param>
        /// <param name="messages">Messages</param>
        /// <returns>bool that indicates if a .bot file is valid</returns>
        private static bool ValidateBotFile(BotConfiguration botConfiguration, ConfigurationOptions options, List<NotificationMessage> messages)
        {
            try
            {
                NotificationMessage message;

                var errorMsg = string.Empty;
                var validationResult = true;
                var missingEndpoints = string.Empty;

                // Checks if the project name which uses the .bot file contains white spaces.
                // If it has at least one white space, the process will fail and return an error.
                if (options.ForbidSpacesInProjectName)
                {
                    if (!BotValidatorHelper.ProjectNameIsValid(options.ProjectPath, out errorMsg))
                    {
                        message = new NotificationMessage(errorMsg, NotificationMessageTypes.Error);
                        messages.Add(message);
                        validationResult = false;
                    }
                }

                if (!options.RequireBotFile)
                {
                    return validationResult;
                }

                // Check if the .bot file contains the specified endpoints
                if (options.RequiredEndpoints.Count() != 0)
                {
                    if (!BotValidatorHelper.ValidateEndpoints(botConfiguration, options.RequiredEndpoints, true, out missingEndpoints))
                    {
                        errorMsg = string.IsNullOrWhiteSpace(missingEndpoints) ?
                            $"There isn't any {REQUIREDENDPOINTS} in the .bot file." :
                            $"The .bot file does not have the next {REQUIREDENDPOINTS}: {missingEndpoints}";

                        message = new NotificationMessage(errorMsg, NotificationMessageTypes.Error);
                        messages.Add(message);
                        validationResult = false;
                    }
                }

                // Check if the .bot file does not contain the forbidden endpoints
                if (options.ForbiddenEndpoints.Count() != 0)
                {
                    var forbiddenEndpoints = options.ForbiddenEndpoints;

                    // Checks that there isn't any forbidden endpoint in the required endpoint's list
                    if (options.RequiredEndpoints.Count() != 0)
                    {
                        forbiddenEndpoints = FixForbiddenEndpoints(options.ForbiddenEndpoints, options.RequiredEndpoints, out errorMsg);

                        // If there is at least one repeated endpoint in both required and forbidden list, then creates a warning to notify
                        // that the repeated forbidden Endpoint wont be validated
                        if (!string.IsNullOrWhiteSpace(errorMsg))
                        {
                            message = new NotificationMessage(errorMsg, (int)NotificationMessageTypes.Warning);
                            messages.Add(message);
                        }
                    }

                    // Validates the forbidden endpoints
                    if (!BotValidatorHelper.ValidateEndpoints(botConfiguration, forbiddenEndpoints, false, out missingEndpoints))
                    {
                        // If there is at least one forbidden endpoint in the .bot file, an error will be thrown
                        if (!string.IsNullOrWhiteSpace(missingEndpoints))
                        {
                            errorMsg = $"The .bot file has (but shouldn't) the next {FORBIDDENENDPOINTS}: {missingEndpoints}";

                            message = new NotificationMessage(errorMsg, NotificationMessageTypes.Error);
                            messages.Add(message);
                            validationResult = false;
                        }
                    }
                }

                // Check if the .bot file has a Luis Key
                if (options.RequireLuisKey)
                {
                    if (!BotValidatorHelper.ValidateLuisKey(botConfiguration))
                    {
                        errorMsg = "The .bot file does not have a Luis Key.";
                        message = new NotificationMessage(errorMsg, NotificationMessageTypes.Error);
                        messages.Add(message);
                        validationResult = false;
                    }
                }

                // Check if the .bot file has a qna Key
                if (options.RequireQnAMakerKey)
                {
                    if (!BotValidatorHelper.ValidateQnAKey(botConfiguration))
                    {
                        errorMsg = "The .bot file does not have a QnA Key.";
                        message = new NotificationMessage(errorMsg, NotificationMessageTypes.Error);
                        messages.Add(message);
                        validationResult = false;
                    }
                }

                return validationResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Validate the specified endpoints of a .bot file, according to if they are required or not.
        /// </summary>
        /// <param name="botConfiguration">Bot configuration</param>
        /// <param name="endpoints">Endpoints</param>
        /// <param name="required">Specify if the provided endpoints are required (True) or forbidden (False)</param>
        /// <param name="missingEndpoints">List of the missing endpoints in the bot file. If its null/empty and the methods returns FALSE, it means that there isn't ANY endpoint in the file.</param>
        /// <returns>bool that indicates if a .bot file's endpoints are valid</returns>
        private static bool ValidateEndpoints(BotConfiguration botConfiguration, IEnumerable<string> endpoints, bool required, out string missingEndpoints)
        {
            var missingEndpointsList = new List<string>();

            // Get all the endpoint types in the service list
            var botEndpoints =
                BotValidatorHelper.GetBotServices(botConfiguration, BotServiceType.Endpoint);

            // If there isn't any endpoint, returns false
            if (botEndpoints == null)
            {
                missingEndpoints = string.Empty;
                return false;
            }

            // Checks that all the specified endpoints are/aren't in the bot file (according to the 'required' parameter)
            foreach (var endpoint in endpoints)
            {
                // checks if the bot file contains one of the endpoints provided in the required/forbidden 'endpoints' list
                var existEndpoint = botEndpoints.Any(ep => CompareEndpointsName((EndpointService)ep, endpoint));

                // Compare the previous result (existEndpoint) with the 'required' parameter to check if there are any missing endpoints (required = true
                // and at least one of the provided required endpoints isn't in the .bot file) or if the .bot file has incorrect endpoints (required = false
                // and at least one of the provided forbidden endpoints is in the .bot file), according to the value of 'required'
                if (existEndpoint != required)
                {
                    missingEndpointsList.Add(endpoint.Trim());
                }
            }

            // If there is at least one missing endpoint, the method will return an error message listing all of them
            if (missingEndpointsList.Count() != 0)
            {
                missingEndpoints = string.Join(", ", missingEndpointsList);

                return false;
            }

            missingEndpoints = string.Empty;
            return true;
        }

        /// <summary>
        /// Compare an Endpoint's Name with another provided endpoint
        /// </summary>
        /// <param name="botEndpoint">Bot Endpoint</param>
        /// <param name="providedEndpoint">Endpoint to compare</param>
        /// <returns>bool that indicates if a provided Bot's Endpoint and a String endpoint have the same name</returns>
        private static bool CompareEndpointsName(EndpointService botEndpoint, string providedEndpoint)
        {
            return botEndpoint.Name.Trim() == providedEndpoint.Trim();
        }

        /// <summary>
        /// Checks if the Luis service has specified its key.
        /// </summary>
        /// <param name="botConfiguration">Bot configuration</param>
        /// <returns>bool that indicates if a .bot file's Luis keys are valid</returns>
        private static bool ValidateLuisKey(BotConfiguration botConfiguration)
        {
            try
            {
                // Get all the luis types in the service list
                var luisServices =
                BotValidatorHelper.GetBotServices(botConfiguration, BotServiceType.Luis);

                // If there isn't any Luis service, returns false
                if (luisServices == null)
                {
                    return false;
                }

                // Check if exists any LuisService without its key (SubscriptionKey)
                var existEmptyLuis = luisServices.Any(x => string.IsNullOrWhiteSpace(((LuisService)x).SubscriptionKey));

                // If there is at least one LuisService without its key, the process will return false
                if (existEmptyLuis)
                {
                    return false;
                }

                // Get all the Dispatch (Luis) types in the service list
                var dispatchServices =
                BotValidatorHelper.GetBotServices(botConfiguration, BotServiceType.Dispatch);

                // The dispatch services not appears in all the .bot files of the bot's samples project.
                // Therefore, they only will be validated if the .bot file contains at least one service of this type.
                if (dispatchServices != null && dispatchServices.Count() != 0)
                {
                    // Gets the amount of dispatchServices without its key (SubscriptionKey)
                    var existEmptyDispatch = dispatchServices.Any(x => string.IsNullOrWhiteSpace(((LuisService)x).SubscriptionKey));

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
        /// Checks if the QnA service has specified its key.
        /// </summary>
        /// <param name="botConfiguration">Bot Configuration</param>
        /// <returns>bool that indicates if a .bot's file QNA Keys are valid</returns>
        private static bool ValidateQnAKey(BotConfiguration botConfiguration)
        {
            try
            {
                // Get all the qna types in the service list
                var qnaServices =
                BotValidatorHelper.GetBotServices(botConfiguration, BotServiceType.Qna);

                // If there isn't any qna service, returns false
                if (qnaServices == null)
                {
                    return false;
                }

                var qna = (QnAMakerService)qnaServices.FirstOrDefault();

                // If the qna service does not have a key, returns an error
                if (string.IsNullOrWhiteSpace(qna.SubscriptionKey))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Check if the Project's Name contains white spaces. If there is any white space, will return an error.
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="errorMsg">Error message</param>
        /// <returns>bool that indicate if a Project's Name is valid</returns>
        private static bool ProjectNameIsValid(string folder, out string errorMsg)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            var file = Directory.GetFiles(folder, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (file != null)
            {
                var containsEmptySpaces = Regex.IsMatch(file, @"\s+");

                errorMsg = !containsEmptySpaces ?
                    string.Empty :
                    "The \'.csproj\' file\'s name can NOT have white spaces.";

                return !containsEmptySpaces;
            }

            errorMsg = "There isn't any \'.csproj\' in the specified folder";
            return false;
        }

        /// <summary>
        /// Returns a IEnumerable with a specific type of bot's services contained in the .bot file
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration</param>
        /// <param name="serviceType">Specifies the type of service to return</param>
        /// <returns>List of .bot file' services</returns>
        private static IEnumerable<ConnectedService> GetBotServices(BotConfiguration botConfiguration, BotServiceType serviceType)
        {
            // List that contains the.bot's services, where the endpoints are specified.
            var botServices = botConfiguration.Services;

            // If there isn't any services, returns an error
            if (botServices == null || botServices.Count() == 0)
            {
                return null;
            }

            // Obtains all the endpoints specified in the bot file's services
            var botEndpoints = botServices.Where(service => service.Type.Trim().ToLower() == serviceType.ToString().ToLower());

            // If there isn't any endpoint specified, returns an error
            if (botEndpoints == null || botEndpoints.Count() == 0)
            {
                return null;
            }

            return botEndpoints;
        }

        /// <summary>
        /// Deletes from the forbidden endpoint's list those who also appears in the required list
        /// </summary>
        /// <param name="forbiddenEndpoints">List of forbidden endpoints</param>
        /// <param name="requiredEndpoints">List of required endpoints</param>
        /// <param name="errorMsg">Error message</param>
        /// <returns>Fixed forbidden endpoints</returns>
        private static IEnumerable<string> FixForbiddenEndpoints(IEnumerable<string> forbiddenEndpoints, IEnumerable<string> requiredEndpoints, out string errorMsg)
        {
            try
            {
                // Removes from the forbidden endpoints those who also are required
                var finalForbiddenEP = forbiddenEndpoints.Except(requiredEndpoints);

                // Gets the forbidden endpoints repeated in the required list
                var repeatedForbiddenEP = string.Join(", ", forbiddenEndpoints.Except(finalForbiddenEP));

                errorMsg = string.IsNullOrWhiteSpace(repeatedForbiddenEP) ? string.Empty : $"The next forbidden endpoints won't be checked because they also appear as required:{repeatedForbiddenEP}";

                return finalForbiddenEP;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Load the .bot file from the specified folder
        /// </summary>
        /// <param name="options">Contains the necessary values to Load the .bot file</param>
        /// <returns>The first .bot file in the specified folder</returns>
        private static BotConfiguration LoadFromFolder(ConfigurationOptions options)
        {
            try
            {
                return BotConfiguration.LoadFromFolder(options.ProjectPath, options.Secret);
            }
            catch (Exception ex)
            {
                // If the exception is an ArgumentNullException, it means that the `LoadFromFolder` method failed because a project path was not provided
                // If the exception is an FileNotFoundException, it means that there where no .bot files in the folder
                if (ex is ArgumentNullException || ex is FileNotFoundException)
                {
                    throw;
                }
                else
                {
                    // The last kind of exception that we should catch are the one related to the missing `secret` parameter of the `LoadFromFolder` method, which is necessary when the .bot file was encrypted.
                    throw new Exception("Error: A `SECRET` is needed to access the .bot file. Provide it setting the property \'AppSecret\' in your \'.csproj\' file as stated in README.md.");
                }
            }
        }
    }
}

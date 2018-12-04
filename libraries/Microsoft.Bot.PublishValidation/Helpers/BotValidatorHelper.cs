using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.PublishValidation;

namespace Microsoft.Bot.PublishValidation
{
    public enum BotServiceType
    {
        endpoint,
        luis,
        qna
    }

    public class BotValidatorHelper
    {
        private const string FORBIDDEN_ENDPOINTS = "Forbidden Endpoints";
        private const string REQUIRED_ENDPOINTS = "Required Endpoints";

        /// <summary>
        /// Checks if a bot file is valid or not according to some configuration options
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="configurationOptions"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static bool BotFileIsValid(string folder, ConfigurationOptions configurationOptions, ref IEnumerable<NotificationMessage> messages)
        {
            try
            {
                // Load the first .bot file from the provided folder.
                // Also validates its existence (throws an exception is there isn't any file)
                BotConfiguration botConfiguration = BotValidatorHelper.LoadFromFolder(folder);

                return BotValidatorHelper.ValidateBotFile(botConfiguration, configurationOptions, folder, ref messages);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Performs the validation of the specified .bot file, according the specified validation options
        /// </summary>
        /// <param name="botConfiguration"></param>
        /// <param name="options"></param>
        /// <param name="folder"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        private static bool ValidateBotFile(BotConfiguration botConfiguration, ConfigurationOptions options, string folder, ref IEnumerable<NotificationMessage> messages)
        {
            try
            {
                NotificationMessage message;

                string errorMsg = string.Empty;
                bool validationResult = true;
                string missingEndpoints = string.Empty;

                // Checks if the project name which uses the .bot file contains white spaces on its name.
                // If it has at least one white space, the process will fail and return an error.
                if (options.ForbidSpacesInProjectName)
                {
                    if (!BotValidatorHelper.ProjectNameIsValid(folder, out errorMsg))
                    {
                        message = new NotificationMessage(errorMsg, (int)NotificationMessageTypes.Error);
                        messages = messages.Append(message);
                        validationResult = false;
                    }
                }

                if (!options.RequireBotFile)
                {
                    return validationResult;
                }

                // Check if the .bot file contains the specified endpoints
                if (!string.IsNullOrWhiteSpace(options.RequiredEndpoints))
                {
                    if (!BotValidatorHelper.ValidateEndpoints(botConfiguration, options.RequiredEndpoints, true, out missingEndpoints))
                    {
                        errorMsg = string.IsNullOrWhiteSpace(missingEndpoints) ?
                            $"There isnt't any { REQUIRED_ENDPOINTS } in the .bot file.\n" :
                            $"The .bot file does not have the next { REQUIRED_ENDPOINTS }: { missingEndpoints }";

                        message = new NotificationMessage(errorMsg, (int)NotificationMessageTypes.Error);
                        messages = messages.Append(message);
                        validationResult = false;
                    }
                }

                // Check if the .bot file does not contain the forbidded endpoints
                if (!string.IsNullOrWhiteSpace(options.ForbiddenEndpoints))
                {
                    string forbiddenEndpoints = options.ForbiddenEndpoints;

                    // Checks that there isn't any forbidden endpoint in the required endpoint's list
                    if (!string.IsNullOrWhiteSpace(options.RequiredEndpoints))
                    {
                        forbiddenEndpoints = FixForbiddenEndpoints(options.ForbiddenEndpoints, options.RequiredEndpoints, out errorMsg);

                        // If there is at least one repeated endpoint in both required and forbidden list, then creates a warning to notify
                        // that the repeated forbidden Endpoint wont be validated
                        if (!string.IsNullOrWhiteSpace(errorMsg))
                        {
                            message = new NotificationMessage(errorMsg, (int)NotificationMessageTypes.Warning);
                            messages = messages.Append(message);
                        }
                    }

                    // Validates the forbidden endpoints
                    if (!BotValidatorHelper.ValidateEndpoints(botConfiguration, forbiddenEndpoints, false, out missingEndpoints))
                    {
                        errorMsg = string.IsNullOrWhiteSpace(missingEndpoints) ?
                            $"There isnt't any { FORBIDDEN_ENDPOINTS } in the .bot file." :
                            $"The .bot file has (but shouldn't) the next { FORBIDDEN_ENDPOINTS }: { missingEndpoints }";

                        message = new NotificationMessage(errorMsg, (int)NotificationMessageTypes.Error);
                        messages = messages.Append(message);
                        validationResult = false;
                    }
                }

                // Check if the .bot file has a Luis Key
                if (options.RequireLuisKey)
                {
                    if (!BotValidatorHelper.ValidateLuisKey(botConfiguration))
                    {
                        errorMsg = "The .bot file does not have a Luis Key.";
                        message = new NotificationMessage(errorMsg, (int)NotificationMessageTypes.Error);
                        messages = messages.Append(message);
                        validationResult = false;
                    }
                }

                // Check if the .bot file has a qna Key
                if (options.RequireLuisKey)
                {
                    if (!BotValidatorHelper.ValidateQnAKey(botConfiguration))
                    {
                        errorMsg = "The .bot file does not have a QnA Key.";
                        message = new NotificationMessage(errorMsg, (int)NotificationMessageTypes.Error);
                        messages = messages.Append(message);
                        validationResult = false;
                    }
                }

                return validationResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Validate the specified endpoints of a .bot file, according to if they are required or not.
        /// </summary>
        /// <param name="botConfiguration"></param>
        /// <param name="specifiedEndpoints"></param>
        /// <param name="required"></param>
        /// <missingEndpoints name="missingEndpoints">List of the missing endpoints in the bot file. If its null/empty and the methods returns FALSE, it means that there isn't ANY endpoint in the file.</missingEndpoints>
        /// <returns></returns>
        private static bool ValidateEndpoints(BotConfiguration botConfiguration, string specifiedEndpoints, bool required, out string missingEndpoints)
        {
            List<string> endpoints = specifiedEndpoints.Trim().Split(',').Select(ep => ep.Trim()).ToList();
            List<string> missingEndpointsList = new List<string>();

            // Get all the endpoint types in the service list
            IEnumerable<ConnectedService> botEndpoints =
                BotValidatorHelper.GetBotServices(botConfiguration, BotServiceType.endpoint.ToString());

            // If there isn't any endpoint, returns false
            if (botEndpoints == null)
            {
                missingEndpoints = string.Empty;
                return false;
            }

            // Checks that all the specified endpoints are/aren't in the bot file
            foreach (var endpoint in endpoints)
            {
                if (botEndpoints.Any(ep => ((EndpointService)ep).Name.Trim() == endpoint.Trim()) != required)
                {
                    missingEndpointsList.Add(endpoint.Trim());
                }
            }

            // If there is at least one missing endpoint, the method will return an error message listing all of them
            if (missingEndpointsList.Count() != 0)
            {
                missingEndpoints = string.Join("\n\t*", missingEndpointsList);
                missingEndpoints = "\n\t*" + missingEndpoints;
                return false;
            }

            missingEndpoints = string.Empty;
            return true;
        }

        /// <summary>
        /// Checks if the Luis service has specified its key.
        /// </summary>
        /// <param name="botConfiguration"></param>
        /// <returns></returns>
        private static bool ValidateLuisKey(BotConfiguration botConfiguration)
        {
            try
            {
                // Get all the luis types in the service list
                IEnumerable<ConnectedService> luisServices =
                BotValidatorHelper.GetBotServices(botConfiguration, BotServiceType.luis.ToString());

                // If there isn't any Luis service, returns false
                if (luisServices == null)
                    return false;

                LuisService luis = (LuisService)luisServices.FirstOrDefault();

                // If the luis service does not have a key, returns an error
                if (string.IsNullOrWhiteSpace(luis.SubscriptionKey))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Checks if the QnA service has specified its key.
        /// </summary>
        /// <param name="botConfiguration"></param>
        /// <returns></returns>
        private static bool ValidateQnAKey(BotConfiguration botConfiguration)
        {
            try
            {
                // Get all the qna types in the service list
                IEnumerable<ConnectedService> qnaServices =
                BotValidatorHelper.GetBotServices(botConfiguration, BotServiceType.qna.ToString());

                // If there isn't any qna service, returns false
                if (qnaServices == null)
                    return false;

                QnAMakerService qna = (QnAMakerService)qnaServices.FirstOrDefault();

                // If the qna service does not have a key, returns an error
                if (string.IsNullOrWhiteSpace(qna.SubscriptionKey))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Check if the Project's Name contains white spaces. If there is any white space, will return an error.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
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
        /// Returns a IEnumerable containing and specific type of bot's services contained in the .bot file
        /// </summary>
        /// <param name="botConfiguration">BotConfiguration</param>
        /// <param name="serviceType">Specifies the type of service to return</param>
        /// <returns></returns>
        private static IEnumerable<ConnectedService> GetBotServices(BotConfiguration botConfiguration, string serviceType)
        {
            // List that contains the.bot's services, where the endpoints are specified.
            var botServices = botConfiguration.Services;

            // If there isn't any services, returns an error
            if (botServices == null || botServices.Count() == 0)
                return null;

            // Obtains all the endpoints specified in the bot file's services
            var botEndpoints = botServices.Where(service => service.Type.Trim().ToLower() == serviceType);

            // If there isn't any endpoint specified, returns an error
            if (botEndpoints == null || botEndpoints.Count() == 0)
                return null;

            return botEndpoints;
        }

        /// <summary>
        /// Deletes from the forbidden endpoint's list those who also appears in the required list
        /// </summary>
        /// <param name="forbiddenEndpoints"></param>
        /// <param name="requiredEndpoints"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static string FixForbiddenEndpoints(string forbiddenEndpoints, string requiredEndpoints, out string errorMsg)
        {
            try
            {
                IEnumerable<string> forbiddenEP = forbiddenEndpoints.Trim().Split(',').Select(ep => ep.Trim()).ToList();
                IEnumerable<string> requiredEP = requiredEndpoints.Trim().Split(',').Select(ep => ep.Trim()).ToList();

                // Removes from the forbidden endpoints those who also are required
                string finalForbiddenEP = string.Join(",", forbiddenEP.Except(requiredEP));

                // Gets the forbidden endpoints repeated in the required list
                string repeteadForbiddenEP = string.Join("\n\t*", forbiddenEP.Except(forbiddenEP.Except(requiredEP)));

                repeteadForbiddenEP = "\n\t*" + repeteadForbiddenEP;

                errorMsg = $"The next forbidden endpoints won't be checked because they also appear as required:{ repeteadForbiddenEP }";

                return finalForbiddenEP;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Load the .bot file from the specified folder
        /// </summary>
        /// <param name="folder">The folder where the .bot file is located</param>
        /// <param name="secret">The path of the secret file to decrypt the .bot file</param>
        /// <returns>The first .bot file in the specified folder</returns>
        private static BotConfiguration LoadFromFolder(string folder, string secret = null)
        {
            try
            {
                return BotConfiguration.LoadFromFolder(folder, secret);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

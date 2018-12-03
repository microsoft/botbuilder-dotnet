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
        private const string forbiddenEndpoints = "Forbidden Endpoints";
        private const string requiredEndpoints = "Required Endpoints";

        /// <summary>
        /// Checks if a bot file is valid or not according to some configuration options
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="configurationOptions"></param>
        /// <returns></returns>
        public static bool BotFileIsValid(string folder, ConfigurationOptions configurationOptions, out string errorMsg)
        {
            try
            {
                errorMsg = string.Empty;                

                // Load the first .bot file from the provided folder.
                // Also validates its existence (throws an exception is there isn't any file)
                BotConfiguration botConfiguration = BotValidatorHelper.LoadFromFolder(folder);

                return BotValidatorHelper.ValidateBotFile(botConfiguration, configurationOptions, folder, out errorMsg);
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
        /// <returns></returns>
        private static bool ValidateBotFile(BotConfiguration botConfiguration, ConfigurationOptions options, string folder, out string errorMsg)
        {
            try
            {
                errorMsg = string.Empty;
                bool validationResult = true;
                string missingEndpoints = string.Empty;

                // Checks if the project name which uses the .bot file contains white spaces on its name.
                // If it has at least one white space, the process will fail and return an error.
                if (options.ForbidSpacesInProjectName)
                {
                    if (!BotValidatorHelper.ProjectNameIsValid(folder, out errorMsg))
                    {
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
                        errorMsg += string.IsNullOrWhiteSpace(missingEndpoints) ?
                            $"There isnt't any { requiredEndpoints } in the .bot file.\n" :
                            $"The .bot file does not have the next { requiredEndpoints }: { missingEndpoints }.\n";
                        
                        validationResult = false;
                    }
                }

                // Check if the .bot file does not contain the forbidded endpoints
                if (!string.IsNullOrWhiteSpace(options.ForbiddenEndpoints))
                {
                    if (!BotValidatorHelper.ValidateEndpoints(botConfiguration, options.ForbiddenEndpoints, false, out missingEndpoints))
                    {
                        errorMsg += string.IsNullOrWhiteSpace(missingEndpoints) ? 
                            $"There isnt't any { forbiddenEndpoints } in the .bot file.\n" :
                            $"The .bot file have (but shouldn't have) the next { forbiddenEndpoints }: { missingEndpoints }.\n";

                        validationResult = false;
                    }
                }

                // Check if the .bot file has a Luis Key
                if (options.RequireLuisKey)
                {
                    if (!BotValidatorHelper.ValidateLuisKey(botConfiguration))
                    {
                        errorMsg += "The .bot file does not have a Luis Key.\n";

                        validationResult = false;
                    }
                }

                // Check if the .bot file has a qna Key
                if (options.RequireLuisKey)
                {
                    if (!BotValidatorHelper.ValidateQnAKey(botConfiguration))
                    {
                        errorMsg += "The .bot file does not have a QnA Key.\n";

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
            List<string> endpoints = specifiedEndpoints.Trim().Split(',').ToList();
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
                if (botEndpoints.Any(ep => ((EndpointService)ep).Name == endpoint) != required)
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

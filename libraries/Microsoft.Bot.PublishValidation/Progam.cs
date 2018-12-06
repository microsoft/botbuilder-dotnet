// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.PublishValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        // Return codes
        private const int ERROR = 2;
        private const int OK = 0;

        //dotnet publishValidations.dll
        //  -ProjectPath C:\my-project\
        //  -AllowSpacesInProjectName
        //  -NotRequireBotFile
        //  -RequireEndpoints Production,Production2
        //  -ForbidEndpoints Dev,Test
        //  -RequireLuisKey
        //  -RequireQnAMakerKey
        public static int Main(string[] args)
        {
            try
            {
                var errorMsg = string.Empty;

                var options = ConfigurationParser.ParseConfiguration(args);
                
                List<NotificationMessage> messages = new List<NotificationMessage>();

                var validationResult = BotValidatorHelper.BotFileIsValid(options, messages);

                if(!validationResult)
                {
                    Console.WriteLine(GetErrorMessage(messages));
                }

                return validationResult ? OK : ERROR;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ERROR;
            }
        }

        private static string GetErrorMessage(IEnumerable<NotificationMessage> messages)
        {
            string errorMessage = string.Empty;

            foreach (var message in messages)
            {
                errorMessage += ((NotificationMessage)message).ToString() + "\n";
            }
            
            return errorMessage;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.PublishValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ConfigurationParser
    {
        private const string PROJECTPATH = "-ProjectPath";
        private const string APPSECRET = "-AppSecret";
        private const string ALLOWSPACESINPROJECTNAME = "-AllowSpacesInProjectName";
        private const string NOTREQUIREBOTFILE = "-NotRequireBotFile";
        private const string REQUIREENDPOINTS = "-RequireEndpoints";
        private const string FORBIDENDPOINTS = "-ForbidEndpoints";
        private const string REQUIRELUISKEY = "-RequireLuisKey";
        private const string REQUIREQNAMAKERKEY = "-RequireQnAMakerKey";

        public static ConfigurationOptions ParseConfiguration(string[] options)
        {
            try
            {
                var configurationOptions = new ConfigurationOptions
                {
                    // Parse the PROJECT PATH
                    ProjectPath = options.Contains(PROJECTPATH) ? GetOptionValue(options, PROJECTPATH) : string.Empty,

                    // Parse the AppSecret Path
                    Secret = options.Contains(APPSECRET) ? GetOptionValue(options, APPSECRET) : string.Empty,

                    // If the option 'AllowSpacesInProjectName' is present, the process won't validated if the project's name has white spaces
                    ForbidSpacesInProjectName = options.Contains(ALLOWSPACESINPROJECTNAME) ? false : true,

                    // If the option 'NotRequireBotFile' is present, the process won't validated if the project has a bot file. Also, the remaining validations will be skipped.
                    RequireBotFile = options.Contains(NOTREQUIREBOTFILE) ? false : true,

                    // Parse the parameters related to the option RequireEndpoints to a IEnumerable<String>. If there isn't any option, the default value will be "production"
                    RequiredEndpoints = options.Contains(REQUIREENDPOINTS) ? GetOptionValue(options, REQUIREENDPOINTS).Split(',').Select(endpoint => endpoint.Trim()) : new List<string>() { Endpoints.Production.ToString() },

                    // Parse the parameters related to the option ForbidEndpoints to a IEnumerable<String>
                    ForbiddenEndpoints = options.Contains(FORBIDENDPOINTS) ? GetOptionValue(options, FORBIDENDPOINTS).Split(',').Select(endpoint => endpoint.Trim()) : new List<string>(),

                    // Check if the configuration will require the LUIS KEY
                    RequireLuisKey = options.Contains(REQUIRELUISKEY) ? true : false,

                    // Check if the configuration will require the QNA MAKER KEY
                    RequireQnAMakerKey = options.Contains(REQUIREQNAMAKERKEY) ? true : false
                };

                return configurationOptions;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GetOptionValue(string[] options, string optionParameter)
        {
            return options[options.ToList().IndexOf(optionParameter) + 1];
        }
    }
}

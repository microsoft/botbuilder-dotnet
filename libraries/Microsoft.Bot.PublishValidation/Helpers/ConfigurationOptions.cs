using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.PublishValidation
{
    public class ConfigurationOptions
    {
        public bool ForbidSpacesInProjectName { get; set; }
        public bool RequireBotFile { get; set; }
        public string RequiredEndpoints { get; set; }
        public string ForbiddenEndpoints { get; set; }
        public bool RequireLuisKey { get; set; }
        public bool RequireQnAMakerKey { get; set; }

        public ConfigurationOptions(string ForbidSpacesInProjectName, string RequireBotFile,
            string RequiredEndpoints, string ForbidedEndpoints,
            string RequireLuisKey, string RequireQnAMakerKey)
        {
            this.ForbidSpacesInProjectName = ParseConfigOption(ForbidSpacesInProjectName, true);
            this.RequireBotFile = ParseConfigOption(RequireBotFile, true);
            this.RequireLuisKey = ParseConfigOption(RequireLuisKey, true);
            this.RequireQnAMakerKey = ParseConfigOption(RequireQnAMakerKey, true);

            this.ForbiddenEndpoints = ForbidedEndpoints;
            this.RequiredEndpoints = RequiredEndpoints;
        }

        private bool ParseConfigOption(string configOption, bool defaultOption)
        {
            bool result = false;

            if (bool.TryParse(configOption.ToLower(), out result))
                return result;

            return defaultOption;
        }
    }
}

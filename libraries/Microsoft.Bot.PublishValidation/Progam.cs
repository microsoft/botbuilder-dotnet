namespace Microsoft.Bot.PublishValidation
{
    using System;
    using Microsoft.Bot.Configuration;

    class Program
    {
        // Return codes
        private const int ERROR = 2;
        private const int OK = 0;

        public static int Main(string[] args)
        {
            Console.WriteLine("Args: " + string.Join(", ", args));

            var projectPath = args[0].ToString();
            var requireEndpoints = args[1].ToString();
            var forbidEndpoints = args[2].ToString();
            var forbidSpacesInProjectName = args[3].ToString();
            var requireBotFile = args[4].ToString();
            var requireLuisKey = args[5].ToString();
            var requireQnAMakerKey = args[6].ToString();

            try
            {
                string errorMsg = string.Empty;

                ConfigurationOptions options = new ConfigurationOptions(
                        forbidSpacesInProjectName,
                        requireBotFile,
                        requireEndpoints,
                        forbidEndpoints,
                        requireLuisKey,
                        requireQnAMakerKey
                    );

                bool validationResult = BotValidatorHelper.BotFileIsValid(projectPath, options, out errorMsg);

                if(!validationResult)
                {
                    Console.WriteLine($"Errors found:\n{ errorMsg }");
                }

                return validationResult ? OK : ERROR;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ERROR;
            }
        }
    }
}

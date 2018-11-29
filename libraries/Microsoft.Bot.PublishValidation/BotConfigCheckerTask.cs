using Microsoft.Build.Framework;
using TaskBuilder.Helpers;

namespace Microsoft.Bot.PublishValidation
{
    public class BotConfigCheckerTask : Microsoft.Build.Utilities.Task
    {
        public string ProjectPath { get; set; }

        public override bool Execute()
        {
            LoggerHelper logHelper = new LoggerHelper(Log.LogError, Log.LogWarning);

            string resultMsg = string.Empty;
            int logType = 0;

            // Validate if the Project directory path is valid
            if (!DirectoryValidatorHelper.DirectoryIsValid(ProjectPath, out resultMsg, out logType))
            {
                logHelper.Log(resultMsg, logType);
                return false;
            }

            // Validate if there is any .bot file inside the Project Directory 
            if (!DirectoryValidatorHelper.FileExists(ProjectPath, "*.bot", out resultMsg, out logType))
            {
                logHelper.Log(resultMsg, logType);
                return false;
            }

            return true;
        }
    }
}

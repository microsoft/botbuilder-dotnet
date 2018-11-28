using Microsoft.Build.Framework;
using TaskBuilder.Helpers;

namespace TaskBuilder
{
    public class ValidationTask : Microsoft.Build.Utilities.Task
    {
        public string DirectoryPath { get; set; }
        
        public override bool Execute()
        {
            LoggerHelper logHelper = new LoggerHelper(Log.LogError, Log.LogWarning);
            
            string resultMsg = string.Empty;
            int logType = 0;

            // Validate if the Project directory path is valid
            if (!DirectoryValidatorHelper.DirectoryIsValid(DirectoryPath, out resultMsg, out logType))
            {
                logHelper.Log(resultMsg, logType);
                return false;
            }
            
            // Validate if in the Project Directory exists any .bot file
            if(!DirectoryValidatorHelper.FileExists(DirectoryPath, "*.bot", out resultMsg, out logType))
            {
                logHelper.Log(resultMsg, logType);
                return false;
            }

            return true;
        }
    }
}

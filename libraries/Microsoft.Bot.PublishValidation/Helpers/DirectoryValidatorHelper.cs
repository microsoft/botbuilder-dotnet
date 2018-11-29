using System.IO;

namespace TaskBuilder.Helpers
{
    public class DirectoryValidatorHelper
    {
        public static bool DirectoryIsValid(string directoryPath, out string resultMsg, out int logtype)
        {
            
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                resultMsg = "You did not provide a DirectoryPath.";
                logtype = (int)LogType.Error;
                return false;
            }
            
            if (!Directory.Exists(directoryPath) && !string.IsNullOrWhiteSpace(directoryPath))
            {
                resultMsg = string.Format("The directory \'{0}\' does not exists.", directoryPath);
                logtype = (int)LogType.Error;
                return false;
            }

            resultMsg = string.Empty;
            logtype = (int)LogType.Error;
            return true;
        }

        public static bool FileExists(string basePath, string fileName, out string resultMsg, out int logtype)
        {
            string fullPath = Path.Combine(basePath, fileName);

            if (Directory.GetFiles(basePath, fileName).Length <= 0)
            {
                resultMsg = string.Format("The file \'{0}\' does not exists in the directory \'{1}\'.", fileName, basePath);
                logtype = (int)LogType.Error;
                return false;
            }

            resultMsg = string.Empty;
            logtype = (int)LogType.Error;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class LGParsingException : Exception
    {
        public LGParsingException(string message)
            : base(message)
        {

        }

    }

    public class LGEvaluatingException : Exception
    {
        public LGEvaluatingException(string message)
            : base(message)
        {

        }

    }

    public class LGReportMessage
    {
        public string ReportType { get; set; }
        public string Message { get; set; }

        public LGReportMessage(string message, string reportType = LGReportMessageType.Error)
        {
            Message = message;
            ReportType = reportType;
        }
    }

    public static class LGReportMessageType
    {
        public const string Error = "ERROR";
        public const string WARN = "WARN";
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Tests
{
    public class TestLogger : ILogger
    {
        public TestLogger()
        {
            this.LogAction = null;
        }

        public Action<LogLevel, EventId, object, Exception> LogAction { get; set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            this.LogAction?.Invoke(logLevel, eventId, state, exception);
        }
    }
}

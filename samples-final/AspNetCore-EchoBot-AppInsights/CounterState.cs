// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AspNetCore_EchoBot_With_AppInsights
{
    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class CounterState
    {
        public int TurnCount { get; set; } = 0;
    }
}

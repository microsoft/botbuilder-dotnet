// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Microsoft.Bot.Builder.AI.Luis.Tests
{
    public class TelemetryConvertResult : IRecognizerConvert
    {
        private RecognizerResult _result;

        public TelemetryConvertResult()
        {
        }

        /// <summary>
        /// Convert recognizer result.
        /// </summary>
        /// <param name="result">Result to convert.</param>
        public void Convert(dynamic result)
        {
            _result = result as RecognizerResult;
        }
    }
}

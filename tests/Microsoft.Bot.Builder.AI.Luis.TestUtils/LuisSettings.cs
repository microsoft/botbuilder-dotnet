// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.Luis.TestUtils
{
    public class LuisSettings
    {
        // By default (when the Mocks are being used), the subscription key used can be any GUID. Only if the tests
        // are connecting to LUIS is an actual key needed.
        // NOTE: DO NOT REMOVE THIS APP ID or ENDPOINT.  It points to the server model used for updating these tests.
        public string AppId { get; set; } = "38d43f13-3e8d-45f3-b23f-ef8a0b9d98ac";

        public string Endpoint { get; set; } = "https://westus.api.cognitive.microsoft.com";

        public string Key { get; set; }

        // LUIS tests run off of recorded HTTP responses to avoid service dependencies.
        // To update the recorded responses:
        // 1) Change Mock to false below, or set LUISMOCK=false in your environment
        // 2) Set environment variable LUISSUBSCRIPTIONKEY = any valid LUIS endpoint key
        // 3) Run the LuisRecognizerTests
        // 4) If the http responses have changed there will be a file in this directory of<test>.json.new
        // 5) Run the review.cmd file to review each file if approved the new oracle file will replace the old one.
        // Changing this to false will cause running against the actual LUIS service.
        // This is useful in order to see if the oracles for mocking or testing have changed.
        public bool Mock { get; set; } = true;

        public void GetEnvironmentVars()
        {
            if (string.IsNullOrWhiteSpace(AppId))
            {
                AppId = Environment.GetEnvironmentVariable("LUISAPPID");
            }

            if (string.IsNullOrWhiteSpace(AppId))
            {
                throw new Exception("Environment variable 'LuisAppId' not found.");
            }

            if (string.IsNullOrWhiteSpace(Key))
            {
                Key = Environment.GetEnvironmentVariable("LUISSUBSCRIPTIONKEY");
            }

            if (string.IsNullOrWhiteSpace(Key))
            {
                Key = Guid.Empty.ToString();
            }

            if (string.IsNullOrWhiteSpace(Endpoint))
            {
                Endpoint = Environment.GetEnvironmentVariable("LUISENDPOINT");
            }

            if (string.IsNullOrWhiteSpace(Endpoint))
            {
                throw new Exception("Environment variable 'LuisEndPoint' not found.");
            }

            var mock = Environment.GetEnvironmentVariable("LUISMOCK");
            if (mock != null)
            {
                Mock = bool.Parse(mock);
            }
        }
    }
}

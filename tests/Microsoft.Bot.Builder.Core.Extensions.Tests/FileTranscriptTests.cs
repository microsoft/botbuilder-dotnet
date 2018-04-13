// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
{
    [TestClass]
    public class FileTranscriptTests : TranscriptBaseTests
    {
        private static string path;

        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            path = Path.Combine(Environment.GetEnvironmentVariable("temp"), "FileTranscriptTest");
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch (IOException) { }
        }

        public FileTranscriptTests() : base()
        {
            this.store = new FileTranscriptStore(path);
        }

        [TestMethod]
        public async Task FileTranscript_BadArgs()
        {
            await base._BadArgs();
        }

        [TestMethod]
        public async Task FileTranscript_LogActivity()
        {
            await base._LogActivity();
        }

        [TestMethod]
        public async Task FileTranscript_LogMultipleActivities()
        {
            await base._LogMultipleActivities();
        }


        [TestMethod]
        public async Task FileTranscript_GetTranscriptActivities()
        {
            await base._GetTranscriptActivities();
        }

        [TestMethod]
        public async Task FileTranscript_GetTranscriptActivitiesStartDate()
        {
            await base._GetTranscriptActivitiesStartDate();
        }

        [TestMethod]
        public async Task FileTranscript_ListTranscripts()
        {
            await base._ListTranscripts();
        }

        [TestMethod]
        public async Task FileTranscript_DeleteTranscript()
        {
            await base._DeleteTranscript();
        }
    }
}
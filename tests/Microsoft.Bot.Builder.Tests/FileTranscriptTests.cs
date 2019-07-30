// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class FileTranscriptTests : TranscriptBaseTests
    {
        public static string Folder { get { return Path.Combine(Path.GetTempPath(), nameof(FileTranscriptTests)); } }

        [ClassInitialize]

        public static void ClassInitialize(TestContext context)
        {
            if (Directory.Exists(Folder))
            {
                Directory.Delete(Folder, true);
            }
        }

        public FileTranscriptTests()
            : base()
        {
            this.Store = new FileTranscriptLogger(Folder);
        }

        [TestMethod]
        public async Task FileTranscript_BadArgs()
        {
            await BadArgs();
        }

        [TestMethod]
        public async Task FileTranscript_LogActivity()
        {
            await LogActivity();
        }

        [TestMethod]
        public async Task FileTranscript_LogMultipleActivities()
        {
            await LogMultipleActivities();
        }

        [TestMethod]
        public async Task FileTranscript_GetConversationActivities()
        {
            await GetTranscriptActivities();
        }

        [TestMethod]
        public async Task FileTranscript_GetConversationActivitiesStartDate()
        {
            await GetTranscriptActivitiesStartDate();
        }

        [TestMethod]
        public async Task FileTranscript_ListConversations()
        {
            await ListTranscripts();
        }

        [TestMethod]
        public async Task FileTranscript_DeleteConversation()
        {
            await DeleteTranscript();
        }
    }
}

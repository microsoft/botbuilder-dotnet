// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class MemoryTranscriptTests : TranscriptBaseTests
    {
        public MemoryTranscriptTests()
            : base()
        {
            this.Store = new MemoryTranscriptStore();
        }

        [TestMethod]
        public async Task MemoryTranscript_BadArgs()
        {
            await BadArgs();
        }

        [TestMethod]
        public async Task MemoryTranscript_LogActivity()
        {
            await LogActivity();
        }

        [TestMethod]
        public async Task MemoryTranscript_LogMultipleActivities()
        {
            await LogMultipleActivities();
        }

        [TestMethod]
        public async Task MemoryTranscript_GetConversationActivities()
        {
            await GetTranscriptActivities();
        }

        [TestMethod]
        public async Task MemoryTranscript_GetConversationActivitiesStartDate()
        {
            await GetTranscriptActivitiesStartDate();
        }

        [TestMethod]
        public async Task MemoryTranscript_ListConversations()
        {
            await ListTranscripts();
        }

        [TestMethod]
        public async Task MemoryTranscript_DeleteConversation()
        {
            await DeleteTranscript();
        }
    }
}

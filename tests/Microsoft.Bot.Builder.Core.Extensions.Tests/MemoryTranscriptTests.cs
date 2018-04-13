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
    public class MemoryTranscriptTests : TranscriptBaseTests
    {
        public MemoryTranscriptTests() : base()
        {
            this.store = new MemoryTranscriptStore();
        }

        [TestMethod]
        public async Task MemoryTranscript_BadArgs()
        {
            await base._BadArgs();
        }

        [TestMethod]
        public async Task MemoryTranscript_LogActivity()
        {
            await base._LogActivity();
        }

        [TestMethod]
        public async Task MemoryTranscript_LogMultipleActivities()
        {
            await base._LogMultipleActivities();
        }

        [TestMethod]
        public async Task MemoryTranscript_GetConversationActivities()
        {
            await base._GetTranscriptActivities();
        }

        [TestMethod]
        public async Task MemoryTranscript_GetConversationActivitiesStartDate()
        {
            await base._GetTranscriptActivitiesStartDate();
        }

        [TestMethod]
        public async Task MemoryTranscript_ListConversations()
        {
            await base._ListTranscripts();
        }

        [TestMethod]
        public async Task MemoryTranscript_DeleteConversation()
        {
            await base._DeleteTranscript();
        }

    }
}
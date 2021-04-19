// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class FileTranscriptTests : TranscriptBaseTests
    {
        public FileTranscriptTests()
            : base()
        {
            this.Store = new FileTranscriptLogger(Folder);
            var folder = Path.Combine(Path.GetTempPath(), nameof(FileTranscriptTests));
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        public static string Folder
        {
            get { return Path.Combine(Path.GetTempPath(), nameof(FileTranscriptTests)); }
        }

        [Fact]
        public async Task FileTranscript_BadArgs()
        {
            await BadArgs();
        }

        [Fact]
        public async Task FileTranscript_LogActivity()
        {
            await LogActivity();
        }

        [Fact]
        public async Task FileTranscript_LogActivityWithInvalidIds()
        {
            await LogActivityWithInvalidIds();
        }

        [Fact]
        public async Task FileTranscript_LogMultipleActivities()
        {
            await LogMultipleActivities();
        }

        [Fact]
        public async Task FileTranscript_GetConversationActivities()
        {
            await GetTranscriptActivities();
        }

        [Fact]
        public async Task FileTranscript_GetConversationActivitiesStartDate()
        {
            await GetTranscriptActivitiesStartDate();
        }

        [Fact]
        public async Task FileTranscript_ListConversations()
        {
            await ListTranscripts();
        }

        [Fact]
        public async Task FileTranscript_DeleteConversation()
        {
            await DeleteTranscript();
        }
    }
}

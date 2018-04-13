// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class BlobTranscriptTests : TranscriptBaseTests
    {
        private static string emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\azurestorageemulator.exe");
        private const string noEmulatorMessage = "This test requires Azure Storage Emulator! go to https://go.microsoft.com/fwlink/?LinkId=717179 to download and install.";

        private static Lazy<bool> hasStorageEmulator = new Lazy<bool>(() =>
        {
            if (File.Exists(emulatorPath))
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = emulatorPath;
                p.StartInfo.Arguments = "status";
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return output.Contains("IsRunning: True");
            }
            return false;
        });

        public bool CheckStorageEmulator()
        {
            if (!hasStorageEmulator.Value)
                System.Diagnostics.Debug.WriteLine(noEmulatorMessage);
            if (System.Diagnostics.Debugger.IsAttached)
                Assert.IsTrue(hasStorageEmulator.Value, noEmulatorMessage);
            return hasStorageEmulator.Value;
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var containerName = "BlobTranscriptTests".ToLower();
            var blobClient = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            container.DeleteAsync();
        }

        public BlobTranscriptTests() : base()
        {
            this.store = new AzureBlobTranscriptStore(CloudStorageAccount.DevelopmentStorageAccount, "BlobTranscriptTests");
        }

        [TestMethod]
        public async Task BlobTranscript_BadArgs()
        {
            if (CheckStorageEmulator())
                await base._BadArgs();
        }

        [TestMethod]
        public async Task BlobTranscript_LogActivity()
        {
            if (CheckStorageEmulator())
                await base._LogActivity();
        }

        [TestMethod]
        public async Task BlobTranscript_LogMultipleActivities()
        {
            if (CheckStorageEmulator())
                await base._LogMultipleActivities();
        }

        [TestMethod]
        public async Task BlobTranscript_GetConversationActivities()
        {
            if (CheckStorageEmulator())
                await base._GetTranscriptActivities();
        }

        [TestMethod]
        public async Task BlobTranscript_GetConversationActivitiesStartDate()
        {
            if (CheckStorageEmulator())
                await base._GetTranscriptActivitiesStartDate();
        }

        [TestMethod]
        public async Task BlobTranscript_ListConversations()
        {
            if (CheckStorageEmulator())
                await base._ListTranscripts();
        }

        [TestMethod]
        public async Task BlobTranscript_DeleteConversation()
        {
            if (CheckStorageEmulator())
                await base._DeleteTranscript();
        }

    }
}
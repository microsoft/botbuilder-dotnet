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

        public bool HasStorage()
        {
            return this.store != null;
        }

        public static CloudStorageAccount cloudStorageAccount;
        public static string containerName;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            containerName = nameof(BlobTranscriptTests).ToLower() + Guid.NewGuid().ToString("n");
            cloudStorageAccount = (hasStorageEmulator.Value) ? CloudStorageAccount.DevelopmentStorageAccount : null;

            // The commented out code below allows the tests to run against actual Azure Blobs
            // rather than the local emulator. We used to have this enabled to run on our
            // build servers, but hitting network resources as part of automated builds is problematic
            // so it's been commented out here. 

            var connectionString = "";
            // var connectionString = Environment.GetEnvironmentVariable("STORAGECONNECTIONSTRING");


            if (!String.IsNullOrEmpty(connectionString))
                cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            if (cloudStorageAccount != null)
            {
                var container = cloudStorageAccount.CreateCloudBlobClient().GetContainerReference(containerName);
                await container.DeleteIfExistsAsync();
            }
        }

        public BlobTranscriptTests() : base()
        {
            if (cloudStorageAccount != null)
                this.store = new AzureBlobTranscriptStore(cloudStorageAccount, containerName);
        }

        [TestMethod]
        public async Task BlobTranscript_BadArgs()
        {
            if (HasStorage())
                await base._BadArgs();
        }

        [TestMethod]
        public async Task BlobTranscript_LogActivity()
        {
            if (HasStorage())
                await base._LogActivity();
        }

        [TestMethod]
        public async Task BlobTranscript_LogMultipleActivities()
        {
            if (HasStorage())
                await base._LogMultipleActivities();
        }

        [TestMethod]
        public async Task BlobTranscript_GetConversationActivities()
        {
            if (HasStorage())
                await base._GetTranscriptActivities();
        }

        [TestMethod]
        public async Task BlobTranscript_GetConversationActivitiesStartDate()
        {
            if (HasStorage())
                await base._GetTranscriptActivitiesStartDate();
        }

        [TestMethod]
        public async Task BlobTranscript_ListConversations()
        {
            if (HasStorage())
                await base._ListTranscripts();
        }

        [TestMethod]
        public async Task BlobTranscript_DeleteConversation()
        {
            if (HasStorage())
                await base._DeleteTranscript();
        }

    }
}
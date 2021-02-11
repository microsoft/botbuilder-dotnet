// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    /// <summary>
    /// Helpers to get activities from trancript files    ///. </summary>
    public static class TranscriptUtilities
    {
        private const string BotBuilderTranscriptsLocationKey = "BOTBUILDER_TRANSCRIPTS_LOCATION";
        private const string DefaultTranscriptRepositoryZipLocation = "https://github.com/Microsoft/BotBuilder/archive/master.zip";
        private const string TranscriptsZipFolder = "/Common/Transcripts/"; // Folder within the repo/zip

        private static readonly object _syncRoot = new object();

        private static string TranscriptsLocalPath { get; set; } = @"../../../../../tests/Transcripts/";

        /// <summary>
        /// Loads a list of activities from a transcript file.
        /// Use the context of the test to find the transcript file.
        /// </summary>
        /// <param name="className">Class name.</param>
        /// <param name="testName">Test name.</param>
        /// <returns>A list of activities to test.</returns>
        public static IEnumerable<IActivity> GetActivitiesFromFile(string className, string testName)
        {
            // Use TestContext to find transcripts using the following naming convention:
            // {BOTBUILDER_TRANSCRIPTS_LOCATION}\{TestClassName}\{TestMethodName}.chat
            var relativePath = Path.Combine($"{className}", $"{testName}.transcript");
            return GetActivities(relativePath);
        }

        /// <summary>
        /// Loads a list of activities from a trnascript file.
        /// </summary>
        /// <param name="relativePath">Path relative to the BOTBUILDER_TRANSCRIPTS_LOCATION environment variable value.</param>
        /// <returns>A list of activities to test.</returns>
        public static IEnumerable<IActivity> GetActivities(string relativePath)
        {
            var transcriptsRootFolder = TranscriptUtilities.EnsureTranscriptsDownload();
            var path = Path.Combine(transcriptsRootFolder, relativePath);

            // Look for .chat files first and use Chatdown tool to generate .transcripts
            // If .chat file does not exists, try .transcript instead. Throw an exception if neither .chat nor .transcript file is found.
            if (!File.Exists(path))
            {
                path = Path.Combine(transcriptsRootFolder, relativePath.Replace(".transcript", ".chat", StringComparison.InvariantCultureIgnoreCase));
            }

            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Required transcript file '{path}' does not exists in '{transcriptsRootFolder}' folder. Review the 'TranscriptsRootFolder' environment variable value.");
            }

            string content;
            if (string.Equals(Path.GetExtension(path), ".chat", StringComparison.InvariantCultureIgnoreCase))
            {
                content = ExecuteChatdownTool(path);
            }
            else
            {
                content = File.ReadAllText(path);
            }

            var activities = JsonConvert.DeserializeObject<List<Activity>>(content);

            var lastActivity = activities.Last();
            if (lastActivity.Text.Last() == '\n')
            {
                lastActivity.Text = lastActivity.Text.Remove(lastActivity.Text.Length - 1);
            }

            return activities.Take(activities.Count - 1).Append(lastActivity);
        }

        public static string EnsureTranscriptsDownload()
        {
            if (!string.IsNullOrWhiteSpace(TranscriptsLocalPath))
            {
                return TranscriptsLocalPath;
            }

            var transcriptsLocation = DefaultTranscriptRepositoryZipLocation;

            var tempPath = Path.GetTempPath();
            var zipFilePath = Path.Combine(tempPath, Path.GetFileName(transcriptsLocation));

            lock (_syncRoot)
            {
                if (!string.IsNullOrWhiteSpace(TranscriptsLocalPath))
                {
                    return TranscriptsLocalPath;
                }

                // Only download and extract zip when provided a valid absolute url. Otherwise, use it as local path
                if (Uri.IsWellFormedUriString(transcriptsLocation, UriKind.Absolute))
                {
                    DownloadFile(transcriptsLocation, zipFilePath);

                    var transcriptsExtractionPath = Path.Combine(tempPath, "Transcripts/");
                    ExtractZipFolder(zipFilePath, TranscriptsZipFolder, transcriptsExtractionPath);

                    // Set TranscriptsLocalPath for next use
                    TranscriptsLocalPath = transcriptsExtractionPath;
                }
                else
                {
                    TranscriptsLocalPath = transcriptsLocation;
                }

                return TranscriptsLocalPath;
            }
        }

        /// <summary>
        /// Get a conversation reference.
        /// This method can be used to set the conversation reference needed to create a <see cref="Adapters.TestAdapter"/>.
        /// </summary>
        /// <param name="activity">IActivity.</param>
        /// <returns>A valid conversation reference to the activity provides.</returns>
        public static ConversationReference GetConversationReference(this IActivity activity)
        {
            bool IsReply(IActivity act) => string.Equals("bot", act.From?.Role, StringComparison.InvariantCultureIgnoreCase);
            var bot = IsReply(activity) ? activity.From : activity.Recipient;
            var user = IsReply(activity) ? activity.Recipient : activity.From;
            return new ConversationReference
            {
                User = user,
                Bot = bot,
                Conversation = activity.Conversation,
                ChannelId = activity.ChannelId,
                ServiceUrl = activity.ServiceUrl,
            };
        }

        private static void ExtractZipFolder(string zipFilePath, string zipFolder, string path)
        {
            using (var zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                var zipFolderEntry = zipArchive.Entries.SingleOrDefault(e => e.FullName.EndsWith(zipFolder));
                if (zipFolderEntry == null)
                {
                    throw new InvalidOperationException($"Folder '{zipFolder}' not found in '{zipFilePath}' file.");
                }

                // Create extraction folder in temp folder
                CreateDirectoryIfNotExists(path);

                // Iterate each entry in the zip file
                foreach (var entry in zipArchive.Entries
                    .Where(e => e.FullName.StartsWith(zipFolderEntry.FullName)))
                {
                    var entryName = entry.FullName.Remove(0, zipFolderEntry.FullName.Length);

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        // No Name, it is a folder
                        CreateDirectoryIfNotExists(Path.Combine(path, entryName));
                    }
                    else
                    {
                        entry.ExtractToFile(Path.Combine(path, entryName), overwrite: true);
                    }
                }
            }
        }

        private static void DownloadFile(string url, string path)
        {
            // Download file from url to disk
            using (var httpClient = new HttpClient())
            {
                using (var urlStream = httpClient.GetStreamAsync(url).Result)
                {
                    using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    {
                        urlStream.CopyTo(fileStream);
                    }
                }
            }
        }

        private static void CreateDirectoryIfNotExists(string tempTranscriptPath)
        {
            if (!Directory.Exists(tempTranscriptPath))
            {
                Directory.CreateDirectory(tempTranscriptPath);
            }
        }

        private static string ExecuteChatdownTool(string path)
        {
            var file = new FileInfo(path);
            var chatdown = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "chatdown_gen.cmd",
                Arguments = file.FullName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            var chatdownProcess = System.Diagnostics.Process.Start(chatdown);
            var content = chatdownProcess.StandardOutput.ReadToEnd();
            var errorContent = chatdownProcess.StandardError.ReadToEnd();
            chatdownProcess.WaitForExit();
            if (string.IsNullOrEmpty(content))
            {
                string message = $"Chatdown error. Please check if chatdown is correctly installed or install it with \"npm i -g chatdown\". Error details: {errorContent}";
                throw new Exception(message);
            }

            return content;
        }
    }
}

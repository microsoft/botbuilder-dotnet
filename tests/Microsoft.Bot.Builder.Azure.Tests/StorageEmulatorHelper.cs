// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Activity = Microsoft.Bot.Schema.Activity;

// These tests require Azure Storage Emulator v5.7
// The emulator must be installed at this path C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe
// More info: https://docs.microsoft.com/azure/storage/common/storage-use-emulator
namespace Microsoft.Bot.Builder.Azure.Tests
{
    public static class StorageEmulatorHelper
    {
        /* Usage:
         * ======
           AzureStorageEmulator.exe init            : Initialize the emulator database and configuration.
           AzureStorageEmulator.exe start           : Start the emulator.
           AzureStorageEmulator.exe stop            : Stop the emulator.
           AzureStorageEmulator.exe status          : Get current emulator status.
           AzureStorageEmulator.exe clear           : Delete all data in the emulator.
           AzureStorageEmulator.exe help [command]  : Show general or command-specific help.
         */
        public enum StorageEmulatorCommand
        {
            /// <summary>Init command</summary>
            Init,

            /// <summary>Start command</summary>
            Start,

            /// <summary>Stop command</summary>
            Stop,

            /// <summary>Status command</summary>
            Status,

            /// <summary>Clear command</summary>
            Clear,
        }

        public static int StartStorageEmulator()
        {
            return ExecuteStorageEmulatorCommand(StorageEmulatorCommand.Start);
        }

        public static int StopStorageEmulator()
        {
            return ExecuteStorageEmulatorCommand(StorageEmulatorCommand.Stop);
        }

        public static int ExecuteStorageEmulatorCommand(StorageEmulatorCommand command)
        {
            var emulatorPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft SDKs",
                "Azure",
                "Storage Emulator",
                "AzureStorageEmulator.exe");

            var start = new ProcessStartInfo
            {
                Arguments = command.ToString(),
                FileName = emulatorPath,
            };
            var exitCode = ExecuteProcess(start);
            return exitCode;
        }

        private static int ExecuteProcess(ProcessStartInfo startInfo)
        {
            int exitCode = -1;
            using (var proc = new Process { StartInfo = startInfo })
            {
                proc.Start();
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

            return exitCode;
        }
    }
}

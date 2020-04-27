// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        public static bool CheckEmulator()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AGENT_NAME")))
            {
                Assert.Inconclusive("This test requires Azure Storage Emulator to run and is disabled on the build server.");
                return false;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var (code, output) = StorageEmulatorHelper.Status();
                if (output.IndexOf("IsRunning: True") > 0)
                {
                    return true;
                }

                (code, output) = StorageEmulatorHelper.StartStorageEmulator();
                return output.IndexOf("started") > 0;
            }

            Assert.Inconclusive("This test requires Azure Storage Emulator to run");
            return false;
        }

        public static bool EnsureStarted()
        {
            var (code, output) = StorageEmulatorHelper.Status();
            if (output.IndexOf("IsRunning: True") > 0)
            {
                return true;
            }

            (code, output) = StorageEmulatorHelper.StartStorageEmulator();
            return output.IndexOf("started") > 0;
        }

        public static (int, string) StartStorageEmulator()
        {
            return ExecuteStorageEmulatorCommand(StorageEmulatorCommand.Start);
        }

        public static (int, string) Status()
        {
            return ExecuteStorageEmulatorCommand(StorageEmulatorCommand.Status);
        }

        public static (int, string) StopStorageEmulator()
        {
            return ExecuteStorageEmulatorCommand(StorageEmulatorCommand.Stop);
        }

        public static (int, string) ExecuteStorageEmulatorCommand(StorageEmulatorCommand command)
        {
            var emulatorPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft SDKs",
                "Azure",
                "Storage Emulator",
                "AzureStorageEmulator.exe");

            StringBuilder sb = new StringBuilder();
            var startIInfo = new ProcessStartInfo
            {
                Arguments = command.ToString(),
                FileName = emulatorPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            using (var proc = new Process { StartInfo = startIInfo })
            {
                proc.OutputDataReceived += (sender, e) => sb.Append(e.Data);
                proc.Start();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                return (proc.ExitCode, sb.ToString());
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    /// <summary>
    /// This class uses the command line to merge all the schemas in the project.
    /// </summary>
    /// <remarks>
    /// This will install the latest version of botframewrork-cli if the schema changed and npm is present.
    /// </remarks>
    public class SchemaTestsFixture : IDisposable
    {
        public SchemaTestsFixture()
        {
            var testsPath = Path.GetFullPath(Path.Combine(ProjectPath, ".."));
            var schemaPath = Path.Combine(testsPath, "tests.schema");

            // Only generate schemas on Windows.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Save an in memory copy of the current schema file.
                var oldSchema = File.Exists(schemaPath) ? File.ReadAllText(schemaPath) : string.Empty;
                File.Delete(schemaPath);

                // Merge all schema files,
                var mergeCommand = $"/C bf dialog:merge ../../libraries/**/*.schema ../../libraries/**/*.uischema ../**/*.schema !../**/testbot.schema -o {schemaPath}";
                var error = RunCommand(mergeCommand);

                // Check if there were any errors or if the new schema file has changed.
                var newSchema = File.Exists(schemaPath) ? File.ReadAllText(schemaPath) : string.Empty;
                if (error.Length != 0 || !newSchema.Equals(oldSchema))
                {
                    // Try installing latest bf if the schema changed to make sure the discrepancy is not because
                    // we are using a different version.
                    error = RunCommand("/C npm i -g @microsoft/botframework-cli@next");
                    Assert.True(error.Length == 0, error);

                    // Rerun merge command
                    error = RunCommand(mergeCommand);
                    Assert.True(error.Length == 0, error);
                }
            }

            // Load the generated schema
            Schema = JSchema.Parse(File.ReadAllText(schemaPath));
            Assert.NotNull(Schema);
        }

        public static string ProjectPath => Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));

        public static string SolutionPath => Path.GetFullPath(Path.Combine(ProjectPath, PathUtils.NormalizePath(@"..\..")));

        /// <summary>
        /// Gets an instance of the generated schema.
        /// </summary>
        /// <value>
        /// An instance of the generated schema.
        /// </value>
        public JSchema Schema { get; }

        public void Dispose()
        {
        }

        private static string RunCommand(string args)
        {
            var startInfo = new ProcessStartInfo("cmd.exe", args)
            {
                WorkingDirectory = ProjectPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            return process.ExitCode != 0 ? process.StandardError.ReadToEnd() : string.Empty;
        }
    }
}

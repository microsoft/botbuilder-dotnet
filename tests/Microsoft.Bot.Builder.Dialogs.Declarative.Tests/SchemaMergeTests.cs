﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
#pragma warning disable SA1629 // Documentation text should end with a period

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    /// <summary>
    /// NOTE: This requires BF CLI to be installed.
    /// </summary>
    /// <remarks>
    /// npm config set registry https://botbuilder.myget.org/F/botframework-cli/npm/
    /// npm i -g @microsoft/botframework-cli
    /// bf plugins:install @microsoft/bf-dialog
    /// </remarks>
    [TestClass]
    [TestCategory("IgnoreInAutomatedBuild")]
    public class SchemaMergeTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public static JSchema Schema { get; set; }

        public static IEnumerable<object[]> Dialogs { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // static field initialization
            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, PathUtils.NormalizePath(@"..\..\..")));
            var testsPath = Path.GetFullPath(Path.Combine(projectPath, ".."));
            var solutionPath = Path.GetFullPath(Path.Combine(projectPath, PathUtils.NormalizePath(@"..\..")));
            var schemaPath = Path.Combine(testsPath, "tests.schema");

            var ignoreFolders = new string[]
            {
                PathUtils.NormalizePath(@"Microsoft.Bot.Builder.TestBot.Json\Samples\EmailBot"),
                PathUtils.NormalizePath(@"Microsoft.Bot.Builder.TestBot.Json\Samples\CalendarBot"),
                "bin"
            };

            ResourceExplorer = new ResourceExplorer()
                .AddFolders(Path.Combine(solutionPath, "libraries"), monitorChanges: false)
                .AddFolders(Path.Combine(solutionPath, "tests"), monitorChanges: false);

            Dialogs = ResourceExplorer.GetResources(".dialog")
                .Cast<FileResource>()
                .Where(r => !r.Id.EndsWith(".schema.dialog") && !ignoreFolders.Any(f => r.FullName.Contains(f)))
                .Select(resource => new object[] { resource });

            try
            {
                ProcessStartInfo startInfo;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    File.Delete(schemaPath);
                    startInfo = new ProcessStartInfo("cmd.exe", $"/C bf dialog:merge ../../libraries/**/*.schema ../../libraries/**/*.uischema ../**/*.schema !../**/testbot.schema -o {schemaPath}");
                    startInfo.WorkingDirectory = projectPath;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardError = true;

                    var process = Process.Start(startInfo);
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    Assert.AreEqual(error, string.Empty);
                }
            }
            catch (Exception err)
            {
                Assert.Fail(err.Message);
            }

            Assert.IsTrue(File.Exists(schemaPath));
            var json = File.ReadAllText(schemaPath);
            Schema = JSchema.Parse(json);
        }

        [DataTestMethod]
        [DynamicData(nameof(Dialogs))]
        public async Task TestDialogResourcesAreValidForSchema(Resource resource)
        {
            if (Schema == null)
            {
                Assert.Fail("missing schema file");
            }

            FileResource fileResource = resource as FileResource;

            // load the merged app schema file (validating it's truly a jsonschema doc
            // and use it to validate all .dialog files are valid to this schema
            var json = await resource.ReadTextAsync();
            var jtoken = (JToken)JsonConvert.DeserializeObject(json);
            var jobj = jtoken as JObject;
            var schema = jobj["$schema"]?.ToString();

            try
            {
                // everything should have $schema
                Assert.IsNotNull(schema, "Missing $schema");

                var folder = Path.GetDirectoryName(fileResource.FullName);

                // NOTE: Some schemas are not local.  We don't validate against those because they often depend on the SDK itself
                if (!schema.StartsWith("http"))
                {
                    Assert.IsTrue(File.Exists(Path.Combine(folder, PathUtils.NormalizePath(schema))), $"$schema {schema}");
                    jtoken.Validate(Schema);
                }
            }
            catch (JSchemaValidationException err)
            {
                Assert.Fail($"{fileResource.FullName}\n{err.Message}", $"Dialog is not valid for the $schema {schema}");
            }
        }
    }
}

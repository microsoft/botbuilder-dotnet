// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Tests
{
    /// <summary>
    /// Test schema merging and instances.
    /// </summary>
    public class SchemaMergeTests : IClassFixture<SchemaTestsFixture>
    {
        private readonly SchemaTestsFixture _schemaTestsFixture;

        /// <summary>
        /// Initializes static members of the <see cref="SchemaMergeTests"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor  loads the dialogs to be tested in a static <see cref="Dialogs"/> property so they can be used in theory tests.
        /// </remarks>
        static SchemaMergeTests()
        {
            var ignoreFolders = new[]
            {
                PathUtils.NormalizePath(@"Microsoft.Bot.Builder.TestBot.Json\Samples\EmailBot"),
                PathUtils.NormalizePath(@"Microsoft.Bot.Builder.TestBot.Json\Samples\CalendarBot"),
                "bin"
            };

            var resourceExplorer = new ResourceExplorer()
                .AddFolders(Path.Combine(SchemaTestsFixture.SolutionPath, "libraries"), monitorChanges: false)
                .AddFolders(Path.Combine(SchemaTestsFixture.SolutionPath, "tests"), monitorChanges: false);

            // Store the dialog list in the Dialogs property.
            Dialogs = resourceExplorer.GetResources(".dialog")
                .Cast<FileResource>()
                .Where(r => !r.Id.EndsWith(".schema.dialog") && !ignoreFolders.Any(f => r.FullName.Contains(f)))
                .Select(resource => new object[]
                {
                    resource.Id,
                    resource.FullName
                });
        }

        public SchemaMergeTests(SchemaTestsFixture schemaTestsFixture)
        {
            _schemaTestsFixture = schemaTestsFixture;
        }

        public static IEnumerable<object[]> Dialogs { get; }

        [Theory]
        [MemberData(nameof(Dialogs))]
        public async Task TestDialogResourcesAreValidForSchema(string resourceId, string resourceName)
        {
            Assert.NotNull(resourceId);
            Assert.NotNull(resourceName);

            // load the merged app schema file (validating it's truly a json schema doc
            // and use it to validate all .dialog files are valid to this schema
            var fileResource = new FileResource(resourceName);
            var json = await fileResource.ReadTextAsync();
            var jToken = JsonConvert.DeserializeObject<JToken>(json);
            var jObj = (JObject)jToken;
            var schema = jObj["$schema"]?.ToString();

            // everything should have $schema
            Assert.NotNull(schema);

            if (schema.StartsWith("http"))
            {
                // NOTE: Some schemas are not local.  We don't validate against those because they often depend on the SDK itself
                return;
            }

            var folder = Path.GetDirectoryName(fileResource.FullName);
            Assert.True(File.Exists(Path.Combine(folder, PathUtils.NormalizePath(schema))), $"$schema {schema}");

            // NOTE: Microsoft.SendActivity in the first file fails validation even though it is valid, same as Microsoft.StaticActivityTemplate on the last two.
            // Bug filed with Newtonsoft: https://stackoverflow.com/questions/63493078/why-does-validation-fail-in-code-but-work-in-newtonsoft-web-validator
            var omit = new List<string>
            {
                "Action_SendActivity.test.dialog",
                "Action_BeginSkill.test.dialog",
                "Action_BeginSkillEndDialog.test.dialog",
                "Action_SendTabAuthResponseErrorWithAdapter.test.dialog",
                "Action_SendTaskModuleCardResponseError.test.dialog",
                "Action_SendAppBasedLinkQueryResponseError.test.dialog",
                "Action_SendTabCardResponseError.test.dialog",
                "Action_SendMEAuthResponseError.test.dialog",
                "Action_SendMESelectItemResponseError.test.dialog",
                "Action_SendMEAuthResponseErrorWithAdapter.test.dialog",
                "Action_SendMEMessageResponseError.test.dialog",
                "Action_SendMEBotMessagePreviewResponseError.test.dialog",
                "Action_SendMEConfigQuerySettingUrlResponseError.test.dialog",
                "Action_SendTabAuthResponseError.test.dialog",
                "TestScriptTests_OAuthInputLG.test.dialog"
            };
            if (omit.Any(e => fileResource.FullName.Contains(e)))
            {
                // schema is in the omit list, end the test.
                return;
            }

            try
            {
                jToken.Validate(_schemaTestsFixture.Schema);
            }
            catch (JSchemaValidationException err)
            {
                throw new XunitException($"{fileResource.FullName}\n{err.Message}");
            }
        }
    }
}

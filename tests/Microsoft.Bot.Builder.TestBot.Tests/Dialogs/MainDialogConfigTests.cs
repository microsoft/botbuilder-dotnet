using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Utils;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class MainDialogConfigTests : DialogTestsBase
    {
        public MainDialogConfigTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("A", "", "")]
        [InlineData("", "B", "")]
        [InlineData("", "", "C")]
        [InlineData("A", "B", "")]
        [InlineData("A", "", "C")]
        [InlineData("", "B", "C")]
        public async Task MessageIfLuisNotConfigured(string luisAppId, string luisApiKey, string luisApiHostName)
        {
            // Arrange
            var luisMockConfig = new Mock<IConfiguration>();
            luisMockConfig.Setup(x => x["LuisAppId"]).Returns(luisAppId);
            luisMockConfig.Setup(x => x["LuisAPIKey"]).Returns(luisApiKey);
            luisMockConfig.Setup(x => x["LuisAPIHostName"]).Returns(luisApiHostName);

            var sut = new MainDialog(luisMockConfig.Object, MockLogger.Object);
            var testBot = new DialogsTestBot(sut, Output);

            // Act/Assert
            var reply = await testBot.SendAsync<IMessageActivity>("hi");
            Assert.Equal("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", reply.Text);
        }

        [Fact]
        public async Task ShowPromptIfLuisIsConfigured()
        {
            // Arrange
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object);
            var testBot = new DialogsTestBot(sut, Output);

            // Act/Assert
            var reply = await testBot.SendAsync<IMessageActivity>("hi");
            Assert.Equal("What can I help you with today?", reply.Text);
        }
    }
}

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Dialogs
{
    public class MainDialogDITests : DialogTestsBase
    {
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
            var luisMockConfig = new Mock<IConfiguration>();
            luisMockConfig.Setup(x => x["LuisAppId"]).Returns(luisAppId);
            luisMockConfig.Setup(x => x["LuisAPIKey"]).Returns(luisApiKey);
            luisMockConfig.Setup(x => x["LuisAPIHostName"]).Returns(luisApiHostName);

            var sut = new MainDialog(luisMockConfig.Object, MockLogger.Object);
            var testFlow = BuildTestFlow(sut);
            await testFlow.Send("Hi")
                .AssertReply("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.")
                .StartTestAsync();
        }

        [Fact]
        public async Task ShowPromptIfLuisIsConfigured()
        {
            var sut = new MainDialog(MockConfig.Object, MockLogger.Object);
            var testFlow = BuildTestFlow(sut);
            await testFlow.Send("Hi")
                .AssertReply("What can I help you with today?")
                .StartTestAsync();
        }
    }
}

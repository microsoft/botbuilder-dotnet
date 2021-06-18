using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class TabContextTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("hi")]
        public void TabContextInits(string theme)
        {
            var tabContext = new TabContext()
            {
                Theme = theme
            };

            Assert.NotNull(tabContext);
            Assert.IsType<TabContext>(tabContext);
            Assert.Equal(theme, tabContext.Theme);
        }
    }
}

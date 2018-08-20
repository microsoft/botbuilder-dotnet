using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Configuration.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public async Task ConnectAssignsUniqueIds()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                service.Id = "1";
                config2.ConnectService(service);
            }

            HashSet<string> hashset = new HashSet<string>();
            foreach (var service in config2.Services)
            {
                Assert.IsFalse(hashset.Contains(service.Id), "the id assigned is not unique for the collection");
                hashset.Add(service.Id);
            }
        }

        [TestMethod]
        public async Task FindServices()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            Assert.IsNotNull(config.FindServiceByNameOrId("3"), "Should find by id");
            Assert.IsNotNull(config.FindServiceByNameOrId("testInsights"), "Should find by name");
            Assert.IsNotNull(config.FindService("3"), "Should find by id");
            Assert.IsNull(config.FindService("testInsights"), "Should not find by name ");
        }

        [TestMethod]
        public async Task DisconnectServicesById()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
            }

            var servicesIds = config2.Services.Select(s => s.Id).ToArray();

            foreach (var key in servicesIds)
            {
                config2.DisconnectService(key);
            }
            Assert.AreEqual(config2.Services.Count, 0, "didn't remove all services");
        }

        [TestMethod]
        public async Task DisconnectServicesByNameOrId_UsingId()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
            }
            var servicesIds = config2.Services.Select(s => s.Id).ToArray();

            foreach (var id in servicesIds)
            {
                config2.DisconnectServiceByNameOrId(id);
            }
            Assert.AreEqual(config2.Services.Count, 0, "didn't remove all services");
        }

        [TestMethod]
        public async Task DisconnectByNameOrId_UsingName()
        {
            var config = await BotConfiguration.LoadAsync(@"..\..\test.bot");
            var config2 = new BotConfiguration();
            foreach (var service in config.Services)
            {
                config2.ConnectService(service);
            }
            var serviceNames = config2.Services.Select(s => s.Name).ToArray();

            foreach (var name in serviceNames)
            {
                config2.DisconnectServiceByNameOrId(name);
            }
            Assert.AreEqual(config2.Services.Count, 0, "didn't remove all services");
        }
    }
}

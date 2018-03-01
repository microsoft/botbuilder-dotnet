using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.NetCore
{
    internal class BotFrameworkBuilder : IBotFrameworkBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        //private readonly OptionsConfigurer _optionsConfigurer;

        public BotFrameworkBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            //_optionsConfigurer = new OptionsConfigurer();
            //_serviceCollection.AddSingleton<IConfigureOptions<BotFrameworkOptions>>(_optionsConfigurer);
        }

        //public IBotBuilder AddBot<T>(string name = null) where T : class, IBot
        //{
        //    name = name ?? "default";

        //    //_optionsConfigurer.BotNameMap.Add(name, typeof(T));

        //    _serviceCollection.AddTransient<T>();

        //    return new BotFrameworkConfigurationBuilder();
        //}

        //private sealed class OptionsConfigurer : IConfigureOptions<BotFrameworkOptions>
        //{
        //    public Dictionary<string, Type> BotNameMap = new Dictionary<string, Type>();

        //    public void Configure(BotFrameworkOptions options)
        //    {
        //        options.BotNameMap = BotNameMap;
        //    }
        //}
    }
}

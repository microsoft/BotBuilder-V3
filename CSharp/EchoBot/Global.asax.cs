using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace EchoBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Conversation.UpdateContainer(
               builder =>
               {
                    // Bot Storage: Here we register the state storage for your bot. 
                    // Default store: volatile in-memory store - Only for prototyping!
                    // We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
                    // For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure
                    var store = new InMemoryDataStore();

                    // Other storage options
                    // var store = new TableBotDataStore("...DataStorageConnectionString..."); // requires Microsoft.BotBuilder.Azure Nuget package 
                    // var store = new DocumentDbBotDataStore("cosmos db uri", "cosmos db key"); // requires Microsoft.BotBuilder.Azure Nuget package 

                   var memorystore = new InMemoryDataStore();
                   builder
                       .RegisterType<InMemoryDataStore>()
                       .Keyed<IBotDataStore<BotData>>(typeof(ConnectorStore));

                   builder.Register(c => new CachingBotDataStore(memorystore, CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                       .As<IBotDataStore<BotData>>()
                       .AsSelf()
                       .SingleInstance();
               });
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}

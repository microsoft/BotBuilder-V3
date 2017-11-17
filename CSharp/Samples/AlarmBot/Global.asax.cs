using System;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.AlarmBot.Models;

namespace Microsoft.Bot.Sample.AlarmBot
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var config = GlobalConfiguration.Configuration;

            Conversation.UpdateContainer(
                builder =>
                {
                    // Register the Bot Builder module
                    builder.RegisterModule(new DialogModule());
                    // Register the alarm dependencies
                    builder.RegisterModule(new AlarmModule());

                    builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                    // Bot Storage: Here we register the state storage for your bot. 
                    // Default store: volatile in-memory store - Only for prototyping!
                    // We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
                    // For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure
                    var store = new InMemoryDataStore();

                    // Other storage options
                    // var store = new TableBotDataStore("...DataStorageConnectionString..."); // requires Microsoft.BotBuilder.Azure Nuget package 
                    // var store = new DocumentDbBotDataStore("cosmos db uri", "cosmos db key"); // requires Microsoft.BotBuilder.Azure Nuget package 

                    builder.Register(c => store)
                        .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                        .AsSelf()
                        .SingleInstance();

                    // Register your Web API controllers.
                    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                    builder.RegisterWebApiFilterProvider(config);
                });

            // Set the dependency resolver to be Autofac.
            config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);

            // WebApiConfig stuff
            GlobalConfiguration.Configure(cfg =>
            {
                cfg.MapHttpAttributeRoutes();

                cfg.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            });
        }

        public static ILifetimeScope FindContainer()
        {
            var config = GlobalConfiguration.Configuration;
            var resolver = (AutofacWebApiDependencyResolver)config.DependencyResolver;
            return resolver.Container;
        }
    }
}
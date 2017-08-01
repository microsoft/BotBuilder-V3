namespace Microsoft.Bot.Sample.EchoBot
{
    using System;
    using System.Reflection;
    using System.Web.Http;

    using Autofac;
    using Autofac.Integration.WebApi;

    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.WindowsAzure.Storage;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            {
                // http://docs.autofac.org/en/latest/integration/webapi.html#quick-start
                var builder = new ContainerBuilder();

                // register the Bot Builder module
                builder.RegisterModule(new DialogModule());
                // register the alarm dependencies
                builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                builder
                    .RegisterInstance(new EchoDialog())
                    .As<IDialog<object>>();

                var store = new TableBotDataStore(CloudStorageAccount.DevelopmentStorageAccount);
                builder.Register(c => store)
                    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                    .AsSelf()
                    .SingleInstance();


                builder.Register(c => new CachingBotDataStore(store,
                                                              CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .InstancePerLifetimeScope();

                // Get your HttpConfiguration.
                var config = GlobalConfiguration.Configuration;

                // Register your Web API controllers.
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                // OPTIONAL: Register the Autofac filter provider.
                builder.RegisterWebApiFilterProvider(config);


                // Uncomment the block below to register the private state storage for your bot
                // builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                //// Uncomment one of the lines below to choose your store
                //// var store = new TableBotDataStore("...DataStorageConnectionString..."); // requires Microsoft.BotBuilder.Azure Nuget package 
                //// var store = new DocumentDbBotDataStore("cosmos db uri", "cosmos db key"); // requires Microsoft.BotBuilder.Azure Nuget package 
                //// var store = new InMemoryStore(); // volatile in-memory store

                //builder.Register(c => store)
                //    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                //    .AsSelf()
                //    .SingleInstance();

                // Set the dependency resolver to be Autofac.
                builder.Update(Conversation.Container);
                config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);

            }

            // WebApiConfig stuff
            GlobalConfiguration.Configure(config =>
            {
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            });
        }
    }
}

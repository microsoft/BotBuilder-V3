using System.Web.Http;

namespace Microsoft.Bot.Sample.TemplateBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Bot Storage: This is a great spot to register the private state storage for your bot. 
            // We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
            // For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure

            // Uncomment the block below to register the private state storage for your bot
            //Conversation.UpdateContainer(
            //    builder =>
            //        {
            //            builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

            //            // Uncomment one of the lines below to choose your store
            //            // var store = new TableBotDataStore("...DataStorageConnectionString..."); // requires Microsoft.BotBuilder.Azure Nuget package 
            //            // var store = new DocumentDbBotDataStore("cosmos db uri", "cosmos db key"); // requires Microsoft.BotBuilder.Azure Nuget package 
            //            // var store = new InMemoryDataStore(); // volatile in-memory store

            //            builder.Register(c => store)
            //                .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
            //                .AsSelf()
            //                .SingleInstance();

            //        });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}

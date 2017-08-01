using System.Web.Http;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Uncomment the block below to register the private state storage for your bot
            //Conversation.UpdateContainer(
            //    builder =>
            //        {
            //            builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));


            //            // Uncomment one of the lines below to choose your store
            //            // var store = new TableBotDataStore("...DataStorageConnectionString..."); // requires Microsoft.BotBuilder.Azure Nuget package 
            //            // var store = new DocumentDbBotDataStore("cosmos db uri", "cosmos db key"); // requires Microsoft.BotBuilder.Azure Nuget package 
            //            // var store = new InMemoryStore(); // volatile in-memory store

            //            builder.Register(c => store)
            //                .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
            //                .AsSelf()
            //                .SingleInstance();

            //            builder.Update(Conversation.Container);
            //        });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}

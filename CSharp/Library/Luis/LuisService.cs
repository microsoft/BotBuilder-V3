using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Microsoft.Bot.Builder.Fibers;

namespace Microsoft.Bot.Builder.Luis
{
    public class LuisResult
    {
        public IntentRecommendation[] Intents { get; set; }

        public EntityRecommendation[] Entities { get; set; }
    }

    public interface ILuisService
    {
        Uri BuildUri(string text);
        Task<LuisResult> QueryAsync(Uri uri); 
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Serializable]
    public class LuisModel : Attribute
    {
        public readonly string ModelID;
        public readonly string SubscriptionKey;

        public LuisModel(string modelID, string subscriptionKey)
        {
            SetField.SetNotNull(out this.ModelID, nameof(modelID), modelID);
            SetField.SetNotNull(out this.SubscriptionKey, nameof(subscriptionKey), subscriptionKey);
        }
    }

    [Serializable]
    public sealed class LuisService : ILuisService
    {
        private readonly LuisModel model;

        public LuisService(LuisModel model)
        {
            SetField.SetNotNull(out this.model, nameof(model), model);
        }

        public static readonly Uri UriBase = new Uri("https://api.projectoxford.ai/luis/v1/application");

        Uri ILuisService.BuildUri(string text)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["id"] = this.model.ModelID;
            queryString["subscription-key"] = this.model.SubscriptionKey;
            queryString["q"] = text;

            var builder = new UriBuilder(UriBase);
            builder.Query = queryString.ToString();
            return builder.Uri;
        }

        async Task<LuisResult> ILuisService.QueryAsync(Uri uri)
        {
            string json;
            using (HttpClient client = new HttpClient())
            {
                json = await client.GetStringAsync(uri);
            }

            var result = JsonConvert.DeserializeObject<LuisResult>(json);
            return result;
        }
    }

    public static partial class Extensions
    {
        public static async Task<LuisResult> QueryAsync(this ILuisService service, string text)
        {
            var uri = service.BuildUri(text);
            return await service.QueryAsync(uri);
        }

        public static bool TryFindEntity(this LuisResult result, string type, out EntityRecommendation entity)
        {
            entity = result.Entities?.FirstOrDefault(e => e.Type == type);
            return entity != null;
        }
    }
}

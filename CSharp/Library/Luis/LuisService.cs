using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Microsoft.Bot.Builder.Fibers;

namespace Microsoft.Bot.Builder.Luis
{
    /// <summary>
    /// The result of a LUIS query.
    /// </summary>
    public class LuisResult
    {
        /// <summary>
        /// The intents found in the query text.
        /// </summary>
        public IntentRecommendation[] Intents { get; set; }

        /// <summary>
        /// The entities found in the query text.
        /// </summary>
        public EntityRecommendation[] Entities { get; set; }
    }

    /// <summary>
    /// A mockable interface for the LUIS service.
    /// </summary>
    public interface ILuisService
    {
        /// <summary>
        /// Build the query uri for the query text.
        /// </summary>
        /// <param name="text">The query text.</param>
        /// <returns>The query uri.</returns>
        Uri BuildUri(string text);

        /// <summary>
        /// Query the LUIS service using this uri.
        /// </summary>
        /// <param name="uri">The query uri.</param>
        /// <returns>The LUIS result.</returns>
        Task<LuisResult> QueryAsync(Uri uri); 
    }

    /// <summary>
    /// The LUIS model information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Serializable]
    public class LuisModel : Attribute
    {
        /// <summary>
        /// The LUIS model ID.
        /// </summary>
        public readonly string ModelID;

        /// <summary>
        /// The LUIS subscription key.
        /// </summary>
        public readonly string SubscriptionKey;

        /// <summary>
        /// Construct the LUIS model information.
        /// </summary>
        /// <param name="modelID">The LUIS model ID.</param>
        /// <param name="subscriptionKey">The LUIS subscription key.</param>
        public LuisModel(string modelID, string subscriptionKey)
        {
            SetField.SetNotNull(out this.ModelID, nameof(modelID), modelID);
            SetField.SetNotNull(out this.SubscriptionKey, nameof(subscriptionKey), subscriptionKey);
        }
    }

    /// <summary>
    /// Standard implementation of ILuisService against actual LUIS service.
    /// </summary>
    [Serializable]
    public sealed class LuisService : ILuisService
    {
        private readonly LuisModel model;

        /// <summary>
        /// Construct the LUIS service using the model information.
        /// </summary>
        /// <param name="model">The LUIS model information.</param>
        public LuisService(LuisModel model)
        {
            SetField.SetNotNull(out this.model, nameof(model), model);
        }

        /// <summary>
        /// The base URi for accessing LUIS.
        /// </summary>
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

    /// <summary>
    /// LUIS extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Query the LUIS service using this text.
        /// </summary>
        /// <param name="text">The query text.</param>
        /// <returns>The LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, string text)
        {
            var uri = service.BuildUri(text);
            return await service.QueryAsync(uri);
        }

        /// <summary>
        /// Try to find an entity within the result.
        /// </summary>
        /// <param name="result">The LUIS result.</param>
        /// <param name="type">The entity type.</param>
        /// <param name="entity">The found entity.</param>
        /// <returns>True if the entity was found, false otherwise.</returns>
        public static bool TryFindEntity(this LuisResult result, string type, out EntityRecommendation entity)
        {
            entity = result.Entities?.FirstOrDefault(e => e.Type == type);
            return entity != null;
        }
    }
}


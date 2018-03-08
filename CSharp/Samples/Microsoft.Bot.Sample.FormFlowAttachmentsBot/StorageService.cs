using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.FormFlowAttachmentsBot
{
    public class StorageService
    {
        public async Task<long> StoreImageAsync(Stream image)
        {
            // This is a mock service simulating image retrieval & storing from the input stream
            // We are actually copying the data to a memory stream in order to get its size
            var ms = new MemoryStream();
            await image.CopyToAsync(ms);

            return ms.Length;
        }
    }
}
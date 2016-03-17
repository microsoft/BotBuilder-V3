using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder.Serializers
{
    internal class CookieSerializer
    {
        public static string Serialize<T>(T obj)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            {
                new BinaryFormatter().Serialize(stream, obj);
                stream.Close();
                return HttpServerUtility.UrlTokenEncode(cmpStream.ToArray());
            }
        }

        public static T Deserialize<T>(string str)
            where T : class
        {
            byte[] bytes = HttpServerUtility.UrlTokenDecode(str);

            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            {
                return new BinaryFormatter().Deserialize(gz) as T;
            }
        }
    }
}

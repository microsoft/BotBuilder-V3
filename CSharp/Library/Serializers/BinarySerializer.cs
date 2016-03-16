using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Serializers
{
    internal class BinarySerializer
    {
        public static string Serialize<T>(T obj)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memStream, obj);
                return Convert.ToBase64String(memStream.ToArray());
            }
        }

        public static T Deserialize<T>(string str)
            where T : class
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return new BinaryFormatter().Deserialize(stream) as T;
            }
        }

        public static string GZipSerialize<T>(T obj)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            {
                new BinaryFormatter().Serialize(stream, obj);
                stream.Close();
                return Convert.ToBase64String(cmpStream.ToArray());
            }
        }

        public static T GZipDeserialize<T>(string str)
            where T : class
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            {
                return new BinaryFormatter().Deserialize(gz) as T;
            }
        }

    }
}

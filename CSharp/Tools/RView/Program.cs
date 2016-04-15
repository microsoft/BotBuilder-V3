using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace RView
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("RView <resource file path> [-c <locale>] [-p <prefix>]");
            Console.WriteLine("If -c is not specified will print the resource file on the console.");
            Console.WriteLine("-c <locale> : Will copy resources to a <path-locale.res>.");
            Console.WriteLine("-p <prefix> : If specified will add prefix to every target string when copying.");
            System.Environment.Exit(-1);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
            }
            var path = args[0];
            var isResX = Path.GetExtension(path) == ".resx";
            string locale = null;
            string prefix = null;
            for (var i = 1; i < args.Length; ++i)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-c":
                        if (++i < args.Length)
                        {
                            locale = args[i];
                        }
                        break;
                    case "-p":
                        if (++i < args.Length)
                        {
                            prefix = args[i];
                        }
                        break;
                    default:
                        Usage();
                        break;
                }
            }

            IResourceWriter writer = null;
            FileStream outStream = null;
            if (locale != null)
            {
                var outPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "-" + locale + Path.GetExtension(path));
                Console.Write($"Copying to {outPath}");
                if (prefix != null)
                {
                    Console.Write($" with prefix {prefix}");
                }
                Console.WriteLine();
                outStream = new FileStream(outPath, FileMode.Create);
                writer = isResX ? (IResourceWriter)new ResXResourceWriter(outStream) : (IResourceWriter)new ResourceWriter(outStream);
            }
            using (var stream = new FileStream(path, FileMode.Open))
            using (var reader = isResX ? (IResourceReader)new ResXResourceReader(stream) : (IResourceReader)new ResourceReader(stream))
            using (outStream)
            using (writer)
            {
                int values = 0;
                int lists = 0;
                int templates = 0;
                foreach (DictionaryEntry entry in reader)
                {
                    var fullKey = (string)entry.Key;
                    var value = (string)entry.Value;
                    var typeAndKey = fullKey.Split(SEPERATOR);
                    var type = typeAndKey[0];
                    if (writer == null)
                    {
                        Console.WriteLine($"{fullKey}: {value}");
                    }
                    else
                    {
                        if (type == "LIST" || type == "TEMPLATE")
                        {
                            writer.AddResource(fullKey, MakeList(from elt in SplitList(value) select prefix + elt));
                        }
                        else if (type == "CULTURE")
                        {
                            writer.AddResource("CULTURE" + SSEPERATOR, locale);
                        }
                        else
                        {
                            writer.AddResource(fullKey, prefix + value);
                        }
                    }
                    switch (type)
                    {
                        case "VALUE": ++values; break;
                        case "LIST": ++lists; break;
                        case "TEMPLATE": ++templates; break;
                    }
                }
                Console.WriteLine($"Found {values} values, {lists} lists and {templates} templates");
            }
        }

        const char SEPERATOR = ';';
        const string SSEPERATOR = ";";
        const string ESCAPED_SEPERATOR = "__semi";

        static string MakeList(IEnumerable<string> elements)
        {
            return string.Join(SSEPERATOR, from elt in elements select elt.Replace(SSEPERATOR, ESCAPED_SEPERATOR));
        }

        static IEnumerable<string> SplitList(string str)
        {
            var elements = str.Split(SEPERATOR);
            return from elt in elements select elt.Replace(ESCAPED_SEPERATOR, SSEPERATOR);
        }
    }
}


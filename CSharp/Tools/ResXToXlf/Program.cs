using System;
using System.Linq;
using System.Xml.Linq;

namespace ResXToXlf
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("ResXToXlf <source.resx> <destination.xlf>");
                Console.WriteLine("Will copy source resources to destination and show an error if any resource is not in desination.");
                System.Environment.Exit(-1);
            }
            var resx = args[0];
            var xlf = args[1];
            var error = false;
            var reswDoc = XDocument.Load(resx);
            var sourceEntries = reswDoc.Root.Elements("data").ToDictionary(e => e.Attribute("name").Value,
                                                            e => e.Element("value").Value);
            var xlfDoc = XDocument.Load(xlf);
            var targetEntries = xlfDoc.Descendants().Where(d => d.Name.LocalName == "trans-unit").
                                 ToDictionary(e => e.Attribute("id").Value,
                                            e => e.Elements().Skip(1).FirstOrDefault());

            foreach (var entry in sourceEntries)
            {
                if (!targetEntries.ContainsKey(entry.Key))
                {
                    error = true;
                    Console.WriteLine("");
                    Console.WriteLine("** Key {0} not found in {1}", entry.Key, xlf);
                    Console.WriteLine("** {0} not modified", xlf);
                    Console.WriteLine("");
                    continue;
                }
                var target = targetEntries[entry.Key];
                target.Value = entry.Value;
                target.Attribute("state").Value = "signed-off";
            }
            if (!error)
            {
                xlfDoc.Save(xlf);
            }
        }
    }
}

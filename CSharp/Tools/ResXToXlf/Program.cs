using System;
using System.Linq;
using System.Xml.Linq;

namespace ResXToXlf
{
    class Program
    {
        static void Main(string[] args)
        {
            var resx = string.Format("{0}.resx", filename);
            var xlf = string.Format("{0}.xlf", filename);

            var error = false;

            var reswDoc = XDocument.Load(resx);
            var sourceEntries = reswDoc.Root.Elements("data").ToDictionary(e => e.Attribute("name").Value,
                                                            e => e.Element("value").Value);
            var xlfDoc = XDocument.Load(xlf);
            var targetEntries = xlfDoc.Descendants().Where(d => d.Name.LocalName == "trans-unit").
                                 ToDictionary(e => e.Attribute("id").Value.Substring(5).Replace('\', '.'),


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
                xlfDoc.Save(xlf);
        }
    }
}

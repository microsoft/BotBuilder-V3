// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
                Console.WriteLine("This is a tool to copy XML resource file information to the translation standard XLF file https://en.wikipedia.org/wiki/XLIFF.");
                Console.WriteLine("Will copy source resources to destination and show an error if any resource is not in destination.");
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

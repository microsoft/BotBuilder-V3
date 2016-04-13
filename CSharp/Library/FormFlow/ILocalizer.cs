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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace Microsoft.Bot.Builder.FormFlow.Advanced
{
    internal interface ILocalizer
    {
        /// <summary>
        /// Return the localizer culture.
        /// </summary>
        /// <returns>Current culture.</returns>
        CultureInfo Culture { get; set; }

        /// <summary>
        /// Translate a key to a translation.
        /// </summary>
        /// <param name="key">Key to lookup.</param>
        /// <returns>Translation.</returns>
        string Lookup(string key);

        /// <summary>
        /// Translate a key to a list of terms.
        /// </summary>
        /// <param name="key">Key to lookup.</param>
        /// <returns>List of translations.</returns>
        IEnumerable<string> LookupList(string key);

        #region Documentation
        /// <summary>   Enumerates template definitions. </summary>
        /// <param name="name"> The field name. </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process templates in this collection.
        /// </returns>
        #endregion
        IEnumerable<string> LookupPatterns(TemplateUsage usage, string name);

        /// <summary>
        /// Add a key and its translation to the localizer.
        /// </summary>
        /// <param name="key">Key for indexing translation.</param>
        /// <param name="translation">Translation for key.</param>
        void Add(string key, string translation);

        /// <summary>
        /// Add a key and a list of translations to the localizer.
        /// </summary>
        /// <param name="key">Key for indexing translation list.</param>
        /// <param name="list">List of translated terms.</param>
        void Add(string key, IEnumerable<string> list);

        #region Documentation
        /// <summary>   Add a template. </summary>
        /// <param name="template"> The template to add. </param>
        /// <remarks>Templates are special because they are shared but overridable per field.</remarks>
        #endregion
        void Add(TemplateAttribute template, string name);

        /// <summary>
        /// Remove a key from the localizer.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        void Remove(string key);

        /// <summary>
        /// Save localizer resource information.
        /// </summary>
        /// <param name="stream">Where to write resources.</param>
        void Save(Stream stream);

        /// <summary>
        /// Load the localizer from a stream.
        /// </summary>
        /// <param name="stream">Stream to load from.</param>
        /// <param name="missing">Keys found in current localizer that are not in loaded localizer.</param>
        /// <param name="extra">Keys found in loaded localizer that were not in current localizer.</param>
        /// <returns>New localizer for culture.</returns>
        ILocalizer Load(Stream stream, out IEnumerable<string> missing, out IEnumerable<string> extra);
    }

    internal class ResourceLocalizer : ILocalizer
    {
        public CultureInfo Culture { get; set; }

        public void Add(string key, IEnumerable<string> list)
        {
            _listTranslations.Add(key, list.ToArray());
        }

        public void Add(string key, string translation)
        {
            _translations.Add(key, translation);
        }

        public void Add(TemplateAttribute template, string name)
        {
            _templateTranslations.Add(name + SSEPERATOR + template.Usage, template.Patterns);
        }

        public ILocalizer Load(Stream stream, out IEnumerable<string> missing, out IEnumerable<string> extra)
        {
            var lmissing = new List<string>();
            var lextra = new List<string>();
            var newLocalizer = new ResourceLocalizer();
            using (var reader = new ResourceReader(stream))
            {
                foreach(DictionaryEntry entry in reader)
                {
                    var fullKey = (string) entry.Key;
                    var dot = fullKey.IndexOf(".");
                    var type = fullKey.Substring(0, dot);
                    var key = fullKey.Substring(dot + 1);
                    var val = (string)entry.Value;
                    if (type == "CULTURE")
                    {
                        newLocalizer.Culture = new CultureInfo(val);
                    }
                    else if (type == "VALUE")
                    {
                        newLocalizer.Add(key, val);
                    }
                    else if (type == "LIST")
                    {
                        newLocalizer.Add(key, SplitList(val).ToArray());
                    }
                    else if (type == "TEMPLATE")
                    {
                        var elements = SplitList(key);
                        var usage = elements.First();
                        var fields = elements.Skip(1);
                        var patterns = SplitList(val);
                        var template = new TemplateAttribute((TemplateUsage) Enum.Parse(typeof(TemplateUsage), usage), patterns.ToArray());
                        foreach(var field in fields)
                        {
                            newLocalizer.Add(template, field);
                        }
                    }
                }
            }

            // Find missing and extra keys
            lmissing.AddRange(_translations.Keys.Except(newLocalizer._translations.Keys));
            lmissing.AddRange(_listTranslations.Keys.Except(newLocalizer._listTranslations.Keys));
            lmissing.AddRange(_templateTranslations.Keys.Except(newLocalizer._templateTranslations.Keys));
            lextra.AddRange(newLocalizer._translations.Keys.Except(_translations.Keys));
            lextra.AddRange(newLocalizer._listTranslations.Keys.Except(_listTranslations.Keys));
            lextra.AddRange(newLocalizer._templateTranslations.Keys.Except(_templateTranslations.Keys));
            missing = lmissing;
            extra = lextra;
            return newLocalizer;
        }

        public void Remove(string key)
        {
            _translations.Remove(key);
            _listTranslations.Remove(key);
            _templateTranslations.Remove(key);
        }

        public void Save(Stream stream)
        {
            using (var writer = new ResourceWriter(stream))
            {
                writer.AddResource("CULTURE.", Culture.Name);
                // Switch from field;usage -> patterns
                // to usage;pattern* -> [fields]
                var byPattern = new Dictionary<string, List<string>>();
                foreach (var entry in _templateTranslations)
                {
                    var names = SplitList(entry.Key).ToArray();
                    var field = names[0];
                    var usage = names[1];
                    var key = MakeList(AddPrefix(usage, entry.Value));
                    List<string> fields;
                    if (byPattern.TryGetValue(key, out fields))
                    {
                        fields.Add(field);
                    }
                    else
                    {
                        byPattern.Add(key, new List<string> { field });
                    }
                }

                // Write out usage;field* -> pattern*
                foreach (var entry in byPattern)
                {
                    var elements = SplitList(entry.Key).ToArray();
                    var usage = elements[0];
                    var patterns = elements.Skip(1);
                    var key = "TEMPLATE." + MakeList(AddPrefix(usage, entry.Value));
                    writer.AddResource(key, MakeList(patterns));
                }

                foreach (var entry in _translations)
                {
                    writer.AddResource("VALUE." + entry.Key, entry.Value);
                }

                foreach (var entry in _listTranslations)
                {
                    writer.AddResource("LIST." + entry.Key, MakeList(entry.Value));
                }
            }
        }

        public string Lookup(string key)
        {
            string translation;
            _translations.TryGetValue(key, out translation);
            return translation;
        }

        public IEnumerable<string> LookupList(string key)
        {
            string[] translation;
            _listTranslations.TryGetValue(key, out translation);
            return translation;
        }

        public IEnumerable<string> LookupPatterns(TemplateUsage usage, string name)
        {
            string[] patterns;
            _templateTranslations.TryGetValue(MakeList(AddPrefix(name, new string[] { usage.ToString() })), out patterns);
            return patterns;
        }

        protected const char SEPERATOR = ';';
        protected const string SSEPERATOR = ";";
        protected const string ESCAPED_SEPERATOR = "_;_";

        protected IEnumerable<string> AddPrefix(string prefix, IEnumerable<string> suffix)
        {
            return new string[] { prefix }.Union(suffix);
        }

        protected string MakeList(IEnumerable<string> elements)
        {
            return string.Join(SSEPERATOR, from elt in elements select elt.Replace(SSEPERATOR, ESCAPED_SEPERATOR));
        }

        protected IEnumerable<string> SplitList(string str)
        {
            var elements = str.Split(SEPERATOR);
            return from elt in elements select elt.Replace(ESCAPED_SEPERATOR, SSEPERATOR);
        }

        protected Dictionary<string, string> _translations = new Dictionary<string, string>();
        protected Dictionary<string, string[]> _listTranslations = new Dictionary<string, string[]>();
        protected Dictionary<string, string[]> _templateTranslations = new Dictionary<string, string[]>();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    public interface ILocalizer
    {
        /// <summary>
        /// Return the localizer culture.
        /// </summary>
        /// <returns>Current culture.</returns>
        CultureInfo Culture();

        /// <summary>
        /// Translate a key to a translation.
        /// </summary>
        /// <param name="key">Key to lookup.</param>
        /// <returns>Translation.</returns>
        string Translate(string key);

        /// <summary>
        /// Translate a key to a list of terms.
        /// </summary>
        /// <param name="key">Key to lookup.</param>
        /// <returns>List of translations.</returns>
        IEnumerable<string> TranslateList(string key);

        /// <summary>
        /// Add a key and its translation to the localizer.
        /// </summary>
        /// <param name="key">Key for indexing translation.</param>
        /// <param name="translation">Translation for key.</param>
        /// <returns>The key.</returns>
        string Add(string key, string translation);

        /// <summary>
        /// Add a key and a list of translations to the localizer.
        /// </summary>
        /// <param name="key">Key for indexing translation list.</param>
        /// <param name="list">List of translated terms.</param>
        /// <returns>The key.</returns>
        string Add(string key, IEnumerable<string> list);

        /// <summary>
        /// Remove a key from the localizer.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        void Remove(string key);

        /// <summary>
        /// Save the localizer to a stream.
        /// </summary>
        /// <param name="stream">Stream to output to.</param>
        void Save(Stream stream);

        /// <summary>
        /// Load the localizer from a stream.
        /// </summary>
        /// <param name="culture">Culture being loaded.</param>
        /// <param name="stream">Stream to load from.</param>
        /// <param name="missing">Keys found in current localizer that are not in loaded localizer.</param>
        /// <param name="extra">Keys found in loaded localizer that were not in current localizer.</param>
        /// <returns>New localizer for culture.</returns>
        ILocalizer Load(string culture, Stream stream, out IEnumerable<string> missing, out IEnumerable<string> extra);
    }

    public class ResourceLocalizer : ILocalizer
    {
        public ResourceLocalizer(string culture)
        {
            _culture = CultureInfo.GetCultureInfo(culture);
        }

        public string Add(string key, IEnumerable<string> list)
        {
            _listTranslations.Add(key, list.ToArray());
            return key;
        }

        public string Add(string key, string translation)
        {
            _translations.Add(key, translation);
            return key;
        }

        public CultureInfo Culture()
        {
            return _culture;
        }

        public ILocalizer Load(string culture, Stream stream, out IEnumerable<string> missing, out IEnumerable<string> extra)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            _translations.Remove(key);
            _listTranslations.Remove(key);
        }

        public void Save(Stream stream)
        {
            throw new NotImplementedException();
        }

        public string Translate(string key)
        {
            string translation;
            _translations.TryGetValue(key, out translation);
            return translation;
        }

        public IEnumerable<string> TranslateList(string key)
        {
            string[] translation;
            _listTranslations.TryGetValue(key, out translation);
            return translation;
        }

        protected CultureInfo _culture;
        protected Dictionary<string, string> _translations = new Dictionary<string, string>();
        protected Dictionary<string, string[]> _listTranslations = new Dictionary<string, string[]>();
    }
}

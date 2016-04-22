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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Dialogs
{
    #region Documentation
    /// <summary>   Extensions for resources. </summary>
    #endregion
    public static partial class ResourceExtensions
    {
        /// <summary>   The separator character between elements in a string list. </summary>
        public const string SEPARATOR = ";";

        /// <summary>   When the <see cref="SEPARATOR"/> is found in a string list, the escaped replacement.</summary>
        public const string ESCAPED_SEPARATOR = "__semi";

        #region Documentation
        /// <summary>   Makes a string list. </summary>
        /// <param name="elements">     The elements to combine into a list. </param>
        /// <param name="separator">    The separator character between elements in a string list. </param>
        /// <param name="escape">       The escape string for separator characters. </param>
        /// <returns>   A string. </returns>
        #endregion
        public static string MakeList(IEnumerable<string> elements, string separator = SEPARATOR, string escape = ESCAPED_SEPARATOR)
        {
            return string.Join(separator, from elt in elements select elt.Replace(separator, escape));
        }

        #region Documentation
        /// <summary>   Makes a list from parameters. </summary>
        /// <param name="elements"> The elements to combine into a list. </param>
        /// <returns>   A string. </returns>
        #endregion
        public static string MakeList(params string[] elements)
        {
            return MakeList(elements.AsEnumerable());
        }

        #region Documentation
        /// <summary>   A string extension method that splits a list. </summary>
        /// <param name="str">          The str to act on. </param>
        /// <param name="separator">    The separator character between elements in a string list. </param>
        /// <param name="escape">       The escape string for separator characters. </param>
        /// <returns>   A string[]. </returns>
        #endregion
        public static string[] SplitList(this string str, string separator = SEPARATOR, string escape = ESCAPED_SEPARATOR)
        {
            var elements = str.Split(separator[0]);
            return (from elt in elements select elt.Replace(escape, separator)).ToArray();
        }
    }
}

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public static partial class Extensions
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }
    }
}

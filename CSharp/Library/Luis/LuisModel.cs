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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Luis
{
    /// <summary>
    /// A mockable interface for the LUIS model.
    /// </summary>
    public interface ILuisModel
    {
        /// <summary>
        /// The LUIS model ID.
        /// </summary>
        string ModelID { get; }

        /// <summary>
        /// The LUIS subscription key.
        /// </summary>
        string SubscriptionKey { get; }
    }

    /// <summary>
    /// The LUIS model information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    [Serializable]
    public class LuisModelAttribute : Attribute, ILuisModel, IEquatable<ILuisModel>
    {
        private readonly string modelID;
        public string ModelID => modelID;

        private readonly string subscriptionKey;
        public string SubscriptionKey => subscriptionKey;

        /// <summary>
        /// Construct the LUIS model information.
        /// </summary>
        /// <param name="modelID">The LUIS model ID.</param>
        /// <param name="subscriptionKey">The LUIS subscription key.</param>
        public LuisModelAttribute(string modelID, string subscriptionKey)
        {
            SetField.NotNull(out this.modelID, nameof(modelID), modelID);
            SetField.NotNull(out this.subscriptionKey, nameof(subscriptionKey), subscriptionKey);
        }

        public bool Equals(ILuisModel other)
        {
            return other != null
                && object.Equals(this.ModelID, other.ModelID)
                && object.Equals(this.SubscriptionKey, other.SubscriptionKey)
                ;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as ILuisModel);
        }

        public override int GetHashCode()
        {
            return ModelID.GetHashCode() ^ SubscriptionKey.GetHashCode();
        }
    }
}

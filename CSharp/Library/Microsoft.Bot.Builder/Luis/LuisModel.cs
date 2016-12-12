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
    /// Luis api version.
    /// </summary>
    public enum LuisApiVersion
    {
        V1,
        V2
    }

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

        /// <summary>
        /// The base Uri for accessing LUIS.
        /// </summary>
        Uri UriBase { get; }

        /// <summary>
        /// Luis Api Version.
        /// </summary>
        LuisApiVersion ApiVersion { get; }
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

        private readonly Uri uriBase; 
        public Uri UriBase => uriBase;

        private readonly LuisApiVersion apiVersion;
        public LuisApiVersion ApiVersion => apiVersion;

        public static readonly IReadOnlyDictionary<LuisApiVersion, Uri> LuisEndpoints = new Dictionary<LuisApiVersion, Uri>()
        {
            {LuisApiVersion.V1, new Uri("https://api.projectoxford.ai/luis/v1/application")},
            {LuisApiVersion.V2, new Uri("https://api.projectoxford.ai/luis/v2.0/apps/")}
        };
        
        /// <summary>
        /// Construct the LUIS model information.
        /// </summary>
        /// <param name="modelID">The LUIS model ID.</param>
        /// <param name="subscriptionKey">The LUIS subscription key.</param>
        /// <param name="apiVersion">The LUIS API version.</param>
        public LuisModelAttribute(string modelID, string subscriptionKey, LuisApiVersion apiVersion = LuisApiVersion.V2)
        {
            SetField.NotNull(out this.modelID, nameof(modelID), modelID);
            SetField.NotNull(out this.subscriptionKey, nameof(subscriptionKey), subscriptionKey);
            this.apiVersion = apiVersion;
            this.uriBase = LuisEndpoints[this.apiVersion];
        }

        public bool Equals(ILuisModel other)
        {
            return other != null
                && object.Equals(this.ModelID, other.ModelID)
                && object.Equals(this.SubscriptionKey, other.SubscriptionKey)
                && object.Equals(this.ApiVersion, other.ApiVersion)
                && object.Equals(this.UriBase, other.UriBase)
                ;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as ILuisModel);
        }

        public override int GetHashCode()
        {
            return ModelID.GetHashCode() 
                ^ SubscriptionKey.GetHashCode() 
                ^ UriBase.GetHashCode() 
                ^ ApiVersion.GetHashCode();
        }
    }
}

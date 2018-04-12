using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Connector
{
    public partial class MediaCard
    {
        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Image != null)
            {
                Image.Validate();
            }
            if (Media != null)
            {
                foreach (var element in Media)
                {
                    if (element != null)
                    {
                        element.Validate();
                    }
                }
            }
        }
    }
}

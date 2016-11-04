using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public static partial class Extensions
    {
        /// <summary>
        /// Creates a new attachment from <see cref="HeroCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="HeroCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this HeroCard card)
        {
            return new Attachment
            {
                Content = card,
                ContentType = HeroCard.ContentType
            };
        }

        /// <summary>
        /// Creates a new attachment from <see cref="ThumbnailCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="ThumbnailCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this ThumbnailCard card)
        {
            return new Attachment
            {
                Content = card,
                ContentType = ThumbnailCard.ContentType
            };
        }

        /// <summary>
        /// Creates a new attachment from <see cref="SigninCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="SigninCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this SigninCard card)
        {
            return new Attachment
            {
                Content = card,
                ContentType = SigninCard.ContentType
            };
        }

        /// <summary>
        /// Creates a new attachment from <see cref="ReceiptCard"/>.
        /// </summary>
        /// <param name="card"> The instance of <see cref="ReceiptCard"/>.</param>
        /// <returns> The generated attachment.</returns>
        public static Attachment ToAttachment(this ReceiptCard card)
        {
            return new Attachment
            {
                Content = card,
                ContentType = ReceiptCard.ContentType
            };
        }
    }
}

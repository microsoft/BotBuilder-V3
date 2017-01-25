using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Base
{
    /// <summary>
    /// This class sets the ambient thread culture for a scope of code with a using-block.
    /// </summary>
    public struct LocalizedScope : IDisposable
    {
        private readonly CultureInfo previousCulture;
        private readonly CultureInfo previousUICulture;

        public LocalizedScope(string locale)
        {
            this.previousCulture = Thread.CurrentThread.CurrentCulture;
            this.previousUICulture = Thread.CurrentThread.CurrentUICulture;

            if (!string.IsNullOrWhiteSpace(locale))
            {
                CultureInfo found = null;
                try
                {
                    found = CultureInfo.GetCultureInfo(locale);
                }
                catch (CultureNotFoundException)
                {
                }

                if (found != null)
                {
                    Thread.CurrentThread.CurrentCulture = found;
                    Thread.CurrentThread.CurrentUICulture = found;
                }
            }
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = previousCulture;
            Thread.CurrentThread.CurrentUICulture = previousUICulture;
        }
    }
}

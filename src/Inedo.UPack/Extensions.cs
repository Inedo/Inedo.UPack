using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.UPack
{
    internal static class Extensions
    {
        public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var registration = cancellationToken.Register(request.Abort);
            try
            {
                return (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            }
            finally
            {
                registration.Dispose();
            }
        }
    }
}

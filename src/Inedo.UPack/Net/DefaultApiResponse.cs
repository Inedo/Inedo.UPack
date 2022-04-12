using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Default implementation of <see cref="ApiResponse"/> which encapsulates a
    /// <see cref="HttpResponseMessage"/> object.
    /// </summary>
    public class DefaultApiResponse : ApiResponse
    {
        private readonly HttpResponseMessage response;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiResponse"/> class.
        /// </summary>
        /// <param name="response">The web response to encapsulate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is null.</exception>
        public DefaultApiResponse(HttpResponseMessage response) => this.response = response ?? throw new ArgumentNullException(nameof(response));

        public override string ContentType => this.response.Content.Headers.ContentType?.ToString() ?? string.Empty;
        public override int StatusCode => (int)this.response.StatusCode;

        public override Task<Stream> GetResponseStreamAsync(CancellationToken cancellationToken = default)
        {
#if NETSTANDARD2_0
            return this.response.Content.ReadAsStreamAsync();
#else
            return this.response.Content.ReadAsStreamAsync(cancellationToken);
#endif
        }
        public override void ThrowIfNotSuccessful() => this.response.EnsureSuccessStatusCode();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.response.Dispose();

            base.Dispose(disposing);
        }
    }
}

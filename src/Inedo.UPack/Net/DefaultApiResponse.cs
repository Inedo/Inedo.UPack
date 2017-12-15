using System;
using System.IO;
using System.Net;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Default implementation of <see cref="ApiResponse"/> which encapsulates a
    /// <see cref="HttpWebResponse"/> object.
    /// </summary>
    public class DefaultApiResponse : ApiResponse
    {
        private readonly HttpWebResponse response;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiResponse"/> class.
        /// </summary>
        /// <param name="response">The web response to encapsulate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is null.</exception>
        public DefaultApiResponse(HttpWebResponse response) => this.response = response ?? throw new ArgumentNullException(nameof(response));

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ContentType => this.response.ContentType;

        public override Stream GetResponseStream() => this.response.GetResponseStream();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.response.Dispose();

            base.Dispose(disposing);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}

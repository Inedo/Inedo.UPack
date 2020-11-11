using System.IO;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Specifies the information required to make a upack feed API request.
    /// </summary>
    public sealed class ApiRequest
    {
        internal ApiRequest(UniversalFeedEndpoint endpoint, string relativeUrl, string method = "GET", string? contentType = null, Stream? requestBody = null)
        {
            this.RelativeUrl = relativeUrl;
            this.Method = method;
            this.ContentType = contentType;
            this.RequestBody = requestBody;
            this.Endpoint = endpoint;
        }

        /// <summary>
        /// Gets the URL relative to the feed API endpoint.
        /// </summary>
        public string RelativeUrl { get; }
        /// <summary>
        /// Gets the HTTP method which should be used to make the request.
        /// </summary>
        public string Method { get; }
        /// <summary>
        /// Gets the Content-Type of the data supplied in the <see cref="RequestBody"/> stream.
        /// </summary>
        public string? ContentType { get; }
        /// <summary>
        /// Gets the data to write to the request body. May be null.
        /// </summary>
        public Stream? RequestBody { get; }
        /// <summary>
        /// Gets the feed API endpoint information.
        /// </summary>
        public UniversalFeedEndpoint Endpoint { get; }
    }
}

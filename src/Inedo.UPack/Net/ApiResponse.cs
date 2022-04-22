namespace Inedo.UPack.Net
{
    /// <summary>
    /// Represents a response received from a upack API endpoint.
    /// </summary>
    public abstract class ApiResponse : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse"/> class.
        /// </summary>
        protected ApiResponse()
        {
        }

        /// <summary>
        /// Gets the Content-Type of the response.
        /// </summary>
        public abstract string ContentType { get; }
        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        public abstract int StatusCode { get; }

        /// <summary>
        /// Gets the response content body as a stream.
        /// </summary>
        /// <returns>Stream backed by the response content body.</returns>
        public abstract Task<Stream> GetResponseStreamAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Raises an exception if the response was not successful.
        /// </summary>
        public abstract void ThrowIfNotSuccessful();
        /// <summary>
        /// Releases resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the instance.
        /// </summary>
        /// <param name="disposing">Value indicating whether managed resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Represents the transport layer used for making upack API requests.
    /// </summary>
    public abstract class ApiTransport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTransport"/> class.
        /// </summary>
        protected ApiTransport()
        {
        }

        /// <summary>
        /// When implemented in a derived class, makes the API request specified.
        /// </summary>
        /// <param name="request">The API request.</param>
        /// <param name="cancellationToken">Cancellation token used to cancel the request.</param>
        /// <returns>API response.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is null.</exception>
        public abstract Task<ApiResponse> GetResponseAsync(ApiRequest request, CancellationToken cancellationToken);
    }
}

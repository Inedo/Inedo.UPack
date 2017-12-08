using System;
using System.Security;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Represents a remote universal feed endpoint.
    /// </summary>
    [Serializable]
    public sealed class UniversalFeedEndpoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFeedEndpoint"/> class.
        /// </summary>
        /// <param name="uri">The API endpoint URL.</param>
        /// <param name="useDefaultCredentials">Value indicating whether current user credentials should be included in requests. This must be true if using Windows Authentication.</param>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not http or https.</exception>
        public UniversalFeedEndpoint(Uri uri, bool useDefaultCredentials)
        {
            this.Uri = uri ?? throw new ArgumentNullException(nameof(uri));

            if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Universal feed uri must use http or https.");

            this.UseDefaultCredentials = useDefaultCredentials;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFeedEndpoint"/> class.
        /// </summary>
        /// <param name="uri">The API endpoint URL.</param>
        /// <param name="useDefaultCredentials">Value indicating whether current user credentials should be included in requests. This must be true if using Windows Authentication.</param>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not http or https.</exception>
        public UniversalFeedEndpoint(string uri, bool useDefaultCredentials)
            : this(new Uri(uri), useDefaultCredentials)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFeedEndpoint"/> class.
        /// </summary>
        /// <param name="uri">The API endpoint URL.</param>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not http or https.</exception>
        public UniversalFeedEndpoint(Uri uri)
            : this(uri, false)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFeedEndpoint"/> class.
        /// </summary>
        /// <param name="uri">The API endpoint URL.</param>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not http or https.</exception>
        public UniversalFeedEndpoint(string uri)
            : this(uri, false)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFeedEndpoint"/> class.
        /// </summary>
        /// <param name="uri">The API endpoint URL.</param>
        /// <param name="userName">User name to use for basic authentication.</param>
        /// <param name="password">Password to use for basic authentication.</param>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null or <paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="uri"/> is not http or https.</exception>
        public UniversalFeedEndpoint(Uri uri, string userName, SecureString password)
            : this(uri, false)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            this.UserName = userName;
            this.Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        /// <summary>
        /// Gets the feed API URL.
        /// </summary>
        public Uri Uri { get; }
        /// <summary>
        /// Gets a value indicating whether current user credentials should be included in requests.
        /// </summary>
        public bool UseDefaultCredentials { get; }
        /// <summary>
        /// Gets the user name for basic authentication.
        /// </summary>
        public string UserName { get; }
        /// <summary>
        /// Gets the password for basic authentication.
        /// </summary>
        public SecureString Password { get; }

        /// <summary>
        /// Returns some identifying information abou the feed endpoint.
        /// </summary>
        /// <returns>Feed endpoint information.</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.UserName))
                return this.UserName + " @ " + this.Uri;
            else
                return this.Uri.ToString();
        }
    }
}

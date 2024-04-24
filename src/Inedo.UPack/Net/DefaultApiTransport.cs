using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Inedo.UPack.Net
{
    /// <summary>
    /// Default implementation of <see cref="ApiTransport"/> which uses <see cref="HttpWebRequest"/> to
    /// communicate with a remote feed.
    /// </summary>
    public class DefaultApiTransport : ApiTransport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiTransport"/> class.
        /// </summary>
        public DefaultApiTransport()
        {
        }

        /// <summary>
        /// Gets or sets the User Agent string to use when making requests.
        /// </summary>
        public string? UserAgent { get; set; }
        /// <summary>
        /// Gets or sets the delegate used to create a <see cref="HttpClient"/> instance.
        /// </summary>
        /// <remarks>
        /// When <c>null</c>, the default internal factory is used.
        /// </remarks>
        public Func<ApiRequest, HttpClient>? HttpClientFactory { get; set; }

        public override async Task<ApiResponse> GetResponseAsync(ApiRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using var message = BuildRequestMessage(request);
            HttpContent? content = null;
            try
            {
                if (request.RequestBody != null)
                {
                    content = new StreamContent(request.RequestBody);
                    message.Content = content;
                }

                if (!string.IsNullOrEmpty(request.ContentType) && content != null)
                    content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
                if (!string.IsNullOrEmpty(this.UserAgent))
                    message.Headers.UserAgent.ParseAdd(this.UserAgent);

                var client = this.GetHttpClient(request);
                var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                return new DefaultApiResponse(response);
            }
            finally
            {
                content?.Dispose();
            }
        }

        /// <summary>
        /// Returns a standard Base64-encoded HTTP basic authentication token containing the specified user name and password.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>Base64-encoded HTTP basic authentication token.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="userName"/> is null or empty or <paramref name="password"/> is null.</exception>
        protected static string GetBasicAuthToken(string userName, SecureString password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var utf8 = new UTF8Encoding(false);
            unsafe
            {
                var bstr = (byte*)Marshal.SecureStringToBSTR(password).ToPointer();
                try
                {
                    int length = *(int*)(bstr - 4) / 2;

                    int passwordByteCount = utf8.GetByteCount((char*)bstr, length);

                    var bytes = new byte[utf8.GetByteCount(userName) + 1 + passwordByteCount];
                    try
                    {
                        int n = utf8.GetBytes(userName.ToCharArray(), 0, userName.Length, bytes, 0);
                        bytes[n] = (byte)':';

                        fixed (byte* bytesPtr = bytes)
                        {
                            utf8.GetBytes((char*)bstr, length, bytesPtr + n + 1, passwordByteCount);
                        }

                        return Convert.ToBase64String(bytes);
                    }
                    finally
                    {
                        Array.Clear(bytes, 0, bytes.Length);
                    }
                }
                finally
                {
                    Marshal.ZeroFreeBSTR(new IntPtr(bstr));
                }
            }
        }

        protected static HttpRequestMessage BuildRequestMessage(ApiRequest r)
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r));

            var url = r.Endpoint.Uri.ToString();
            if (!url.EndsWith("/"))
                url += "/";
            url += r.RelativeUrl;

            var message = new HttpRequestMessage(new HttpMethod(r.Method), url);
            //default to using API Key if it exists
            if (!string.IsNullOrEmpty(r.Endpoint.APIKey))
            {
                message.Headers.Add("X-ApiKey", r.Endpoint.APIKey);
            }
            else if (!string.IsNullOrEmpty(r.Endpoint.UserName) && r.Endpoint.Password != null)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Basic", GetBasicAuthToken(r.Endpoint.UserName!, r.Endpoint.Password));
            }

            return message;
        }

        protected HttpClient GetHttpClient(ApiRequest r)
        {
            var client = this.HttpClientFactory?.Invoke(r) ?? InternalHttpClientFactory.Instance.GetClient(r);

            if (this.Timeout.HasValue)
                client.Timeout = this.Timeout.GetValueOrDefault();

            return client;
        }
    }
}

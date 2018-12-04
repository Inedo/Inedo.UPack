using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public string UserAgent { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override async Task<ApiResponse> GetResponseAsync(ApiRequest request, CancellationToken cancellationToken)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var webRequest = BuildWebRequest(request);
            webRequest.Method = request.Method;
            webRequest.ContentType = request.ContentType;
            if (!string.IsNullOrEmpty(this.UserAgent))
                webRequest.UserAgent = this.UserAgent;

            if (request.RequestBody != null)
            {
                webRequest.AllowWriteStreamBuffering = false;
                using (var requestStream = await webRequest.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    await request.RequestBody.CopyToAsync(requestStream, 81920, cancellationToken).ConfigureAwait(false);
                }
            }

            var webResponse = await webRequest.GetResponseAsync(cancellationToken).ConfigureAwait(false);
            return new DefaultApiResponse(webResponse);
        }

        /// <summary>
        /// Returns a <see cref="HttpWebRequest"/> to use based on the specified <see cref="ApiRequest"/>.
        /// </summary>
        /// <param name="r">The desired request.</param>
        /// <returns>Valid <see cref="HttpWebRequest"/> object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="r"/> is null.</exception>
        protected static HttpWebRequest BuildWebRequest(ApiRequest r)
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r));

            var url = r.Endpoint.Uri.ToString();
            if (!url.EndsWith("/"))
                url += "/";
            url += r.RelativeUrl;

            var request = WebRequest.CreateHttp(url);
            if (r.Endpoint.UseDefaultCredentials)
                request.UseDefaultCredentials = true;
            else if (!string.IsNullOrEmpty(r.Endpoint.UserName) && r.Endpoint.Password != null)
                request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + GetBasicAuthToken(r.Endpoint.UserName, r.Endpoint.Password));

            return request;
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
    }
}

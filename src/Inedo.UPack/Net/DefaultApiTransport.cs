using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.UPack.Net
{
    internal class DefaultApiTransport : ApiTransport
    {
        public override async Task<ApiResponse> GetResponseAsync(ApiRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var webRequest = BuildWebRequest(request);
            webRequest.Method = request.Method;
            webRequest.ContentType = request.ContentType;

            if (request.RequestBody != null)
            {
                using (var requestStream = await webRequest.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    await request.RequestBody.CopyToAsync(requestStream, 81920, cancellationToken).ConfigureAwait(false);
                }
            }

            var webResponse = await webRequest.GetResponseAsync(cancellationToken).ConfigureAwait(false);
            return new DefaultApiResponse(webResponse);
        }

        protected static HttpWebRequest BuildWebRequest(ApiRequest r)
        {
            var url = r.Endpoint.Uri.ToString();
            if (!url.EndsWith("/"))
                url += "/";
            url += r.RelativeUrl;

            var request = WebRequest.CreateHttp(url);
            if (r.Endpoint.UseDefaultCredentials)
                request.UseDefaultCredentials = true;
            else if (!string.IsNullOrEmpty(r.Endpoint.UserName) && r.Endpoint.Password != null)
                request.Headers.Add("Basic " + GetBasicAuthToken(r.Endpoint.UserName, r.Endpoint.Password));

            return request;
        }
        protected static string GetBasicAuthToken(string userName, SecureString password)
        {
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

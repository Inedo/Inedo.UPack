using System;
using System.IO;
using System.Net;

namespace Inedo.UPack.Net
{
    internal class DefaultApiResponse : ApiResponse
    {
        private readonly HttpWebResponse response;

        protected internal DefaultApiResponse(HttpWebResponse response)
        {
            this.response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public override string ContentType => this.response.ContentType;

        public override Stream GetResponseStream() => this.response.GetResponseStream();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.response.Dispose();

            base.Dispose(disposing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Inedo.UPack.Net
{
    public sealed class UniversalFeedException : Exception
    {
        public UniversalFeedException(int statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public int? StatusCode { get; }
    }
}

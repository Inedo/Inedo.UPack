using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Inedo.UPack.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inedo.UPack.Tests
{
    [TestClass]
    public class HttpClientFactoryTests
    {
        [TestMethod]
        public async Task HttpClientLifetime()
        {
            var factory = new InternalHttpClientFactory { StaleTicks = 0 };

            await MakeRequestAsync(factory);
            await MakeRequestAsync(factory);

            GC.Collect(3, GCCollectionMode.Forced, true);
            factory.RunCleanup();

            await MakeRequestAsync(factory);
            GC.Collect(3, GCCollectionMode.Forced, true);
            factory.RunCleanup();

            await MakeRequestAsync(factory);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task CertificateCallbackWireup()
        {
            var client = new UniversalFeedClient(new UniversalFeedEndpoint("https://proget.inedo.com/upack/Extensions"));

            bool called = false;

            ServicePointManager.ServerCertificateValidationCallback = callback;
            try
            {
                _ = await client.ListPackagesAsync("inedox", 1);
                Assert.IsTrue(called);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = null;
            }

            bool callback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
            {
                called = true;
                return true;
            }
        }

        private static async Task MakeRequestAsync(InternalHttpClientFactory factory)
        {
            var client = new UniversalFeedClient(
                new UniversalFeedEndpoint("https://proget.inedo.com/upack/Extensions"),
                new DefaultApiTransport { HttpClientFactory = factory.GetClient }
            );

            _ = await client.ListPackagesAsync("inedox", 1);
        }
    }
}

using System;
using System.IO;
using Inedo.UPack.Packaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inedo.UPack.Tests
{
    [TestClass]
    public class MetadataTests
    {
        [TestMethod]
        public void EnsureNoDateMangling()
        {
            // make sure date strings are persisted to JSON exactly as they should be, without any intermediate format conversion

            var metadata = new UniversalPackageMetadata();
            var nowText = DateTimeOffset.Now.ToString("o");

            var createdDate = DateTimeOffset.Now.AddDays(-1);
            metadata.CreatedDate = createdDate;

            metadata["rubbishDate"] = nowText;
            metadata.Name = "name";
            metadata.Version = new UniversalPackageVersion(1, 0, 0);

            using var tempStream = new MemoryStream();

            using (var builder = new UniversalPackageBuilder(tempStream, metadata, true))
            {
            }

            tempStream.Position = 0;

            using var package = new UniversalPackage(tempStream);
            var readMetadata = package.GetFullMetadata();
            Assert.AreEqual(nowText, readMetadata["rubbishDate"]);
            Assert.AreEqual(createdDate, readMetadata.CreatedDate);
            Assert.AreEqual(createdDate.ToString("o"), readMetadata["createdDate"]);
        }
    }
}

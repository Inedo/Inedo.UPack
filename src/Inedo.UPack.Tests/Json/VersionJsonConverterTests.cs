using Inedo.UPack.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Inedo.UPack.Tests.Json
{
    [TestClass]
    public class VersionJsonConverterTests
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters =
            {
                new UniversalPackageVersionJsonConverter(),
            }
        };

        [TestMethod]
        public void TestVersion()
        {
            List<UniversalPackageVersion> expectedVersions = new()
            {
                new(1, 2, 3),
                new(1, 2, 3, "rc1", null),
                new(1, 2, 3, null, "3429"),
                new(1, 2, 3, "rc1", "3429"),
                null,
            };
            var json = @"[
                ""1.2.3"",
                ""1.2.3-rc1"",
                ""1.2.3+3429"",
                ""1.2.3-rc1+3429"",
                null
            ]";
            var parsed = JsonSerializer.Deserialize<List<UniversalPackageVersion>>(json, _options);

            CollectionAssert.AreEqual(expectedVersions, parsed);
        }

        [TestMethod]
        [DataRow(1, 2, 3, null, null)]
        [DataRow(1, 2, 3, "rc1", null)]
        [DataRow(1, 2, 3, "rc1", "3429")]
        [DataRow(1, 2, 3, null, "3429")]
        public void TestVersionReversible(int major, int minor, int patch, string prerelease, string build)
        {
            UniversalPackageVersion version = new(major, minor, patch, prerelease, build);

            var json = JsonSerializer.Serialize(version, _options);
            var parsed = JsonSerializer.Deserialize<UniversalPackageVersion>(json, _options);

            Assert.AreEqual(version, parsed);
        }
    }
}

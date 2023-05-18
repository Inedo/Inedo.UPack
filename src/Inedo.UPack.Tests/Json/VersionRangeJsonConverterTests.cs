using Inedo.UPack.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Inedo.UPack.Tests.Json
{
    [TestClass]
    public class VersionRangeJsonConverterTests
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters =
            {
                new UniversalPackageVersionJsonConverter(),
                new UniversalPackageVersionRangeJsonConverter(),
            }
        };

        [TestMethod]
        public void TestVersionRange()
        {
            List<UniversalPackageVersionRange> expectedVersions = new()
            {
                UniversalPackageVersionRange.Any,
                new UniversalPackageVersionRange(new(1, 2, 3)),
                new UniversalPackageVersionRange(new(1, 0, 0), false, new(2, 0, 0), true),
                new UniversalPackageVersionRange(new(3, 0, 0), false, null, true),
                UniversalPackageVersionRange.Any,
            };
            var json = @"[
                ""*"",
                ""1.2.3"",
                ""[1.0.0,2.0.0)"",
                ""[3.0.0,]"",
                null
            ]";
            var parsed = JsonSerializer.Deserialize<List<UniversalPackageVersionRange>>(json, _options);

            CollectionAssert.AreEqual(expectedVersions, parsed);
        }

        [TestMethod]
        [DataRow("1.2.3", false, null, false)]
        [DataRow("1.0.0", false, "2.0.0", true)]
        public void TestVersionRangeReversible(string lowerBound, bool lowerExclusive, string upperBound, bool upperExclusive)
        {
            UniversalPackageVersion lb = parseOrDefault(lowerBound);
            UniversalPackageVersion ub = parseOrDefault(upperBound);
            UniversalPackageVersionRange version = new(lb, lowerExclusive, ub, upperExclusive);

            var json = JsonSerializer.Serialize(version, _options);
            var parsed = JsonSerializer.Deserialize<UniversalPackageVersionRange>(json, _options);

            Assert.AreEqual(version, parsed);

            static UniversalPackageVersion parseOrDefault(string value)
            {
                return value is null ? null : UniversalPackageVersion.Parse(value);
            }
        }
    }
}

using Inedo.UPack.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Inedo.UPack.Tests.Json
{
    [TestClass]
    public class DependencyJsonConverterTests
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters =
            {
                new UniversalPackageDependencyJsonConverter(),
            }
        };

        [TestMethod]
        public void TestDependency()
        {
            List<UniversalPackageDependency> expectedDependencies = new()
            {
                new(null, "myname", UniversalPackageVersionRange.Any),
                new("mygroup", "myname", UniversalPackageVersionRange.Any),
                new(null, "myname", new UniversalPackageVersion(1, 2, 3)),
                new(null, "myname", new UniversalPackageVersionRange(new(3,0,0), false, null, false)),
                null,
            };
            var json = @"[
                ""myname"",
                ""mygroup/myname"",
                ""myname:1.2.3"",
                ""myname:[3.0.0,]"",
                null
            ]";
            var parsed = JsonSerializer.Deserialize<List<UniversalPackageDependency>>(json, _options);

            CollectionAssert.AreEqual(expectedDependencies, parsed);
        }

        [TestMethod]
        [DataRow("mygroup", "myname", null)]
        [DataRow("mygroup", "myname", "*")]
        [DataRow("mygroup", "myname", "3.0.1")]
        [DataRow("mygroup", "myname", "[3.0.0,]")]
        [DataRow("mygroup", "myname", "[1.0.0,2.0.0)")]
        public void TestDependencyReversible(string group, string name, string versionRange)
        {
            var vrange = versionRange is null ? null : UniversalPackageVersionRange.TryParse(versionRange, out var vr) ? vr : null;

            UniversalPackageDependency id = new(group, name, vrange);

            var json = JsonSerializer.Serialize(id, _options);
            var parsed = JsonSerializer.Deserialize<UniversalPackageDependency>(json, _options);

            Assert.AreEqual(id, parsed);
        }
    }
}

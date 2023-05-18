using Inedo.UPack.Json;
using Inedo.UPack.Packaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Inedo.UPack.Tests.Json
{
    [TestClass]
    public class MetadataJsonConverterTests
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters =
            {
                new UniversalPackageDependencyJsonConverter(),
                new UniversalPackageIdJsonConverter(),
                new UniversalPackageMetadataJsonConverter(),
                new UniversalPackageVersionJsonConverter(),
            }
        };

        [TestMethod]
        public void TestMetadataConverter()
        {
            UniversalPackageMetadata m1 = new()
            {
                Name = "myname",
                Group = "mygroup",
                Version = new(1, 2, 3, "pre1"),
            };
            m1.Dependencies.Add(new("mygroup", "package1", UniversalPackageVersionRange.Any));
            m1.Dependencies.Add(new("mygroup", "package2", UniversalPackageVersionRange.Any));
            m1.Dependencies.Add(new("mygroup", "package3", new UniversalPackageVersionRange(new(1, 0, 0), false, new(2, 0, 0), true)));
            UniversalPackageMetadata m2 = new()
            {
                Name = "myname2",
                Group = "mygroup2",
                ["_custom"] = true,
            };

            List<UniversalPackageMetadata> expected = new()
            {
                m1, m2
            };


            string json = TestResources.MetadataList;

            var parsed = JsonSerializer.Deserialize<List<UniversalPackageMetadata>>(json, _options);

            CollectionAssert.Equals(expected, parsed);
        }

        [TestMethod]
        public void TestMetadataConverterReversible()
        {
            Dictionary<string, object> originalCustomProperty = new()
            {
                { "prop1", "value1" },
                { "prop2", "value2" },
            };
            UniversalPackageMetadata original = new()
            {
                Group = "mygroup",
                Name = "myname",
                Version = new(3, 2, 0, "rc1"),
                ["_customProperty"] = originalCustomProperty,
            };
            original.Dependencies.Add(new("mygroup", "package1", UniversalPackageVersionRange.Any));
            
            var json = JsonSerializer.Serialize(original, _options);

            var parsed = JsonSerializer.Deserialize<UniversalPackageMetadata>(json, _options);

            Assert.AreEqual(original.Group, parsed.Group);
            Assert.AreEqual(original.Name, parsed.Name);
            Assert.AreEqual(original.Version, parsed.Version);

            var customProperty = parsed["_customProperty"] as Dictionary<string, object>;
            Assert.IsNotNull(customProperty);

            CollectionAssert.AreEquivalent(originalCustomProperty, customProperty);
        }
    }
}

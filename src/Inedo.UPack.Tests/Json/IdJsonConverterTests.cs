using Inedo.UPack.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Inedo.UPack.Tests.Json
{
    [TestClass]
    public class IdJsonConverterTests
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters =
            {
                new UniversalPackageIdJsonConverter(),
            }
        };

        [TestMethod]
        public void TestId()
        {
            List<UniversalPackageId> expectedIds = new()
            {
                new("myname"),
                new("mygroup", "myname"),
                null,
            };
            var json = @"[
                ""myname"",
                ""mygroup/myname"",
                null
            ]";
            var parsed = JsonSerializer.Deserialize<List<UniversalPackageId>>(json, _options);

            CollectionAssert.AreEqual(expectedIds, parsed);
        }

        [TestMethod]
        [DataRow("mygroup", "myname")]
        [DataRow(null, "myname")]
        public void TestIdReversible(string group, string name)
        {
            UniversalPackageId id = new(group, name);

            var json = JsonSerializer.Serialize(id, _options);
            var parsed = JsonSerializer.Deserialize<UniversalPackageId>(json, _options);

            Assert.AreEqual(id, parsed);
        }
    }
}

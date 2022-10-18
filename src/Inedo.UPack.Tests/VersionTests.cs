using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inedo.UPack.Tests
{
    [TestClass]
    public class VersionTests
    {
        [DataTestMethod]
        [DataRow(1, 0, 0, null, null, null, "1.0.0")]
        [DataRow(1, 0, 0, null, null, "G", "1.0.0")]
        [DataRow(1, 0, 0, null, null, "U", "1.0.0")]
        [DataRow(1, 0, 0, "abc", null, "G", "1.0.0-abc")]
        [DataRow(1, 0, 0, "abc", null, "U", "1.0.0-abc")]
        [DataRow(1, 0, 0, null, "bld", null, "1.0.0+bld")]
        [DataRow(1, 0, 0, null, "bld", "G", "1.0.0+bld")]
        [DataRow(1, 0, 0, null, "bld", "U", "1.0.0")]
        public void TestSpanFormat(int major, int minor, int patch, string prerelease, string build, string format, string result)
        {
            var buffer = new char[1024];
            var ver = new UniversalPackageVersion(major, minor, patch, prerelease, build);
            Assert.IsTrue(ver.TryFormat(buffer, out int length, format != null ? format.AsSpan() : default));
            Assert.AreEqual(result, buffer.AsSpan(0, length).ToString());
        }

        [TestMethod]
        public void TestSpanFormatTooSmall()
        {
            var buffer = new char[2];
            var ver = new UniversalPackageVersion(1, 0, 0);
            Assert.IsFalse(ver.TryFormat(buffer, out _));
        }
    }
}

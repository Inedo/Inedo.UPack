using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inedo.UPack.Tests
{
    [TestClass]
    public class VersionRangeTests
    {
        [TestMethod]
        public void CreateAny()
        {
            Assert.AreEqual(UniversalPackageVersionRange.Any, new UniversalPackageVersionRange());
            Assert.AreEqual("*", UniversalPackageVersionRange.Any.ToString());
        }
        [TestMethod]
        public void ParseAny()
        {
            var v = UniversalPackageVersionRange.Parse("*");
            Assert.AreEqual(UniversalPackageVersionRange.Any, v);
            Assert.IsNull(v.LowerBound);
            Assert.IsNull(v.UpperBound);
        }

        [TestMethod]
        public void CreateSingle()
        {
            var ver = new UniversalPackageVersion(1, 0, 0);
            var v = new UniversalPackageVersionRange(ver);
            Assert.AreNotEqual(UniversalPackageVersionRange.Any, v);
            Assert.AreEqual(ver, v.LowerBound);
            Assert.AreEqual(ver, v.UpperBound);
            Assert.AreEqual(ver.ToString(), v.ToString());
        }
        [TestMethod]
        public void ParseSingle()
        {
            var ver = new UniversalPackageVersion(1, 0, 0);
            var v = UniversalPackageVersionRange.Parse("1.0.0");
            Assert.AreNotEqual(UniversalPackageVersionRange.Any, v);
            Assert.AreEqual(v.LowerBound, ver);
            Assert.AreEqual(v.UpperBound, ver);
        }

        [TestMethod]
        public void CreateRange()
        {
            var ver1 = new UniversalPackageVersion(1, 0, 0);
            var ver2 = new UniversalPackageVersion(2, 0, 0);
            var range = new UniversalPackageVersionRange(ver1, false, ver2, true);
            Assert.AreEqual(ver1, range.LowerBound);
            Assert.IsFalse(range.LowerExclusive);
            Assert.AreEqual(ver2, range.UpperBound);
            Assert.IsTrue(range.UpperExclusive);
            Assert.AreEqual("[1.0.0,2.0.0)", range.ToString());
        }
        [TestMethod]
        public void ParseRange()
        {
            var ver1 = new UniversalPackageVersion(1, 0, 0);
            var ver2 = new UniversalPackageVersion(2, 0, 0);
            var v = UniversalPackageVersionRange.Parse("[1.0.0,2.0.0)");
            Assert.AreEqual(ver1, v.LowerBound);
            Assert.IsFalse(v.LowerExclusive);
            Assert.AreEqual(ver2, v.UpperBound);
            Assert.IsTrue(v.UpperExclusive);
        }

        [TestMethod]
        public void ParseOpenEndedRange1()
        {
            var ver1 = new UniversalPackageVersion(1, 0, 0);
            var v = UniversalPackageVersionRange.Parse("[1.0.0,]");
            Assert.AreEqual(ver1, v.LowerBound);
            Assert.IsFalse(v.LowerExclusive);
            Assert.IsNull(v.UpperBound);
        }
        [TestMethod]
        public void ParseOpenEndedRange2()
        {
            var ver1 = new UniversalPackageVersion(1, 0, 0);
            var v = UniversalPackageVersionRange.Parse("[,1.0.0]");
            Assert.AreEqual(ver1, v.UpperBound);
            Assert.IsFalse(v.UpperExclusive);
            Assert.IsNull(v.LowerBound);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseInvalid()
        {
            UniversalPackageVersionRange.Parse("[1.0.0");
        }
    }
}

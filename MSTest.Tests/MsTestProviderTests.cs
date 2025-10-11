#if !NET
using DLaB.Xrm.Test.MSTest;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NET
namespace DataverseUnitTest.MSTest.Tests
#else
namespace XrmUnitTest.MSTest.Tests
#endif
{
    [TestClass]
    public class MsTestProviderTests
    {
        private MsTestProvider _provider = null!;

        [TestInitialize]
        public void Setup()
        {
            _provider = new MsTestProvider();
        }

        [TestMethod]
        public void TestMethodAttributeType_ShouldBeTestMethodAttribute()
        {
            Assert.AreEqual(typeof(TestMethodAttribute), _provider.TestMethodAttributeType);
        }

        [TestMethod]
        public void GetFailedException_ShouldReturnAssertFailedException()
        {
            var message = "Failure";
            var ex = _provider.GetFailedException(message);
            Assert.IsInstanceOfType(ex, typeof(AssertFailedException));
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod]
        public void GetInconclusiveException_ShouldReturnAssertInconclusiveException()
        {
            var message = "Inconclusive";
            var ex = _provider.GetInconclusiveException(message);
            Assert.IsInstanceOfType(ex, typeof(AssertInconclusiveException));
            Assert.AreEqual(message, ex.Message);
        }
    }
}

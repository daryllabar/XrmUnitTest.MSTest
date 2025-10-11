#if NET
using DLaB.Xrm;
#else
using DLaB.Xrm;
using DLaB.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
#endif
using Microsoft.Xrm.Sdk;
using System.Xml.Linq;

#if NET
namespace DataverseUnitTest.MSTest.Tests
#else
namespace XrmUnitTest.MSTest.Tests
#endif
{
    [TestClass]
    public class AssertTests
    {
        private const string NoBreakSpace = "\u00a0";
        private const string ExpectedMessage = "This should have failed";
        private static readonly Guid ExpectedGuid = new("BA58029C-AA97-4846-87AD-2BFD46F05949");
        private const string UnexpectedMessage = "This message was not expected";
        private static readonly Guid UnexpectedGuid = new("DC785B7B-4DD2-4FDC-A2A9-943531E5DFE4");

        #region AreCloseEnough

        [TestMethod]
        [DataRow(100, 100, false, DisplayName = "Delay is greater than Max, Should Assert")]
        [DataRow(99, 100, true, DisplayName = "Delay is within Max, Should Not Assert")]
        public void AreCloseEnough(long delay, long maxDelay, bool isValid)
        {
            var now = DateTime.UtcNow;
            var later = now.AddMilliseconds(delay);

            try
            {
                Assert.That.AreCloseEnough(now, later, maxDelay);
                Assert.IsTrue(isValid);
            }
            catch (AssertFailedException ex)
            {
                Assert.IsFalse(isValid, $"Expected AreCloseEnough not to fail, but it failed with: {ex.Message}.");
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.AreCloseEnough(Assert assert", traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.AreCloseEnough(", traces[2]);
            }
        }

        [TestMethod]
        public void AreCloseEnough_Null_Should_Assert()
        {
            try
            {
                Assert.That.AreCloseEnough(DateTime.UtcNow, null);
            }
            catch (AssertFailedException ex)
            {
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.AreCloseEnough(Assert assert", traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.AreCloseEnough_Null_Should_Assert(", traces[2]);
            }
        }

        #endregion AreCloseEnough

        #region AttributesAreEqual

        [TestMethod]
        public void AttributesAreEqual_NullInputs_Should_AssertIfNotEqual()
        {
            Assert.That.AttributesAreEqual(null, null);

            try
            {
                Assert.That.AttributesAreEqual(null, new Entity());
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual("Assert.AttributesAreEqual failed.  Expected null but actual was not null.", ex.Message);
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.AttributesAreEqual(Assert assert", traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.AttributesAreEqual_NullInputs_Should_AssertIfNotEqual(", traces[2]);
            }

            try
            {
                Assert.That.AttributesAreEqual(new Entity(), null);
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual("Assert.AttributesAreEqual failed.  Actual was null.", ex.Message);
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.AttributesAreEqual(Assert assert", traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.AttributesAreEqual_NullInputs_Should_AssertIfNotEqual(", traces[2]);
            }

            try
            {
                Assert.That.AttributesAreEqual(new Entity { ["A"] = null }, new Entity());
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual("Assert.AttributesAreEqual failed.  Attribute A was expected but not found!", ex.Message);
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.AttributesAreEqual(Assert assert", traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.AttributesAreEqual_NullInputs_Should_AssertIfNotEqual(", traces[2]);
            }

            try
            {
                Assert.That.AttributesAreEqual(new Entity { ["A"] = null }, new Entity { ["A"] = "Not NULL" });
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual("Assert.AttributesAreEqual failed.  Expected attribute: \"A\" to be null, but actual was \"Not NULL\".", ex.Message);
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.AttributesAreEqual(Assert assert", traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.AttributesAreEqual_NullInputs_Should_AssertIfNotEqual(", traces[2]);
            }

            try
            {
                Assert.That.AttributesAreEqual(new Entity { ["A"] = "Not NULL" }, new Entity { ["A"] = null });
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual("Assert.AttributesAreEqual failed.  Expected attribute: \"A\" with value: \"Not NULL\" but actual was null.", ex.Message);
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.AttributesAreEqual(Assert assert", traces[1], traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.AttributesAreEqual_NullInputs_Should_AssertIfNotEqual(", traces[2]);
            }
        }

        [TestMethod]
        [DataRow("Boolean", "true but actual was false.", DisplayName = "Different Booleans")]
        [DataRow("Byte", "{length: 1} but actual was {length: 1}.", DisplayName = "Different Bytes")]
        [DataRow("DateTime", "\"1/1/2025 12:00:00 AM\" (638712864000000000) but actual was \"1/2/2025 12:00:00 AM\" (638713728000000000).", DisplayName = "Different Dates")]
        [DataRow("Dictionary", "{\r\n  a: \"This should have failed\"\r\n} but actual was {\r\n  a: \"This message was not expected\"\r\n}.", DisplayName = "Different Dictionary  ")]
        [DataRow("Entity", "{} but actual was {}.", DisplayName = "Different Entity")]
        [DataRow("EntityCollection", "[\r\n  {}\r\n] but actual was [\r\n  {}\r\n].", DisplayName = "Different EntityCollection")]
        [DataRow("EntityReference", "{LogicalName: \"a\", Id: \"00000000-0000-0000-0000-000000000000\"} but actual was {LogicalName: \"b\", Id: \"00000000-0000-0000-0000-000000000000\"}.", DisplayName = "Different EntityReference")]
        [DataRow("EntityReferenceCollection", "[\r\n  {LogicalName: \"\", Id: \"ba58029c-aa97-4846-87ad-2bfd46f05949\"}\r\n] but actual was [\r\n  {LogicalName: \"\", Id: \"dc785b7b-4dd2-4fdc-a2a9-943531e5dfe4\"}\r\n].", DisplayName = "Different EntityReferenceCollection")]
        [DataRow("IEnumerable", "  {\r\n  \"List`1\": [\r\n    \"1\"\r\n  ]\r\n} but actual was   {\r\n  \"List`1\": [\r\n    \"2\"\r\n  ]\r\n}.", DisplayName = "Different IEnumerable")]
        [DataRow("Money", "1 but actual was 2.", DisplayName = "Different Money")]
        [DataRow("OptionSetValue", "1 but actual was 2.", DisplayName = "Different OptionSetValue")]
        public void AttributesAreEqual_InvalidAttributes_Should_Assert(string column, string message)
        {
            var expected = new Entity
            {
                ["Boolean"] = true,
                ["Byte"] = new byte[] { 1 },
                ["DateTime"] = new DateTime(2025, 1, 1),
                ["Dictionary"] = new Dictionary<string, string> { { "a", ExpectedMessage } },
                ["Entity"] = new Entity { Id = ExpectedGuid },
                ["EntityCollection"] = new EntityCollection(new List<Entity> { new() { Id = ExpectedGuid } }),
                ["EntityReference"] = new EntityReference("a"),
                ["EntityReferenceCollection"] = new EntityReferenceCollection(new List<EntityReference> { new() { Id = ExpectedGuid } }),
                ["IEnumerable"] = new List<string> { "1" },
                ["Money"] = new Money(1),
                ["OptionSetValue"] = new OptionSetValue(1),
            };

            var actual = new Entity
            {
                ["Boolean"] = false,
                ["Byte"] = new byte[] { 2 },
                ["DateTime"] = new DateTime(2025, 1, 2),
                ["Dictionary"] = new Dictionary<string, string> { { "a", UnexpectedMessage } },
                ["Entity"] = new Entity { Id = UnexpectedGuid },
                ["EntityCollection"] = new EntityCollection(new List<Entity> { new() { Id = UnexpectedGuid } }),
                ["EntityReference"] = new EntityReference("b"),
                ["EntityReferenceCollection"] = new EntityReferenceCollection(new List<EntityReference> { new() { Id = UnexpectedGuid } }),
                ["IEnumerable"] = new List<string> { "2" },
                ["Money"] = new Money(2),
                ["OptionSetValue"] = new OptionSetValue(2),
            };

            try
            {
                Assert.That.AttributesAreEqual(new Entity { [column] = expected[column] }, new Entity { [column] = actual[column] });
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual($"Assert.AttributesAreEqual failed.  Expected attribute: \"{column}\" with value: {message}", ex.Message.Replace(NoBreakSpace, " "));
            }
        }

        [TestMethod]
        public void AttributesAreEqual_ValidAttributes_Should_NotAssert()
        {
            TestBase.UseLocalCrmDatabase = false;
            var entityRef = new EntityReference("A", Guid.NewGuid());
            Assert.That.AttributesAreEqual(null, null);
            var expected = new Entity()
            {
                Id = Guid.NewGuid(),
                ["string"] = "A",
                ["int"] = 12345,
                ["EntityReference"] = entityRef,
                ["null"] = null,
                ["DateTime"] = DateTime.UtcNow,
                ["Boolean"] = true,
                ["OptionSetValue"] = new OptionSetValue(1),
                ["Money"] = new Money(123.45m),
                ["EntityCollection"] = new EntityCollection(new[] { new Entity("B") })
            };
            var actual = new Entity();
            foreach (var kvp in expected.Attributes)
            {
                actual[kvp.Key] = kvp.Value;
            }
            actual["EntityReference"] = new EntityReference(entityRef.LogicalName, entityRef.Id);
            actual["DateTime"] = actual.GetAttributeValue<DateTime>("DateTime").Date;
            Assert.That.AttributesAreEqual(expected, actual);
            TestBase.UseLocalCrmDatabase = true;
        }

        #endregion AttributesAreEqual

        #region ExpectExecutionToThrowException

        [TestMethod]
        public void ExpectExecutionToThrowException_PluginThrowsExpectedException_Should_NotAssert()
        {
            new PluginExample().ExpectExecutionToThrowException(null!);
        }

        [TestMethod]
        public void ExpectExecutionToThrowException_PluginThrowsNoException_Should_Assert()
        {
            try
            {
                new PluginExample().ExpectExecutionToThrowException(new FakeServiceProvider());
            }
            catch(AssertFailedException ex)
            {
                Assert.AreEqual("Assert.ThrowsException failed.  Expected exception of type InvalidPluginExecutionException to be thrown.", ex.Message);
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(4, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.ThrowsException[TException](Assert assert, Action action, String message) in ", traces[1], traces[1]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.ExpectExecutionToThrowException(IPlugin plugin, IServiceProvider serviceProvider, String message) in ", traces[2], traces[2]);
                Assert.Contains(".MSTest.Tests.AssertTests.ExpectExecutionToThrowException_PluginThrowsNoException_Should_Assert(", traces[3]);
            }
        }

        [TestMethod]
        public void ExpectExecutionToThrowException_PluginThrowsUnexpectedException_Should_Throw()
        {
            try
            {
                var provider = new FakeServiceProvider();
                provider.AddService(nameof(ExpectExecutionToThrowException_PluginThrowsUnexpectedException_Should_Throw));
                new PluginExample().ExpectExecutionToThrowException(provider);
                Assert.Fail("Expected Exception");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(nameof(ExpectExecutionToThrowException_PluginThrowsUnexpectedException_Should_Throw), ex.Message);
            }

            try
            {
                new PluginExample().ExpectExecutionToThrowException(null!, ExpectedMessage);
                Assert.Fail("Expected Exception");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Assert.AreEqual("Service Provider was null!", ex.Message);
            }
        }

        #endregion ExpectExecutionToThrowException

        #region IsNotNullOrWhiteSpace

        [TestMethod]
        public void IsNotNullOrWhiteSpace_Null_Should_Assert()
        {
            try
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Assert.That.IsNotNullOrWhiteSpace(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual("Assert.IsNotNullOrWhiteSpace failed.  No values were passed in.", ex.Message);
                var traces = ex.StackTrace!.Split([Environment.NewLine], StringSplitOptions.None);
                Assert.HasCount(3, traces);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.Fail(String name, String message) in ", traces[0]);
                Assert.StartsWith($"   at Microsoft.VisualStudio.TestTools.UnitTesting.MsTestExtensions.IsNotNullOrWhiteSpace(Assert assert", traces[1]);
                Assert.Contains(".MSTest.Tests.AssertTests.IsNotNullOrWhiteSpace_Null_Should_Assert(", traces[2]);
            }
        }

        [TestMethod]
        public void IsNotNullOrWhiteSpace()
        {
            Assert.That.IsNotNullOrWhiteSpace();
            Assert.That.IsNotNullOrWhiteSpace("A");
            Assert.That.IsNotNullOrWhiteSpace("A", "B");
        }

        [TestMethod]
        [DataRow(new[] { "", "A", "B" }, 0, DisplayName = "First Item Empty")]
        [DataRow(new[] { null, "A", "B" }, 0, DisplayName = "First Item Null")]
        [DataRow(new[] { "A", "B", "" }, 2, DisplayName = "Last Item Empty")]
        [DataRow(new[] { "A", "B", null }, 2, DisplayName = "Last Item Null")]
        [DataRow(new[] { "A", "", "B" }, 1, DisplayName = "Middle Item Empty")]
        [DataRow(new[] { "A", null, "B" }, 1, DisplayName = "Middle Item Null")]
        [DataRow(new object?[] { null }, -1, DisplayName = "Single Item Null")]
        [DataRow(new[] { "" }, -1, DisplayName = "Single Item Empty")]
        public void IsNotNullOrWhiteSpace_WithNulOrWhiteSpace_Should_Fail(object?[] objects, int invalidIndex)
        {
            try
            {
                Assert.That.IsNotNullOrWhiteSpace(objects);
                Assert.Fail("IsNotNullOrWhiteSpace was expected to fail.");
            }
            catch (AssertFailedException ex)
            {
                if (invalidIndex == -1)
                {
                    Assert.AreEqual("Assert.IsNotNullOrWhiteSpace failed.", ex.Message);
                }
                else
                {
                    Assert.AreEqual($"Assert.IsNotNullOrWhiteSpace failed.  The parameter at index \"{invalidIndex}\" was null or white space.", ex.Message);
                }
            }
        }

        #endregion IsNotNullOrWhiteSpace

        #region ThrowsException

        [TestMethod]
        public void ThrowsException_ActionThrowsExpectedExceptions_Should_NotAssert()
        {
            Assert.That.ThrowsException<InvalidOperationException>(() => throw new InvalidOperationException("Test Exception"));
            Assert.That.ThrowsException<InvalidOperationException>(() => throw new InvalidOperationException(ExpectedMessage), ExpectedMessage);
        }

        [TestMethod]
        public void ThrowsException_ActionDoesNotThrow_Should_Assert()
        {
            // Without Message
            try
            {
                Assert.That.ThrowsException<InvalidOperationException>(() => { });
                Assert.Fail("Expected ThrowsException to fail, but it passed.");
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual("Assert.ThrowsException failed.  Expected exception of type InvalidOperationException to be thrown.", ex.Message);
            }

            // With Message
            try
            {
                Assert.That.ThrowsException<InvalidOperationException>(() => { }, ExpectedMessage);
                Assert.Fail("Expected ThrowsException to fail, but it passed.");
            }
            catch (AssertFailedException ex)
            {
                Assert.AreEqual($"Assert.ThrowsException failed.  Expected exception of type InvalidOperationException to be thrown with message \"{ExpectedMessage}\".", ex.Message);
            }
        }

        [TestMethod]
        public void ThrowsException_ActionThrowsUnexpectedMessage_Should_Assert()
        {
            try
            {
                Assert.That.ThrowsException<InvalidOperationException>(() => throw new InvalidOperationException(UnexpectedMessage), ExpectedMessage);
                Assert.Fail("Expected ThrowsException to fail, but it passed.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(UnexpectedMessage, ex.Message);
            }
        }

        [TestMethod]
        public void ThrowsException_ActionThrowsUnexpectedType_Should_Assert()
        {
            try
            {
                Assert.That.ThrowsException<InvalidOperationException>(() => throw new FileNotFoundException());
                Assert.Fail("Expected ThrowsException to fail, but it passed.");
            }
            catch (FileNotFoundException)
            {
                // This was expected!
            }
        }

        #endregion ThrowsException

        #region ThrowsPluginException

        [TestMethod]
        public void ThrowsPluginException_ActionThrowsUnexpectedType_Should_Assert()
        {
            Assert.That.ThrowsPluginException(() => throw new InvalidPluginExecutionException());
            Assert.That.ThrowsPluginException(() => throw new InvalidPluginExecutionException(ExpectedMessage), ExpectedMessage);
        }

        #endregion ThrowsPluginException

        private class PluginExample : IPlugin {
            public void Execute(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                {
                    throw new InvalidPluginExecutionException("Service Provider was null!");
                }

                var error = serviceProvider.GetService<string>();
                if(error != null) { 
                    throw new InvalidOperationException(error);
                }
            }
        }
    }
}

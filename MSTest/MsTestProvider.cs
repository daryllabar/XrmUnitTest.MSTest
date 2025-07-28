using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET

namespace DataverseUnitTest.MSTest
#else
using System;

namespace DLaB.Xrm.Test.MSTest
#endif
{
    /// <summary>
    /// Provides the implementation of <see cref="ITestFrameworkProvider"/> for MSTest.
    /// </summary>
    public class MsTestProvider : ITestFrameworkProvider
    {
        /// <summary>
        /// Used to Disable Live Unit Testing (see https://learn.microsoft.com/en-us/visualstudio/test/live-unit-testing)
        /// </summary>
        public const string SkipWhenLiveUnitTesting = "SkipWhenLiveUnitTesting";

        /// <summary>
        /// If only one Test Attribute Type will be used, gets the type of the attribute used to define a test method.  If more than one is to be used, this must be null and the IMultiTestMethodAttributeTestFrameworkProvider should be used with the TestMethodAttributeTypes populated.
        /// </summary>
        /// <value>
        /// the type of the attribute used to define a test method.
        /// </value>
        public Type TestMethodAttributeType => typeof(TestMethodAttribute);

        /// <summary>
        /// Exception to throw when a custom Assertion has failed.  MsTest: AssertFailedException
        /// </summary>
        /// <returns></returns>
        public Exception GetFailedException(string message)
        {
            return new AssertFailedException(message);
        }

        /// <summary>
        /// Exception to throw when a custom Assertion is inconclusive.
        /// </summary>
        /// <returns></returns>
        public Exception GetInconclusiveException(string message)
        {
            return new AssertInconclusiveException(message);
        }
    }
}

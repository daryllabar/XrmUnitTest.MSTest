#if !NET
using DLaB.Xrm.Test.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using DLaB.Xrm.LocalCrm;
using System;
using System.Linq;
using System.Reflection;

#if NET
namespace DataverseUnitTest.MSTest.Tests
#else
namespace XrmUnitTest.MSTest.Tests
#endif
{
    [TestClass]
    public class LocalTestBaseTests
    {
        [TestMethod]
        public void CreateLocalService_ShouldExposeStaticOptionsOverload()
        {
            var method = typeof(LocalTestBase<,,,>).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .SingleOrDefault(m =>
                {
                    if (m.Name != nameof(LocalTestBase<,,,>.CreateLocalService))
                    {
                        return false;
                    }

                    var parameters = m.GetParameters();
                    return parameters.Length == 2
                           && parameters[0].ParameterType == typeof(Action<LocalCrmDatabaseInfo>)
                           && parameters[1].ParameterType == typeof(ITestLogger);
                });

            Assert.IsNotNull(method);
            Assert.IsTrue(method.GetParameters()[1].HasDefaultValue);
            Assert.IsNull(method.GetParameters()[1].DefaultValue);
        }
    }
}

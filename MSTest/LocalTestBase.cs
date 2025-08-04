using System.Reflection;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter;
using DLaB.Xrm.Ioc;
using DLaB.Xrm.LocalCrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

#if NET
using DLaB.Xrm;
using DataverseUnitTest.Assumptions;
#else
using System.Collections.Generic;
using System.Linq;
using DLaB.Xrm.Test.Assumptions;
#endif
#if NET

namespace DataverseUnitTest.MSTest
#else
using System;

namespace DLaB.Xrm.Test.MSTest
#endif
{
    /// <summary>
    /// Base class for implementing Local CRM Tests using MSTest.
    /// </summary>
    /// <typeparam name="TCrmEnvironmentBuilder">The Crm Environment Builder</typeparam>
    /// <typeparam name="TDataverseContext">The EarlyBound DataverseContext</typeparam>
    /// <typeparam name="TBusinessUnit">The EarlyBound BusinessUnit</typeparam>
    /// <typeparam name="TSystemUser">The SystemUser</typeparam>
    public abstract class LocalTestBase<TCrmEnvironmentBuilder, TDataverseContext, TBusinessUnit, TSystemUser>
        where TCrmEnvironmentBuilder : Builders.CrmEnvironmentBuilderBase<TCrmEnvironmentBuilder>, new()
        where TBusinessUnit : Entity
        where TSystemUser : Entity
        where TDataverseContext : OrganizationServiceContext
    {
        /// <summary>
        /// Contains all Id instances in the Ids struct of the class.  Useful for being able to enumerate through all the Ids.
        /// </summary>
        protected IEnumerable<Id> IdsList => IdsById.Values;
        private Dictionary<Guid, Id> IdsById { get; set; } = null!;

        protected AssumedEntities AssumedEntities { get; set; } = null!;

        protected IIocContainer Container { get; private set; } = null!;

        /// <summary>
        /// Exposes an CrmEnvironmentBuilder that can be used to reflectively associate records without having to specify the join, as well as create the records in the correct order, even handling circular references.
        /// </summary>
        protected TCrmEnvironmentBuilder EnvBuilder { get; private set; } = null!;

        /// <summary>
        /// The Business Unit
        /// </summary>
        protected Id<TBusinessUnit> CurrentBusinessUnit { get; private set; } = null!;

        /// <summary>
        /// The Userid of the current Service
        /// </summary>
        protected Id<TSystemUser> CurrentUser { get; private set; } = null!;

        private DateTime? _utcNow;
        /// <summary>
        /// A UtcNow value that doesn't change throughout the test.
        /// </summary>
        protected DateTime Now => _utcNow ?? (_utcNow = DateTime.UtcNow).Value;
        /// <summary>
        /// A UtcNow without milliseconds (since CRM trims Milliseconds) value that doesn't change throughout the test.
        /// </summary>
        protected DateTime NowSansMilliseconds => Now.RemoveMilliseconds();

        /// <summary>
        /// Test specific (isolated) organization service.
        /// </summary>
        protected IOrganizationService Service { get; set; } = null!;

        public TestContext TestContext { get; set; } = null!;

        private DateTime? _today;
        /// <summary>
        /// The local current date for the user at midnight
        /// </summary>
        protected virtual DateTime Today => _today ?? (_today = DateTime.Today).Value;
        /// <summary>
        /// Tomorrow's date for the user at midnight
        /// </summary>
        protected DateTime Tomorrow => Today.AddDays(1);
        /// <summary>
        /// Fake Tracing Service that can be used to assert that the correct messages were logged.
        /// </summary>
        protected FakeTraceService TracingService { get; set; } = null!;
#if !PRE_MULTISELECT
        /// <summary>
        /// Fake Managed Identity Service that can be used to assert token acquisition in tests.
        /// </summary>
        protected FakeManagedIdentityService ManagedIdentityService { get; set; } = null!;
#endif
        /// <summary>
        /// Yesterday's date for the user at midnight
        /// </summary>
        protected DateTime Yesterday => Today.AddDays(-1);

        /// <summary>
        /// Data Driven tests will have the same test name, so to ensure separate LocalCrmDatabases, a unique id is appended to the test name.
        /// </summary>
        private readonly string _testId = "_" + Guid.NewGuid();
        private string TestId => TestContext.TestName + _testId;

        private readonly TestLogger _logger = new();

        /// <summary>
        /// Initializes the class before each test run.  First utilize overriding Initialize() before resorting to overriding this method.
        /// </summary>
        [TestInitialize]
        public virtual void PreTest()
        {
            _logger.Enabled = false;
            InitializeTestSettings();
            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            IdsById ??= GetType().GetTypeInfo().DeclaredNestedTypes.SelectMany(types => types.GetIds()).ToDictionary(k => k.EntityId, v => v);
            TestBase.LoadUserUnitTestSettings();
            Service = CreateService();
            CurrentBusinessUnit = new Id<TBusinessUnit>(Service.GetFirst<TBusinessUnit>().Id);
            CurrentUser = new Id<TSystemUser>(Service.GetCurrentlyExecutingUserInfo().UserId);
            TracingService = new FakeTraceService(_logger);
#if !PRE_MULTISELECT
            ManagedIdentityService = new FakeManagedIdentityService();
#endif
            EnvBuilder = new TCrmEnvironmentBuilder();
            PreInitialize();
            Initialize();
            _logger.Enabled = true;
        }

        /// <summary>
        /// Should contain a call to TestInitializer.InitializeTestSettings();
        /// </summary>
        protected abstract void InitializeTestSettings();

        private void PreInitialize()
        {
            AssumedEntities = AssumedEntities.Load(Service, GetType());
            Container = RegisterLocalTestServices(new IocContainer());
            Container.AddSingleton(Service);
        }

        /// <summary>
        /// Override to register any local test services that are not registered by default.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        protected virtual IIocContainer RegisterLocalTestServices(IIocContainer container)
        {
            return container
                .AddSingleton(_ => new ExtendedOrganizationServiceSettings
                {
                    LogDetailedRequests = false,
                    ProxyTypesAssembly = typeof(TDataverseContext).Assembly,
                })
                .AddScoped(_ => Service);
        }

        [TestCleanup]
        public void PreTestCleanup()
        {
            // Ids are generally instantiated at the class level, so they will need to be cleared out after each test
            foreach (var kvp in IdsById)
            {
                // Remove all attributes.  Future test initializer will repopulate
                kvp.Value.Entity.Attributes.Clear();

                // Reset the Id to repopulate the attribute
                kvp.Value.Entity.Id = kvp.Key;
            }

            TestCleanup();
        }

        /// <summary>
        /// Override to clean up any test specific data
        /// </summary>
        protected virtual void TestCleanup()
        {
        }

        /// <summary>
        /// Override to define creating a Service.  Defaults to CreateLocalService
        /// </summary>
        protected virtual IOrganizationService CreateService()
        {
            return CreateLocalService(TestId, _logger);
        }

        /// <summary>
        /// Override to initialize any test specific data
        /// </summary>
        protected virtual void Initialize()
        {

        }


        /// <summary>
        /// Ensures that the given action throws an InvalidPluginExecutionException (optionally with the given message) when executed.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        protected void ExpectPluginException(Action action, string? message = null)
        {
            Assert.That.ThrowsPluginException(action, message);
        }

        public static IOrganizationService CreateLocalService<T>(string test, ITestLogger? logger = null) where T : class
        {
            return CreateLocalService(typeof(T).FullName + "|" + test, logger);
        }

        public static IOrganizationService CreateLocalService(string testId, ITestLogger? logger = null)
        {
            return new FakeIOrganizationService(new LocalCrmDatabaseOrganizationService(LocalCrmDatabaseInfo.Create<TDataverseContext>(testId)), logger ?? new TestLogger());
        }
    }
}
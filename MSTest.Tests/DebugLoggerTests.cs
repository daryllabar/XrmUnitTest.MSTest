using System.Diagnostics;
#if NET
using DataverseUnitTest.MSTest;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DLaB.Xrm.Test.MSTest;
#endif

namespace XrmUnitTest.MSTest.Tests
{
    [TestClass]
    public class DebugLoggerTests
    {
        private class DebugListenerStub : TraceListener
        {
            public List<string> Messages { get; } = new();


#if NET
            public override void Write(string? message)
#else
            public override void Write(string message)
#endif
            {
                Messages.Add(message!);
            }

#if NET
            public override void WriteLine(string? message)
#else
            public override void WriteLine(string message)
#endif
            {
                Messages.Add(message!);
            }
        }

        private DebugListenerStub _listener = null!;

        [TestInitialize]
        public void Setup()
        {
            _listener = new DebugListenerStub();
#if NET
            Trace.Listeners.Add(_listener);
#else
            Debug.Listeners.Add(_listener);
#endif
        }

        [TestCleanup]
        public void Cleanup()
        {
#if NET
            Trace.Listeners.Remove(_listener);
#else
            Debug.Listeners.Remove(_listener);
#endif
        }

        [TestMethod]
        public void WriteLine_ShouldWriteMessage_WhenEnabled()
        {
            var logger = new DebugLogger { Enabled = true };
            logger.WriteLine("Test message");
            Assert.IsTrue(_listener.Messages.Contains("Test message"));
        }

        [TestMethod]
        public void WriteLine_ShouldNotWriteMessage_WhenDisabled()
        {
            var logger = new DebugLogger { Enabled = false };
            logger.WriteLine("Test message");
            Assert.IsFalse(_listener.Messages.Contains("Test message"));
        }

        [TestMethod]
        public void WriteLine_Format_ShouldWriteFormattedMessage_WhenEnabled()
        {
            var logger = new DebugLogger { Enabled = true };
            logger.WriteLine("Hello {0}", "World");
            Assert.IsTrue(_listener.Messages.Exists(m => m.Contains("Hello World")));
        }

        [TestMethod]
        public void WriteLine_Format_ShouldNotWriteFormattedMessage_WhenDisabled()
        {
            var logger = new DebugLogger { Enabled = false };
            logger.WriteLine("Hello {0}", "World");
            Assert.IsFalse(_listener.Messages.Exists(m => m.Contains("Hello World")));
        }
    }
}

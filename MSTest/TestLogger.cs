using System.Diagnostics;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

#if NET

namespace DataverseUnitTest.MSTest
#else
using System;

namespace DLaB.Xrm.Test.MSTest
#endif
{
    /// <summary>
    /// Provides a logger implementation that writes output to the debug window.
    /// </summary>
    public class TestLogger : ITestLogger
    {
        /// <summary>
        /// Gets or sets a value indicating whether logging is enabled.  It is useful to disable logging while test setup is running, to avoid cluttering the debug output with setup messages.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Writes a message to the debug output if logging is enabled.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteLine(string message)
        {
            if (!Enabled)
            {
                return;
            }
            Trace.WriteLine(message);
            Trace.WriteLine("");
        }

        /// <summary>
        /// Writes a formatted message to the debug output if logging is enabled.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        public void WriteLine(string format, params object[] args)
        {
            if (!Enabled)
            {
                return;
            }
            Trace.WriteLine(string.Format(format, args));
            Trace.WriteLine("");
        }
    }
}
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;

using System;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter
{
    /// <summary>
    /// Provides extension methods for MSTest assertions and CRM-related testing utilities.
    /// </summary>
    public static class MsTestExtensions
    {
        #region Asserts

        /// <summary>
        /// Asserts that the given values are within <paramref name="maxMillisecondsDifference"/> milliseconds of each other.
        /// Useful when a plugin may call <see cref="DateTime.UtcNow"/> multiple times, and you want to assert that the value is close enough to the expected value.
        /// </summary>
        /// <param name="assert">The <see cref="Assert"/> instance.</param>
        /// <param name="expected">The expected <see cref="DateTime"/> value.</param>
        /// <param name="actual">The actual <see cref="DateTime"/> value to compare. If <c>null</c>, the assertion fails.</param>
        /// <param name="maxMillisecondsDifference">The maximum allowed difference in milliseconds. Default is 100ms.</param>
        /// <param name="message">Optional. The message to include if the assertion fails.</param>
        public static void AreCloseEnough(this Assert assert, DateTime expected, DateTime? actual, long maxMillisecondsDifference = 100L, string? message = null)
        {
            if (actual == null)
            {
                Fail(nameof(AreCloseEnough), message ?? $"{nameof(actual)} should not be null");
                return;
            }

            if (!IsCloseEnough(expected, actual.Value, out var diff, maxMillisecondsDifference))
            {
                Fail(nameof(AreCloseEnough), message ?? $"Expected '{actual}' to be within {maxMillisecondsDifference}ms of '{expected}'.  Actual was {diff.TotalMilliseconds}ms");
            }
        }

        private static bool IsCloseEnough(DateTime expected, DateTime actual, out TimeSpan diff, long maxMillisecondsDifference)
        {
            diff = TimeSpan.FromTicks(Math.Abs(expected.Ticks - actual.Ticks));
            return diff.TotalMilliseconds < maxMillisecondsDifference;
        }

        /// <summary>
        /// Asserts that the specified action throws an exception of type <typeparamref name="TException"/>.
        /// Optionally checks the exception message.
        /// </summary>
        /// <typeparam name="TException">The type of exception expected.</typeparam>
        /// <param name="assert">The Assert instance.</param>
        /// <param name="action">The action expected to throw the exception.</param>
        /// <param name="message">Optional. The expected exception message.</param>
        public static void ThrowsException<TException>(this Assert assert, Action action, string? message = null) where TException : Exception
        {
            try
            {
                action();
                Fail(nameof(ThrowsException), string.IsNullOrEmpty(message)
                    ? $"Expected exception of type {typeof(TException).Name} to be thrown."
                    : $"Expected exception of type {typeof(TException).Name} to be thrown with message \"{message}\".");
            }
            catch (TException ex)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    if (message != ex.Message)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that the specified action throws an <see cref="InvalidPluginExecutionException"/>.
        /// Optionally checks the exception message.
        /// </summary>
        /// <param name="assert">The Assert instance.</param>
        /// <param name="action">The action expected to throw the exception.</param>
        /// <param name="message">Optional. The expected exception message.</param>
        public static void ThrowsPluginException(this Assert assert, Action action, string? message = null)
        {
            assert.ThrowsException<InvalidPluginExecutionException>(action, message);
        }

        /// <summary>
        /// Asserts that all provided values are not null or whitespace.
        /// </summary>
        /// <param name="assert">The Assert instance.</param>
        /// <param name="values">The values to check.</param>
        public static void IsNotNullOrWhiteSpace(this Assert assert, params object?[] values)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (values is null)
            {
                Fail(nameof(IsNotNullOrWhiteSpace), "No values were passed in.");
                return;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(values[i]?.ToString()))
                {
                    if (values.Length == 1)
                    {
                        Fail(nameof(IsNotNullOrWhiteSpace));
                    }

                    Fail(nameof(IsNotNullOrWhiteSpace), $"The parameter at index \"{i}\" was null or white space.");
                }
            }
        }


        /// <summary>
        /// Asserts that the attributes of two <see cref="Entity"/> instances are equal.
        /// </summary>
        /// <param name="assert">The Assert instance.</param>
        /// <param name="expected">The expected entity.</param>
        /// <param name="actual">The actual entity.</param>
        public static void AttributesAreEqual(this Assert assert, Entity? expected, Entity? actual)
        {
            var message = AssertHelper.AttributesAreEqual(expected, actual);
            // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (message == null)
            {
                return;
            }

            Fail(nameof(AttributesAreEqual), message);
        }

        #endregion Asserts

        #region IPlugin

        /// <summary>
        /// Executes the plugin and asserts that it throws an <see cref="InvalidPluginExecutionException"/>.
        /// Optionally checks the exception message.
        /// </summary>
        /// <param name="plugin">The plugin instance.</param>
        /// <param name="serviceProvider">The service provider for plugin execution.</param>
        /// <param name="message">Optional. The expected exception message.</param>
        public static void ExpectExecutionToThrowException(this IPlugin plugin, IServiceProvider serviceProvider, string? message = null)
        {
            Assert.That.ThrowsException<InvalidPluginExecutionException>(() => plugin.Execute(serviceProvider), message);
        }

        #endregion IPlugin

        #region String

        /// <summary>
        /// Throws an <see cref="AssertFailedException"/> with the specified name and message.
        /// </summary>
        /// <param name="name">The name of the assertion.</param>
        /// <param name="message">Optional. The failure message.</param>
#if DEBUGGER_HIDDEN
        [DebuggerHidden]
#endif
        private static void Fail(string name, string? message = null)
        {
            var msg = $"Assert.{name} failed.";
            if (!string.IsNullOrWhiteSpace(message))
            {
                msg += "  " + message;
            }

            throw new AssertFailedException(msg);
        }

        #endregion String
    }
}

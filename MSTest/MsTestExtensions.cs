using DLaB.Xrm;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;

using System;
using System.Collections.Generic;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

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
            // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (expected == null && actual == null)
            {
                return;
            }

            const string name = nameof(AttributesAreEqual);
            if (expected == null)
            {
                Fail(name, "Expected null but actual was not null.");
                return;
            }

            if (actual == null)
            {
                Fail(name, "Actual was null.");
                return;
            }
            // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

            foreach (var att in expected.Attributes)
            {
                if (actual.Contains(att.Key))
                {
                    var actualValue = actual[att.Key];
                    switch (att.Value)
                    {
                        case null when actualValue == null:
                            continue;
                        case null:
                            Fail(name, $"Expected attribute: \"{att.Key}\" to be null, but actual was {actualValue.GetDisplayValue()}.");
                            break;
                        default:
                            {
                                var errorMessage = AttributeIsEqual(actualValue, att);
                                if (errorMessage != null)
                                {
                                    Fail(name, errorMessage);
                                }

                                break;
                            }
                    }
                }
                else
                {
                    Fail(name, $"Attribute {att.Key} was expected but not found!");
                }
            }
        }

        /// <summary>
        /// Returns an exception string to fail if the single attribute value is not equal between expected and actual.
        /// </summary>
        /// <param name="actualValue">The actual value.</param>
        /// <param name="att">The expected attribute key-value pair.</param>
        [DebuggerHidden]
        private static string? AttributeIsEqual(object actualValue,  KeyValuePair<string, object> att)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (actualValue == null) 
            {
                return $"Expected attribute: \"{att.Key}\" with value: {att.Value.GetDisplayValue()} but actual was null.";
            }

            if (att.Value.Equals(actualValue))
            {
                return null;
            }

            if (actualValue is DateTime date)
            {
                // If hitting real database, and if the actual time value is midnight, assume that the Date Time is a Date Only field, and compare dates only:
                if (!TestBase.UseLocalCrmDatabase
                    && date == date.Date
                    && att.Value is DateTime expectedDate
                    && expectedDate.Date.Equals(date))
                {
                    return null;
                }

                return $"Expected attribute: \"{att.Key}\" with value: {GetDateTimeTicks((DateTime)att.Value)} but actual was {GetDateTimeTicks(date)}.";
            }

            return $"Expected attribute: \"{att.Key}\" with value: {att.Value.GetDisplayValue()} but actual was {actualValue.GetDisplayValue()}.";
        }

        /// <summary>
        /// Gets a string representation of a <see cref="DateTime"/> value including its ticks.
        /// </summary>
        /// <param name="date">The date value.</param>
        /// <returns>A string with the date, time, and ticks.</returns>
        private static string GetDateTimeTicks(DateTime date)
        {
            return $"\"{date.ToShortDateString()} {date.ToLongTimeString()}\" ({date.Ticks})";
        }

        /// <summary>
        /// Gets a display string for the specified object, with special handling for CRM types.
        /// </summary>
        /// <param name="obj">The object to display.</param>
        /// <returns>A string representation of the object.</returns>
        private static string GetDisplayValue(this object obj)
        {
            return obj switch
            {
                null => "null",
                Entity entity => entity.ToStringAttributes(),
                EntityReference entityRef => entityRef.ToStringDebug(),
                EntityCollection entities => entities.ToStringDebug(),
                EntityReferenceCollection entityRefCollection => entityRefCollection.ToStringDebug(),
                Dictionary<string, string> dict => dict.ToStringDebug(),
                byte[] imageArray => imageArray.ToStringDebug(),
                IEnumerable enumerable and not string => enumerable.ToStringDebug(),
                OptionSetValue optionSet => optionSet.Value.ToString(CultureInfo.InvariantCulture),
                Money money => money.Value.ToString(CultureInfo.InvariantCulture),
                bool yesNo => yesNo ? "true" : "false",
                _ => obj.IsNumeric() ? obj.ToString()! : $"\"{obj}\""
            };
        }

        /// <summary>
        /// Determines whether the specified object is a numeric type.
        /// </summary>
        /// <param name="o">The object to check.</param>
        /// <returns><c>true</c> if the object is numeric; otherwise, <c>false</c>.</returns>
        private static bool IsNumeric(this object o)
        {
            return o is byte
                or sbyte
                or ushort
                or uint
                or ulong
                or short
                or int
                or long
                or float
                or double
                or decimal;
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

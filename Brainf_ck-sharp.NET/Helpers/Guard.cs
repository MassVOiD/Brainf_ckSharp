﻿using System;
using System.Diagnostics;

namespace Brainf_ck_sharp.NET.Helpers
{
    /// <summary>
    /// A <see langword="class"/> that contains APIs to debug code
    /// </summary>
    [DebuggerStepThrough]
    internal static class Guard
    {
        /// <summary>
        /// Asserts that the input expression is <see langword="true"/>
        /// </summary>
        /// <param name="expression">The input expression to test</param>
        /// <param name="name">The name of the input parameter being tested</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="expression"/> is <see langword="false"/></exception>
        public static void MustBeTrue(bool expression, string name)
        {
            if (!expression) throw new ArgumentException($"Parameter {name} must be true, was false", name);
        }

        /// <summary>
        /// Asserts that the input value must be less than a specified value
        /// </summary>
        /// <typeparam name="T">The type of input values to compare</typeparam>
        /// <param name="value">The input <typeparamref name="T"/> value to test</param>
        /// <param name="max">The exclusive maximum <typeparamref name="T"/> value that is accepted</param>
        /// <param name="name">The name of the input parameter being tested</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is >= <paramref name="max"/></exception>
        public static void MustBeLessThan<T>(T value, T max, string name) where T : IComparable<T>
        {
            if (value.CompareTo(max) >= 0) throw new ArgumentOutOfRangeException(name, $"Parameter {name} must be < {max}, was {value}");
        }

        /// <summary>
        /// Asserts that the input value must be less than or equal to a specified value
        /// </summary>
        /// <typeparam name="T">The type of input values to compare</typeparam>
        /// <param name="value">The input <typeparamref name="T"/> value to test</param>
        /// <param name="maximum">The inclusive maximum <typeparamref name="T"/> value that is accepted</param>
        /// <param name="name">The name of the input parameter being tested</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is > <paramref name="maximum"/></exception>
        public static void MustBeLessThanOrEqualTo<T>(T value, T maximum, string name) where T : IComparable<T>
        {
            if (value.CompareTo(maximum) > 0) throw new ArgumentOutOfRangeException(name, $"Parameter {name} must be <= {maximum}, was {value}");
        }

        /// <summary>
        /// Asserts that the input value must be greater than a specified value
        /// </summary>
        /// <typeparam name="T">The type of input values to compare</typeparam>
        /// <param name="value">The input <typeparamref name="T"/> value to test</param>
        /// <param name="minimum">The exclusive minimum <typeparamref name="T"/> value that is accepted</param>
        /// <param name="name">The name of the input parameter being tested</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is &lt;= <paramref name="minimum"/></exception>
        public static void MustBeGreaterThan<T>(T value, T minimum, string name) where T : IComparable<T>
        {
            if (value.CompareTo(minimum) <= 0) throw new ArgumentOutOfRangeException(name, $"Parameter {name} must be > {minimum}, was {value}");
        }

        /// <summary>
        /// Asserts that the input value must be greater than or equal to a specified value
        /// </summary>
        /// <typeparam name="T">The type of input values to compare</typeparam>
        /// <param name="value">The input <typeparamref name="T"/> value to test</param>
        /// <param name="minimum">The inclusive minimum <typeparamref name="T"/> value that is accepted</param>
        /// <param name="name">The name of the input parameter being tested</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is &lt; <paramref name="minimum"/></exception>
        public static void MustBeGreaterThanOrEqualTo<T>(T value, T minimum, string name) where T : IComparable<T>
        {
            if (value.CompareTo(minimum) < 0) throw new ArgumentOutOfRangeException(name, $"Parameter {name} must be >= {minimum}, was {value}");
        }
    }
}
